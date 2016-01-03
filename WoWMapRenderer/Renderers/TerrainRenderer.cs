using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CASCExplorer;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Layers;
using System.Drawing;

namespace WoWMapRenderer.Renderers
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    class TerrainRenderer
    {
        private GLControl _control;

        private WDT _wdt;

        private Framebuffer _framebuffer;

        private Dictionary<int, ADT> _mapTiles = new Dictionary<int, ADT>();
        private Dictionary<int, TileRenderer> _batchRenderers = new Dictionary<int, TileRenderer>(9 * 3);
        private Dictionary<int, bool> _loadedTiles = new Dictionary<int, bool>();

        private Camera _camera;
        private Shader _shader;

        private BackgroundWorkerEx _loader;

        private Vector2 _currentCenteredTile = Vector2.Zero;

        public delegate void ProgressHandler(int progress, string state);
        public event ProgressHandler OnProgress;

        public bool ForceWireframe { get; private set; }

        private int[] _terrainSamplers = new int[4];
        private int[] _alphaTerrainSamplers = new int[3];

        public TerrainRenderer(GLControl control)
        {
            _control = control;
            _control.MouseClick += (sender, args) => {
                if (args.Button == MouseButtons.Right)
                    OnRightClick(UnprojectCoordinates(args.X, args.Y));
            };
            _control.KeyPress += (sender, args) =>
            {
                if (_camera != null)
                    _camera.Update();
            };
            _control.MouseMove += (sender, args) =>
            {
                if (_camera != null)
                    _camera.Update();
            };
        }

        ~TerrainRenderer()
        {
            /*if (_framebuffer != null)
                _framebuffer.Release();*/
        }

        public void OnForceWireframeToggle(bool forceWireframe)
        {
            ForceWireframe = forceWireframe;
        }

        /// <summary>
        /// Projects screen coordinates to mesh coordinates (2D -> 3D)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector3 UnprojectCoordinates(float x, float y)
        {
            var mouse = new Vector2(x, y);
            return mouse.UnProject(_camera.Projection, _camera.View, new Size(_control.Width, _control.Height)).ToVector3();
        }

        private void OnRightClick(Vector3 terrainCoordinates)
        {
            // 3D space coordinates passed as parameter
        }

        public void LoadMap(string mapName)
        {
            if (_loader == null)
            {
                _loader = new BackgroundWorkerEx();
                _loader.DoWork += (sender, e) =>
                {
                    _loader.ReportProgress(1, "Loading WDT...");
                    _wdt = new WDT(string.Format(@"World\Maps\{0}\{0}.wdt", mapName));
                    if (_wdt.IsGlobalModel)
                    {
                        _loader.ReportProgress(100, "This map is a global model, NYI !");
                        return;
                    }

                    _mapTiles.Clear();
                    int tileIdx = 0, tileCount = _wdt.TileCount;
                    /*for (var i = 20; i < 30; ++i)
                        for (var j = 20; j < 30; ++j)
                            if (_wdt.HasTile(i, j))
                            {
                                ++tileIdx;
                                _mapTiles[(i << 8) | j] = new ADT(mapName, i, j, _wdt);
                                _loader.ReportProgress(tileIdx * 100 / tileCount, "Loading ADTs (" + tileIdx + " / " + tileCount + ") ...");
                            }*/

                    if (!_wdt.HasTile(28, 28))
                        Console.WriteLine("fuck me");

                    _mapTiles[(28 << 8) | 28] = new ADT(mapName, 28, 28, _wdt);
                };
                _loader.ProgressChanged += (sender, args) =>
                {
                    if (OnProgress != null)
                        OnProgress(args.ProgressPercentage, (string)args.UserState);
                };
                _loader.RunWorkerCompleted += (sender, e) =>
                {
                    InitializeView();
                };
            }

            _loader.RunWorkerAsync();
        }

        private void InitializeView()
        {
            //_framebuffer = new Framebuffer(_control.Width, _control.Height);

            #region Generating samplers
            GL.GenSamplers(4, _terrainSamplers);
            GL.GenSamplers(3, _alphaTerrainSamplers);
            #endregion

            _shader = new Shader();
            _shader.CreateFromFile("shaders/vertex.glsl", "shaders/fragment.glsl");
            _shader.SetCurrent();

            _control.Resize += (sender, args) =>
            {
                GL.Viewport(0, 0, _control.Width, _control.Height);
                UpdateRenderers();
                Render();
            };
            _control.Paint += (sender, args) => { Render(); };

            // Find camera coordinates, set it, set viewport, load tiles, render.
            int x, y;
            GetCenterTile(out x, out y);
            var tileCenter = GetTileCenter(x, y);
            _currentCenteredTile = new Vector2(x, y);
            _camera = new Camera(new Vector3(tileCenter.X, tileCenter.Y, 300.0f), -Vector3.UnitZ);
            _camera.SetViewport(_control.Width, _control.Height);
            GL.Viewport(0, 0, _control.Width, _control.Height);

            _camera.OnMovement += () =>
            {
                UpdateRenderers();
                Render();
            };

            UpdateRenderers();
            Render();
        }

        private void UpdateRenderers()
        {
            var cameraPosition = _camera.Position;
            var centeredTile = GetTileAt(cameraPosition.Xy);
            Debug.Assert(centeredTile != Vector2.Zero, "Unable to determinate the tile in which the camera is!");
            if (_currentCenteredTile != centeredTile)
                _currentCenteredTile = centeredTile;

            var keysToKeep = new List<int>(9);

            /*for (var i = 1; i < 2; ++i)
            {
                for (var j = 1; j < 2; ++j)
                {
                    var tileX = (int)(_currentCenteredTile.X - 1 + i);
                    var tileY = (int)(_currentCenteredTile.Y - 1 + j);

                    if (IsTileLoaded(tileX, tileY) || !_wdt.HasTile(tileX, tileY))
                        continue;

                    keysToKeep.Add((tileX << 8) | tileY);

                    LoadTile(tileX, tileY);
                }
            }*/

            var tileX = 28;
            var tileY = 28;
            keysToKeep.Add((tileX << 8) | tileY);
            LoadTile(tileX, tileY);

            while (_loadedTiles.Count != 1)
            {
                var key = _loadedTiles.First(tile => !keysToKeep.Contains(tile.Key)).Key;
                _batchRenderers[key].Delete();
                _batchRenderers.Remove(key);
                _loadedTiles.Remove(key);
            }
        }

        private bool LoadTile(int tileX, int tileY)
        {
            var tileToLoadKey = (tileX << 8) | tileY;
            _mapTiles[tileToLoadKey].Read();

            _loadedTiles[tileToLoadKey] = true;

            var tileRenderer = new TileRenderer();
            tileRenderer.Generate(_mapTiles[tileToLoadKey]);
            tileRenderer.Bind(_shader);

            _batchRenderers[tileToLoadKey] = tileRenderer;
            return true;
        }

        /// <summary>
        /// Returns true if a map tile (ADT) has already been pre-generated; false otherwise.
        /// </summary>
        /// <param name="x">ADT.TilePosition.X</param>
        /// <param name="y">ADT.TilePosition.Y</param>
        /// <returns>'true' if tile has been generated, 'false' otherwise.</returns>
        private bool IsTileLoaded(int x, int y)
        {
            return _loadedTiles.ContainsKey((x << 8) | y) && _loadedTiles[(x << 8) | y];
        }

        /// <summary>
        /// Computes the coordinates of the tile a position is in.
        /// </summary>
        /// <param name="position">The position (X Y) to look for.</param>
        /// <returns>(X Y) corresponding to ADT.TilePosition.X and ADT.TilePosition.Y</returns>
        private Vector2 GetTileAt(Vector2 position)
        {
            foreach (var mapTile in _mapTiles)
            {
                var tilePosition = mapTile.Value.TilePosition.Yx;
                var tileBox = new Box2(tilePosition.X - Constants.TileSize, tilePosition.Y - Constants.TileSize, tilePosition.X, tilePosition.Y);
                if (!(position.X < tileBox.Right) || !(position.X > tileBox.Left))
                    continue;
                if (position.Y < tileBox.Bottom && position.Y > tileBox.Top)
                    return new Vector2(mapTile.Value.X, mapTile.Value.Y);
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// Returns 3D coordinates to the center of the tile.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Vector2 GetTileCenter(int x, int y)
        {
            var adt = _mapTiles[(x << 8) | y];
            var tilePosition = adt.TilePosition.Yx;
            tilePosition.X -= Constants.TileSize / 2;
            tilePosition.Y -= Constants.TileSize / 2;
            return tilePosition;
        }

        /// <summary>
        /// Returns the tile at the center of the map.
        /// </summary>
        private void GetCenterTile(out int x, out int y)
        {
            x = 28;
            y = 28;
            return; // HACK HACK HACK REMOVE ME LATER

            /*
            var topLeft = new[] { 64, 64 };
            var bottomRight = new[] { 0, 0 };
            for (var xx = 0; xx < 64; ++xx)
            {
                for (var yy = 0; yy < 64; ++yy)
                {
                    if (!_wdt.HasTile(xx, yy))
                        continue;

                    topLeft[0] = Math.Min(topLeft[0], xx);
                    topLeft[1] = Math.Min(topLeft[1], yy);
                    bottomRight[0] = Math.Max(bottomRight[0], xx);
                    bottomRight[1] = Math.Max(bottomRight[1], yy);
                }
            }

            x = (int)Math.Floor((topLeft[0] + bottomRight[0]) / 2.0f);
            y = (int)Math.Floor((topLeft[1] + bottomRight[1]) / 2.0f);*/
        }

        private void Render()
        {
            GL.ClearColor(Color.White);
            GL.Viewport(0, 0, _control.Width, _control.Height);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, ForceWireframe ? PolygonMode.Line : PolygonMode.Fill);

            var uniform = Matrix4.Mult(_camera.View, _camera.Projection);
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_modelview"), false, ref uniform);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            foreach (var renderer in _batchRenderers.Values)
                renderer.Render(_shader, _terrainSamplers, _alphaTerrainSamplers);

            _control.SwapBuffers();
        }
    }
}
