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

namespace WoWMapRenderer.Renderers
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
    class TerrainRenderer
    {
        private GLControl _control;

        private WDT _wdt;

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

        public TerrainRenderer(GLControl control)
        {
            _control = control;
            _control.MouseClick += (sender, args) => {
                if (args.Button == MouseButtons.Right)
                    OnRightClick(ProjectCoordinates(args.X, args.Y));
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
        private Vector3 ProjectCoordinates(float x, float y)
        {
            // Translate click coord to 3D.
            // Fire a ray from there and detect hits
            // Not sure OpenTK has an easy way to do this ...
            return Vector3.One;
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
                    for (var i = 0; i < 64; ++i)
                        for (var j = 0; j < 64; ++j)
                            if (_wdt.HasTile(i, j))
                            {
                                ++tileIdx;
                                _mapTiles[(i << 8) | j] = new ADT(mapName, i, j);
                                _loader.ReportProgress(tileIdx * 100 / tileCount, "Loading ADTs (" + tileIdx + " / " + tileCount + ") ...");
                            }
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
                // Camera already updated
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

            for (var i = 0; i < 1; ++i)
            {
                for (var j = 0; j < 1; ++j)
                {
                    var tileX = (int)(_currentCenteredTile.X - 1 + i);
                    var tileY = (int)(_currentCenteredTile.Y - 1 + j);

                    if (IsTileLoaded(tileX, tileY) || !_wdt.HasTile(tileX, tileY))
                        continue;

                    keysToKeep.Add((tileX << 8) | tileY);

                    LoadTile(tileX, tileY);
                }
            }

            while (_loadedTiles.Count != 1)
            {
                var key = _loadedTiles.First(tile => !keysToKeep.Contains(tile.Key)).Key;
                _batchRenderers[key].Delete();
                _batchRenderers.Remove(key);
                _loadedTiles.Remove(key);
            }
        }

        private void LoadTile(int tileX, int tileY)
        {
            var tileToLoadKey = (tileX << 8) | tileY;
            _mapTiles[tileToLoadKey].Read();

            foreach (var t in _mapTiles[tileToLoadKey].Textures.MTEX.Filenames)
                TextureCache.AddRawTexture(t.Value);

            _loadedTiles[tileToLoadKey] = true;

            var verticeList = new List<Vertex>(145);
            var indiceList = new List<uint>(8 * 8 * 4 * 3);

            var tileRenderer = new TileRenderer();
            tileRenderer.Generate(_mapTiles[tileToLoadKey]);
            tileRenderer.Bind(_shader);

            // Add textures per MapChunkRenderer here

            _batchRenderers[tileToLoadKey] = tileRenderer;
        }

        /*private MapChunkRenderer BindIndexedVertex(int mapChunkIndex, ADT terrainTile, Vertex[] vertices, uint[] indices)
        {
            var renderer = new MapChunkRenderer { TriangleCount = indices.Length };

            // Schlumpf guarantees the chunks are "always in the exactly same order". You know who to blame.
            var mapChunk = terrainTile.MapChunks[mapChunkIndex];
            var texMapChunk = terrainTile.Textures.MapChunks[mapChunkIndex];

            for (var i = 0; i < texMapChunk.MCLY.Length; ++i)
            {
                // TODO: This still doesnt work for a LOT of tiles
                if (texMapChunk.MCLY[i] == null || texMapChunk.MCAL == null)
                    continue;

                if (TextureCache.Unit > 10)
                    break;

                var rawTexture = TextureCache.GetRawTexture(terrainTile.Textures.MTEX.Filenames.ElementAt((int)texMapChunk.MCLY[i].TextureId).Value);
                var newTexture = rawTexture.ApplyAlpha(texMapChunk.MCAL.GetAlpha(i));

                if (TextureCache.AddBoundTexture(newTexture))
                    renderer.AddTexture(newTexture);
            }

            GL.BindTexture(TextureTarget.Texture2D, 0); // Release textures

            GL.BindVertexArray(renderer.VAO);

            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = vertices.Length * vertexSize;

            GL.BindBuffer(BufferTarget.ArrayBuffer, renderer.VerticeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), vertices, BufferUsageHint.StaticDraw);

            VertexAttribPointer(_shader.GetAttribLocation("vertex_shading"), 3,
                VertexAttribPointerType.Float, vertexSize, IntPtr.Zero, true);

            VertexAttribPointer(_shader.GetAttribLocation("vertice_position"), 3,
                VertexAttribPointerType.Float, vertexSize, sizeof(float) * 3);

            VertexAttribPointer(_shader.GetAttribLocation("in_TexCoord0"), 2,
                VertexAttribPointerType.Float, vertexSize, (IntPtr)(sizeof(float) * 6));

            

            return renderer;
        }

        private void VertexAttribPointer(int location, int size, VertexAttribPointerType type, int stride, IntPtr offset, bool normalized = false)
        {
            GL.VertexAttribPointer(location, size, type, normalized, stride, offset);
            GL.EnableVertexAttribArray(location);
        }

        private void VertexAttribPointer(int location, int size, VertexAttribPointerType type, int stride, int offset)
        {
            GL.VertexAttribPointer(location, size, type, false, stride, offset);
            GL.EnableVertexAttribArray(location);
        }*/

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
            // var buffer = new FrameBuffer(512, 512);
            // buffer.Load();

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, ForceWireframe ? PolygonMode.Line : PolygonMode.Fill);

            var uniform = Matrix4.Mult(_camera.View, _camera.Projection);
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_modelview"), false, ref uniform);

            GL.Enable(EnableCap.Texture2D);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            foreach (var renderer in _batchRenderers.Values)
                renderer.Render(_shader);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            _control.SwapBuffers();
        }
    }
}
