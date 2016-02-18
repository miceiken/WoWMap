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

        private string _mapName;
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
            _control.MouseClick += (sender, args) =>
            {
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
            if (_camera == null)
                return Vector3.Zero;
            var mouse = new Vector2(x, y);
            var mat4 = mouse.UnProject(_camera.Projection, _camera.View, new Size(_control.Width, _control.Height));
            return mat4.Xyz;
        }

        private void OnRightClick(Vector3 terrainCoordinates)
        {
            // 3D space coordinates passed as parameter
            // TODO fix, not working as planned
            Console.WriteLine($"Clicked coordinates [ {terrainCoordinates.X} {terrainCoordinates.Y} {terrainCoordinates.Z} ]");
        }

        public void LoadMap(string mapName)
        {
            if (_loader == null)
            {
                _loader = new BackgroundWorkerEx();
                _loader.DoWork += (sender, e) =>
                {
                    _mapName = mapName;
                    _loader.ReportProgress(1, "Loading WDT...");
                    _wdt = new WDT(string.Format(@"World\Maps\{0}\{0}.wdt", mapName));
                    if (_wdt.IsGlobalModel)
                    {
                        _loader.ReportProgress(100, "This map is a global model, NYI !");
                        return;
                    }

                    _mapTiles.Clear();
                    int x, y;
                    GetSpawnTile(out x, out y);

                    if (!_wdt.HasTile(x, y))
                        Console.WriteLine("fuck me");

                    _mapTiles[(x << 8) | y] = new ADT(mapName, x, y, _wdt);
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
            // GL.GenSamplers(4, _terrainSamplers);
            // GL.GenSamplers(3, _alphaTerrainSamplers);
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
            GetSpawnTile(out x, out y);
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
            var _currentCenteredTile = GetTileAt(cameraPosition.Xy);
            Debug.Assert(_currentCenteredTile != Vector2.Zero, "Unable to determinate the tile in which the camera is!");

            var keysToKeep = new List<int>(9);

            var tileX = (int)Math.Floor(_currentCenteredTile.X);
            var tileY = (int)Math.Floor(_currentCenteredTile.Y);

            if (LoadTile(tileX, tileY))
                keysToKeep.Add((tileX << 8) | tileY);

            //while (_loadedTiles.Count != 1)
            //{
            //    var key = _loadedTiles.First(tile => !keysToKeep.Contains(tile.Key)).Key;
            //    _batchRenderers[key].Delete();
            //    _batchRenderers.Remove(key);
            //    _loadedTiles.Remove(key);
            //}
        }

        private bool LoadTile(int tileX, int tileY)
        {
            if (IsTileLoaded(tileX, tileY))
                return true;
            if (!_wdt.HasTile(tileX, tileY))
                return false;

            Debug.WriteLine($"Loading {tileX}, {tileY} -- already loaded? {IsTileLoaded(tileX, tileY)}");

            var tileToLoadKey = (tileX << 8) | tileY;
            _loadedTiles[tileToLoadKey] = true;
            if (!_mapTiles.ContainsKey(tileToLoadKey))
                _mapTiles[tileToLoadKey] = new ADT(_mapName, tileX, tileY, _wdt);
            _mapTiles[tileToLoadKey].Read();

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
            return new Vector2((Constants.MaxXY - position.X) / Constants.TileSize, (Constants.MaxXY - position.Y) / Constants.TileSize);
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
            var tilePosition = new Vector2((32 - x) * Constants.TileSize, (32 - y) * Constants.TileSize);
            tilePosition.X -= Constants.TileSize / 2;
            tilePosition.Y -= Constants.TileSize / 2;
            return tilePosition;
        }

        /// <summary>
        /// Returns the tile at the center of the map.
        /// </summary>
        private void GetSpawnTile(out int x, out int y)
        {
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
            y = (int)Math.Floor((topLeft[1] + bottomRight[1]) / 2.0f);
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
