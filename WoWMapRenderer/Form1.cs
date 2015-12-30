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

namespace WoWMapRenderer
{
    public partial class Form1 : Form
    {
        private BackgroundWorkerEx _cascAction;
        private BackgroundWorkerEx _mapAction;

        private DBC<MapRecord> _mapRecords;
        private DBC<AreaTableRecord> _areaTableRecords;
        private DB2<AreaAssignmentRecord> _areaAssignmentRecords;

        private string _localCascPath = string.Empty;

        private TerrainRenderer _renderer;

        public Form1()
        {
            InitializeComponent();
            TextureCache.Initialize();

            _renderer = new TerrainRenderer(GL);
            _renderer.OnProgress += (progress, state) =>
            {
                _backgroundTaskProgress.Value = progress;
                _feedbackText.Text = state;
            };

            /*int maxCombinedTextureImageUnits;
            OpenGL.GL.GetInteger(OpenGL.GetPName.MaxCombinedTextureImageUnits, out maxCombinedTextureImageUnits);
            int maxTextureImageUnits;
            OpenGL.GL.GetInteger(OpenGL.GetPName.MaxTextureImageUnits, out maxTextureImageUnits);

            openGLInformationsToolStripMenuItem.DropDownItems.Add($"GL_MAX_COMBINED_TEXTURE_IMAGE_UNITS = {maxCombinedTextureImageUnits}");
            openGLInformationsToolStripMenuItem.DropDownItems.Add($"GL_MAX_TEXTURE_IMAGE_UNITS = {maxTextureImageUnits}");*/
        }

        private void OnLoad(object obj, EventArgs ea)
        {
            _cascAction = new BackgroundWorkerEx();
            _cascAction.DoWork += (sender, e) => {
                if (string.IsNullOrEmpty(_localCascPath))
                    CASC.InitializeOnline(_cascAction);
                else {
                    try {
                        CASC.Initialize(_localCascPath);
                    } catch (Exception /* ex */) {
                        MessageBox.Show("Path '" + _localCascPath + "/Data' was not found.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            };
            _cascAction.ProgressChanged += (sender, e) => {
                _feedbackText.Text = (string)e.UserState;
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = e.ProgressPercentage;
            };
            _cascAction.RunWorkerCompleted += (sender, e) => {
                if (CASC.Initialized)
                    _mapAction.RunWorkerAsync();
            };

            _mapAction = new BackgroundWorkerEx();
            _mapAction.DoWork += (sender, e) => {
                _cascAction.ReportProgress(0, "Loading maps ...");

                _areaTableRecords = new DBC<AreaTableRecord>(@"DBFilesClient\AreaTable.dbc");
                _mapRecords = new DBC<MapRecord>(@"DBFilesClient\Map.dbc");
                _areaAssignmentRecords = new DB2<AreaAssignmentRecord>(@"DBFilesClient\AreaAssignment.db2");

                var rowIndex = 0;
                foreach (var mapEntry in _mapRecords.Rows)
                {
                    _mapAction.ReportProgress(++rowIndex * 100 / _mapRecords.Rows.Length, mapEntry);
                }
            };
            _mapAction.ProgressChanged += (sender, e) => 
            {
                _feedbackText.Text = "Loading maps ...";
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = e.ProgressPercentage;

                var mapEntry = (MapRecord)e.UserState;

                _mapListBox.Items.Add(new MapListBoxEntry
                {
                    Name = mapEntry.MapNameLang,
                    Directory = mapEntry.Directory,
                    MapID = mapEntry.ID
                });
            };
            _mapAction.RunWorkerCompleted += (sender, e) =>
            {
                _feedbackText.Text = "Maps loaded.";
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
                /*AreaTableRecord parentRecord = null;
                if (record.ParentAreaID != 0)
                    parentRecord = _areaTableRecords.Rows.ToList().First(k => k.ID == record.ParentAreaID);

                while (parentRecord.ParentAreaID != 0)
                    parentRecord = _areaTableRecords.Rows.ToList().First(k => k.ID == parentRecord.ParentAreaID);*/

                if (record.ContinentID == entry.MapID)
                    _areaListBox.Items.Add(new AreaListBoxItem(record));
            }

            _renderer.LoadMap(entry.Directory);
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
            if (_renderer != null)
            {
                forceWireframeToolStripMenuItem.Checked = !forceWireframeToolStripMenuItem.Checked;
                _renderer.OnForceWireframeToggle(forceWireframeToolStripMenuItem.Checked);
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
