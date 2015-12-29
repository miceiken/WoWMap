using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using CASCExplorer;
using WoWMap.Archive;
using WoWMapRenderer.Renderers;
using OpenGL = OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    public partial class Form1 : Form
    {
        private BackgroundWorkerEx _cascAction;
        private BackgroundWorkerEx _mapAction;

        private DBC<MapRecord> _mapRecords;

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

                _mapRecords = new DBC<MapRecord>(@"DBFilesClient\Map.dbc");

                var rowIndex = 0;
                foreach (var mapEntry in _mapRecords.Rows)
                {
                    _mapAction.ReportProgress(++rowIndex * 100 / _mapRecords.Rows.Length, new [] {
                        mapEntry.MapNameLang, mapEntry.Directory
                    });
                }
            };
            _mapAction.ProgressChanged += (sender, e) => 
            {
                _feedbackText.Text = "Loading maps ...";
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = e.ProgressPercentage;

                _mapListBox.Items.Add(new MapListBoxEntry
                {
                    Name = ((string[])e.UserState)[0],
                    Directory = ((string[])e.UserState)[1],
                });
            };
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            // NYI
        }

        private void MapSelected(object sender, EventArgs e)
        {
            var entry = (MapListBoxEntry)_mapListBox.SelectedItem;
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
    }

    internal struct MapListBoxEntry
    {
        public string Name;
        public string Directory;

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
