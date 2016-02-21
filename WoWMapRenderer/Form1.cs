using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CASCExplorer;
using WoWMap.Archive;
using WoWMapRenderer.Renderers;
using OpenGL = OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;
using WoWMap.Geometry;

namespace WoWMapRenderer
{
    public partial class Form1 : Form
    {
        private BackgroundWorkerEx _cascAction;
        private BackgroundWorkerEx _dbcAction;

        private int _dbcIndex = 0;
        private DBC<MapRecord> _mapRecords;
        private DBC<AreaTableRecord> _areaTableRecords;
        private DB2<AreaAssignmentRecord> _areaAssignmentRecords;

        private string _localCascPath = string.Empty;

        private RenderView _view;

        public Form1()
        {
            InitializeComponent();

            _view = new RenderView(GL);
            //_renderer = new TerrainRenderer(GL);
            //_renderer.OnProgress += (progress, state) =>
            //{
            //    _backgroundTaskProgress.Value = progress;
            //    _feedbackText.Text = state;
            //};
        }

        private void OnLoad(object obj, EventArgs ea)
        {
            _cascAction = new BackgroundWorkerEx();
            _cascAction.DoWork += (sender, e) =>
            {
                if (string.IsNullOrEmpty(_localCascPath))
                    CASC.InitializeOnline(_cascAction);
                else {
                    try
                    {
                        CASC.Initialize(_localCascPath, _cascAction);
                    }
                    catch (Exception /* ex */)
                    {
                        MessageBox.Show("Path '" + _localCascPath + "/Data' was not found.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            };
            _cascAction.ProgressChanged += (sender, e) =>
            {
                _feedbackText.Text = (string)e.UserState;
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = e.ProgressPercentage;
            };
            _cascAction.RunWorkerCompleted += (sender, e) =>
            {
                if (CASC.Initialized)
                    _dbcAction.RunWorkerAsync();
            };

            _dbcAction = new BackgroundWorkerEx();
            _dbcAction.DoWork += (sender, e) =>
            {
                switch (_dbcIndex)
                {
                    case 0:
                        _mapRecords = new DBC<MapRecord>(@"DBFilesClient\Map.dbc", _dbcAction);
                        break;
                    case 1:
                        _areaTableRecords = new DBC<AreaTableRecord>(@"DBFilesClient\AreaTable.dbc", _dbcAction);
                        break;
                    case 2:
                        // _areaAssignmentRecords = new DB2<AreaAssignmentRecord>(@"DBFilesClient\AreaAssignment.db2");
                        break;
                }
            };
            _dbcAction.ProgressChanged += (sender, e) =>
            {
                _feedbackText.Text = string.Format(@"Loading {0} ...", (new[] { "maps", "areas" })[_dbcIndex]);
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = e.ProgressPercentage;

                switch (_dbcIndex)
                {
                    case 0:
                        var mapEntry = (MapRecord)e.UserState;
                        _mapListBox.Items.Add(new MapListBoxEntry
                        {
                            Name = mapEntry.MapNameLang,
                            Directory = mapEntry.Directory,
                            MapID = mapEntry.ID
                        });
                        break;
                    case 1:
                        // Don't populate yet
                        break;
                }
            };
            _dbcAction.RunWorkerCompleted += (sender, e) =>
            {
                _feedbackText.Text = string.Format(@"{0} loaded.", (new[] { "Maps", "Areas" })[_dbcIndex]);
                ++_dbcIndex;
                if (_dbcIndex < 2)
                    _dbcAction.RunWorkerAsync();

            };
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            // NYI
        }

        private void MapSelected(object sender, EventArgs e)
        {
            var entry = (MapListBoxEntry)_mapListBox.SelectedItem;
            _areaListBox.Items.Clear();

            for (var i = 0; i < _areaTableRecords.Rows.Length; ++i)
            {
                var record = _areaTableRecords.Rows[i];
                if (record.ContinentID == entry.MapID)
                    _areaListBox.Items.Add(new AreaListBoxItem(record));
            }
            _view.LoadMap(entry.Directory);
        }

        private void LoadOnlineCASC(object sender, EventArgs e)
        {
            _localCascPath = string.Empty;
            _cascAction.RunWorkerAsync();
        }

        private void LoadLocalCASC(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Indicate the path to your World of Warcraft installation.",
                ShowNewFolderButton = false
            };
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            _localCascPath = dialog.SelectedPath;
            _cascAction.RunWorkerAsync();
        }

        private void OnForceWireframeToggle(object sender, EventArgs e)
        {
            if (_view != null)
            {
                forceWireframeToolStripMenuItem.Checked = !forceWireframeToolStripMenuItem.Checked;
                _view.Options[RenderView.RenderOptions.ForceWireframe] = forceWireframeToolStripMenuItem.Checked;
            }
        }

        private void OnAreaSelected(object sender, EventArgs e)
        {
            var entry = (AreaListBoxItem)_areaListBox.SelectedItem;
            var x = _areaAssignmentRecords.Rows.ToList().Where(k => k.MapID == entry.MapId);
            var locs = _areaAssignmentRecords.Rows.ToList().Where(k => k.AreaID == entry.AreaID);
            Debug.Assert(locs.Count() == 1, $"Expected one AreaAssignment record for AreaID {entry.AreaID}, found {locs.Count()}");
            _feedbackText.Text = $"Chunk [ {locs.First().ChunkX} {locs.First().ChunkY} ]";
        }

        private void drawTerraintoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_view == null) return;
            drawTerraintoolStripMenuItem.Checked = !drawTerraintoolStripMenuItem.Checked;
            _view.DrawMeshTypeEnabled[MeshType.Terrain] = drawTerraintoolStripMenuItem.Checked;
        }

        private void drawWMOtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_view == null) return;
            drawWMOtoolStripMenuItem.Checked = !drawWMOtoolStripMenuItem.Checked;
            _view.DrawMeshTypeEnabled[MeshType.WorldModelObject] = drawWMOtoolStripMenuItem.Checked;
        }

        private void drawDoodadtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_view == null) return;
            drawDoodadtoolStripMenuItem.Checked = !drawDoodadtoolStripMenuItem.Checked;
            _view.DrawMeshTypeEnabled[MeshType.Doodad] = drawDoodadtoolStripMenuItem.Checked;
        }

        private void drawLiquidtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_view == null) return;
            drawLiquidtoolStripMenuItem.Checked = !drawLiquidtoolStripMenuItem.Checked;
            _view.DrawMeshTypeEnabled[MeshType.Liquid] = drawLiquidtoolStripMenuItem.Checked;
        }
    }

    internal struct MapListBoxEntry
    {
        public string Name;
        public string Directory;
        public int MapID;

        public override string ToString()
        {
            return Name;
        }
    }

    internal struct AreaListBoxItem
    {
        public string Name;
        public int AreaID;
        public int MapId;

        public AreaListBoxItem(AreaTableRecord record)
        {
            Name = record.AreaName;
            AreaID = record.ID;
            MapId = record.ContinentID;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /*internal struct MapChunkRenderer
    {
        public int IndiceVBO;
        public int VertexVBO;
        public int VAO;
        public int TextureSampler0;
        public int TextureSampler1;
        public int TextureSampler2;
        public int TextureSampler3;
        public int AlphaSampler;
        public int[] TextureIDs;
        public int TriangleCount;
        public Texture AlphaTexture;
    }*/
}
