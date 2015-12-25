using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CASCExplorer;
using WoWMap.Archive;
using WoWMap.Layers;
using OpenTK;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Geometry;
using OpenGL = OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    internal enum VerticeType : short
    {
        Terrain = 0,
        WMO = 1,
        Doodad = 2
    }

    public partial class Form1 : Form
    {
        private AsyncAction _cascAction;
        private AsyncAction _mapAction;

        private DBC<MapRecord> _mapRecords;

        private string _localCascPath = string.Empty;

        private TerrainRenderer _renderer;

        public Form1()
        {
            InitializeComponent();
            _renderer = new TerrainRenderer(GL);
            _renderer.OnProgress += (progress, state) =>
            {
                _backgroundTaskProgress.Value = progress;
                _feedbackText.Text = state;
            };
        }

        private void OnLoad(object sender, EventArgs e)
        {
            _cascAction = new AsyncAction(() => {
                if (string.IsNullOrEmpty(_localCascPath))
                    CASC.InitializeOnline(_cascAction);
                else
                {
                    try {
                        CASC.Initialize(_localCascPath);
                    } catch (Exception ex) {
                        MessageBox.Show("Path '" + _localCascPath + "/Data' was not found.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }, args =>
            {
                _feedbackText.Text = (string)args.UserData;
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = args.Progress;
            });

            _mapAction = new AsyncAction(() => {
                _cascAction.ReportProgress(0, "Loading maps ...");

                _mapRecords = new DBC<MapRecord>(@"DBFilesClient\Map.dbc");

                _mapListBox.Invoke(new Action(() =>
                {
                    var rowIndex = 0;
                    foreach (var mapEntry in _mapRecords.Rows)
                    {
                        ++rowIndex;
                        _mapAction.ReportProgress(rowIndex * 100 / _mapRecords.Rows.Length);

                        _mapListBox.Items.Add(new MapListBoxEntry
                        {
                            Name = mapEntry.MapNameLang,
                            Directory = mapEntry.Directory
                        });
                    }
                }));
            }, args =>
            {
                _feedbackText.Text = (string)args.UserData;
                _backgroundTaskProgress.Style = ProgressBarStyle.Continuous;
                _backgroundTaskProgress.Maximum = 100;
                _backgroundTaskProgress.Value = args.Progress;
            });
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            // NYI
        }

        private async void MapSelected(object sender, EventArgs e)
        {
            var entry = (MapListBoxEntry)_mapListBox.SelectedItem;
            _renderer.LoadMap(entry.Directory);
        }

        private async void LoadOnlineCASC(object sender, EventArgs e)
        {
            _localCascPath = string.Empty;
            await _cascAction.DoAction();
            if (CASC.Initialized)
                await _mapAction.DoAction();
        }

        private async void LoadLocalCASC(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Indicate the path to your World of Warcraft installation.",
                ShowNewFolderButton = false
            };
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            _localCascPath = dialog.SelectedPath;
            await _cascAction.DoAction();
            if (CASC.Initialized)
                await _mapAction.DoAction();
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

    [StructLayout(LayoutKind.Sequential)]
    internal struct Vertex
    {
        public Vector3 Color;
        public Vector3 Position;
        public VerticeType Type;
        // public Vector2 TextureCoordinates;
    }

    internal struct Renderer
    {
        public int TileHash;
        public int IndiceVBO;
        public int VertexVBO;
        public int VAO;
        public int TriangleCount;
    }
}
