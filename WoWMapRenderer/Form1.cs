using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CASCExplorer;
using WoWMap.Archive;
using WoWMap.Layers;
using OpenTK;
using WoWMap.Geometry;
using OpenGL = OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    public partial class Form1 : Form
    {
        private AsyncAction _cascAction;
        private AsyncAction _mapAction;
        private AsyncAction _adtAction;

        private string _wdtPath;
        private WDT _wdt;
        private Dictionary<int, ADT> _adts = new Dictionary<int, ADT>();
        private DBC<MapRecord> _mapRecords;

        private GeometryLoader _geometry = new GeometryLoader();

        private Camera Camera;

        public Form1()
        {
            InitializeComponent();
        }

        private void OnRenderLoaded(object sender, EventArgs e)
        {
            OpenGL.GL.GenBuffers(1, out _geometry.IndiceBuffer);
            OpenGL.GL.GenBuffers(1, out _geometry.VertexBuffer);

            OpenGL.GL.BindBuffer(OpenGL.BufferTarget.ArrayBuffer, _geometry.VertexBuffer);
            OpenGL.GL.BindBuffer(OpenGL.BufferTarget.ElementArrayBuffer, _geometry.IndiceBuffer);
            OpenGL.GL.VertexPointer(3, OpenGL.VertexPointerType.Float, 0, 0);
            OpenGL.GL.EnableClientState(OpenGL.ArrayCap.VertexArray);

            Camera = new Camera(GL.Width, GL.Height);

            var projection = Camera.Projection;
            OpenGL.GL.MatrixMode(OpenGL.MatrixMode.Projection);
            OpenGL.GL.LoadMatrix(ref projection);

        }

        private void OnLoad(object sender, EventArgs e)
        {
            _cascAction = new AsyncAction(() => {
                CASC.InitializeOnline(_cascAction);
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

            _adtAction = new AsyncAction(() =>
            {
                try
                {
                    _wdt = new WDT(_wdtPath);
                    if (_wdt.IsGlobalModel)
                        return; // NYI

                    var tileCount = _wdt.TileCount;
                    var tileIndex = 0;
                    var adt = new ADT(Path.GetFileNameWithoutExtension(_wdtPath), 28, 28);
                    _adts.Add((28 << 8) | 28, adt);
                    adt.Read();
                    _adtAction.ReportProgress(100, "Loading ADTs ...");
                    /*for (var i = 0; i < 64; ++i)
                    {
                        for (var j = 0; j < 64; ++j)
                        {
                            if (_wdt.HasTile(i, j))
                            {
                                _adts.Add((i << 8) | j, new ADT(Path.GetFileNameWithoutExtension(_wdtPath), i, j));
                                ++tileIndex;
                            }
                            _adtAction.ReportProgress(tileIndex * 100 / tileCount, "Loading ADTs ...");
                        }
                    }*/
                }
                catch (Exception ex)
                {
                    _adtAction.ReportProgress(100, "Error when loading ADTs ...");
                }
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
            var entry = (MapListBoxEntry)_mapListBox.Items[_mapListBox.SelectedIndex];
            _backgroundTaskProgress.Style = ProgressBarStyle.Marquee;

            _wdtPath = string.Format(@"World\Maps\{0}\{0}.wdt", entry.Directory);
            await _adtAction.DoAction();
            _feedbackText.Text = string.Format("{0} ADTs loaded!", _adts.Count);

            Cleanup();
            Render();
        }

        private void Cleanup()
        {
            
        }

        private void Render()
        {
            // TODO Make this kinda modifiable
            /*var centerTile = new[] { 28, 28 };
            for (var x = centerTile[0] - 1; x <= centerTile[0] + 1; ++x)
            {
                for (var y = centerTile[1] - 1; y <= centerTile[1] + 1; ++y)
                {
                    var adt = _adts[(x << 8) | y];
                    _geometry.AddADT(adt);
                }
            }*/

            _geometry.AddADT(_adts[(28 << 8) | 28]);

            OpenGL.GL.BindBuffer(OpenGL.BufferTarget.ElementArrayBuffer, _geometry.IndiceBuffer);
            OpenGL.GL.BufferData(OpenGL.BufferTarget.ElementArrayBuffer, (IntPtr)(_geometry.Indices.Length * sizeof(uint)), _geometry.Indices, OpenGL.BufferUsageHint.DynamicDraw);

            // TODO Change when we add textures
            // OpenGL.GL.TexCoordPointer(0, OpenGL.TexCoordPointerType.Float, 3 * sizeof(float), IntPtr.Zero);
            // OpenGL.GL.NormalPointer(OpenGL.NormalPointerType.Float, 3 * sizeof(float), IntPtr.Zero);
            // OpenGL.GL.VertexPointer(3, OpenGL.VertexPointerType.Float, 3 * sizeof(float), (IntPtr)(5 * sizeof(float)));

            OpenGL.GL.BindBuffer(OpenGL.BufferTarget.ArrayBuffer, _geometry.VertexBuffer);
            OpenGL.GL.BufferData(OpenGL.BufferTarget.ArrayBuffer, (IntPtr)(_geometry.Vertices.Length * sizeof(float)), _geometry.Vertices, OpenGL.BufferUsageHint.DynamicDraw);

            OpenGL.GL.DrawArrays(OpenGL.PrimitiveType.Triangles, 0, _geometry.Vertices.Length);

            GL.SwapBuffers();
            
        }

        private async void LoadOnlineCASC(object sender, EventArgs e)
        {
            await _cascAction.DoAction();
            await _mapAction.DoAction();
        }

        private async void LoadLocalCASC(object sender, EventArgs e)
        {
            await _cascAction.DoAction();
            await _mapAction.DoAction();
        }

        private void OnKeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            Camera.Update();
        }

        private void OnRenderResize(object sender, EventArgs e)
        {
            OpenGL.GL.Viewport(0, 0, GL.Width, GL.Height);

            if (Camera == null)
                return;

            var projection = Camera.Projection;
            OpenGL.GL.MatrixMode(OpenGL.MatrixMode.Projection);
            OpenGL.GL.LoadMatrix(ref projection);

            Render();
        }
    }

    struct MapListBoxEntry
    {
        public string Name;
        public string Directory;

        public override string ToString()
        {
            return Name;
        }
    }
}
