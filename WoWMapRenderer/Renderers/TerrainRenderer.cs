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
        private FrameBuffer _textureCache;

        private Camera _camera;
        private Shader _shader;

        private BackgroundWorkerEx _loader;

        private Vector2 _currentCenteredTile = Vector2.Zero;

        public delegate void ProgressHandler(int progress, string state);

        public event ProgressHandler OnProgress;

        private int[] _terrainBuffer = new int[9];

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
                _loader = new AsyncAction(() =>
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
                                _loader.ReportProgress(tileIdx * 100 / tileCount, "Loading ADTs (" + tileIdx + " / " + tileCount +") ...");
                            }
                }, args =>
                {
                    if (OnProgress != null)
                        OnProgress(args.Progress, (string)args.UserData);

                    if (_wdt == null || _wdt.TileCount != _mapTiles.Count)
                        return;

                    InitializeView();
                });

            _loader.DoAction(); // Not awaited, not blocking
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

            _textureCache = new FrameBuffer(_control.Width, _control.Height);

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

                    keysToKeep.Add((tileX << 8) | tileY);

                    if (IsTileLoaded(tileX, tileY) || !_wdt.HasTile(tileX, tileY))
                        continue;

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
                _textureCache.AddTexture((int)t.Key, t.Value);

            _loadedTiles[tileToLoadKey] = true;

            var verticeList = new List<Vertex>(145);
            var indiceList = new List<uint>();

            var tileRenderer = new TileRenderer();

            foreach (var adtChunk in _mapTiles[tileToLoadKey].MapChunks)
            {
                if (adtChunk == null)
                    continue;

                var offset = (uint)verticeList.Count;

                #region Terrain indices
                // Generate indices
                var unitidx = 0;
                for (uint j = 9; j < 8 * 8 + 9 * 8; j++)
                {
                    if (!adtChunk.HasHole(unitidx % 8, unitidx++ / 8))
                    {
                        indiceList.AddRange(new[] { j + offset, j - 9 + offset, j + 8 + offset });
                        indiceList.AddRange(new[] { j + offset, j - 8 + offset, j - 9 + offset });
                        indiceList.AddRange(new[] { j + offset, j + 9 + offset, j - 8 + offset });
                        indiceList.AddRange(new[] { j + offset, j + 8 + offset, j + 9 + offset });
                    }
                    if ((j + 1) % (9 + 8) == 0) j += 9;
                }
                #endregion

                #region Terrain vertices
                for (int i = 0, idx = 0; i < 17; ++i)
                {
                    var maxJ = ((i % 2) != 0) ? 8 : 9;
                    for (var j = 0; j < maxJ; j++)
                    {
                        var color = new Vector3(1.0f, 1.0f, 1.0f);
                        if (adtChunk.MCCV != null)
                        {
                            color.X = adtChunk.MCCV.Entries[idx].Blue / 127.0f;
                            color.Y = adtChunk.MCCV.Entries[idx].Green / 127.0f;
                            color.Z = adtChunk.MCCV.Entries[idx].Red / 127.0f;
                        }

                        verticeList.Add(new Vertex
                        {
                            Color = color,
                            Position = new Vector3
                            {
                                X = adtChunk.MCNK.Position.X - (i * Constants.UnitSize * 0.5f),
                                Y = adtChunk.MCNK.Position.Y - ((j + (((i % 2) != 0) ? 0.5f : 0.0f)) * Constants.UnitSize),
                                Z = adtChunk.MCVT.Heights[idx] + adtChunk.MCNK.Position.Z
                            },
                            TextureCoordinates = new Vector2(i / 8.0f + (((i & 2) == 0) ? 0.5f / 8.0f : 0.0f), j / 17.0f),
                        });

                        ++idx;
                    }
                }
                #endregion

                tileRenderer.AddMapChunk(BindIndexedVertex(adtChunk, _mapTiles[tileToLoadKey].Textures.MTEX, 
                    verticeList.ToArray(), indiceList.ToArray()));
            }
            _batchRenderers[tileToLoadKey] = tileRenderer;

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }

        private MapChunkRenderer BindIndexedVertex(MapChunk adtChunk, MTEX mtex, Vertex[] vertices, uint[] indices)
        {
            var renderer = new MapChunkRenderer { TriangleCount = indices.Length / 3 };

            for (var i = 0; i < adtChunk.MCLY.Length; ++i)
            {
                if (adtChunk.MCLY[i] == null || adtChunk.MCAL == null)
                    continue;

                var texture = _textureCache[mtex.Filenames[adtChunk.MCLY[i].TextureId]];
                texture.MergeAlphaMap(adtChunk.MCAL.GetAlpha(i));
                renderer.AddTexture(texture);
            }

            GL.BindVertexArray(renderer.VAO);

            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = vertices.Length * vertexSize;

            GL.BindBuffer(BufferTarget.ArrayBuffer, renderer.VerticeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), vertices, BufferUsageHint.StaticDraw);
            
            VertexAttribPointer(_shader.GetAttribLocation("vertex_shading"), 3,
                VertexAttribPointerType.Float, vertexSize, IntPtr.Zero);

            // GL.BindSampler(renderer.TextureSampler0, _shader.GetUniformLocation("texture_sampler0"));
            // GL.BindSampler(renderer.TextureSampler1, _shader.GetUniformLocation("texture_sampler1"));
            // GL.BindSampler(renderer.TextureSampler2, _shader.GetUniformLocation("texture_sampler2"));
            // GL.BindSampler(renderer.TextureSampler3, _shader.GetUniformLocation("texture_sampler3"));

            VertexAttribPointer(_shader.GetAttribLocation("vertice_position"), 3,
                VertexAttribPointerType.Float, vertexSize, sizeof(float) * 3);

            VertexAttribPointer(_shader.GetAttribLocation("in_TexCoord0"), 2,
                VertexAttribPointerType.Float, vertexSize, (IntPtr)(sizeof(float) * 6));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderer.IndiceVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)),
                indices, BufferUsageHint.StaticDraw);

            return renderer;
        }

        private void VertexAttribPointer(int location, int size, VertexAttribPointerType type, int stride, IntPtr offset)
        {
            GL.VertexAttribPointer(location, size, type, false, stride, offset);
            GL.EnableVertexAttribArray(location);
        }

        private void VertexAttribPointer(int location, int size, VertexAttribPointerType type, int stride, int offset)
        {
            GL.VertexAttribPointer(location, size, type, false, stride, offset);
            GL.EnableVertexAttribArray(location);
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

            // GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer.FrameBufferID);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            var uniform = Matrix4.Mult(_camera.View, _camera.Projection);
            GL.UniformMatrix4(_shader.GetUniformLocation("projection_modelview"), false, ref uniform);

            // GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            foreach (var renderer in _batchRenderers.Values)
            {
                renderer.Render();
                /*GL.BindVertexArray(renderer.VAO);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderer.IndiceVBO);

                if (renderer.TextureIDs[0] != -1)
                {
                    _textureCache[renderer.TextureIDs[0]].BindToUnit(TextureUnit.Texture0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, renderer.TextureSampler0);
                    GL.Uniform1(_shader.GetUniformLocation("texture_sampler0"), 0);
                }

                if (renderer.TextureIDs[1] != -1)
                {
                    _textureCache[renderer.TextureIDs[1]].BindToUnit(TextureUnit.Texture1);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, renderer.TextureSampler1);
                    GL.Uniform1(_shader.GetUniformLocation("texture_sampler1"), 1);
                }

                if (renderer.TextureIDs[2] != -1)
                {
                    _textureCache[renderer.TextureIDs[2]].BindToUnit(TextureUnit.Texture2);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, renderer.TextureSampler2);
                    GL.Uniform1(_shader.GetUniformLocation("texture_sampler2"), 2);
                }

                if (renderer.TextureIDs[3] != -1)
                {
                    _textureCache[renderer.TextureIDs[4]].BindToUnit(TextureUnit.Texture3);
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, renderer.TextureSampler3);
                    GL.Uniform1(_shader.GetUniformLocation("texture_sampler2"), 3);
                }

                if (renderer.AlphaTexture != null)
                {
                    renderer.AlphaTexture.BindToUnit(TextureUnit.Texture4);
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, renderer.AlphaSampler);
                    GL.Uniform1(_shader.GetUniformLocation("alpha_sampler"), 4);
                }

                GL.DrawElements(PrimitiveType.Triangles, renderer.TriangleCount,
                    DrawElementsType.UnsignedInt, IntPtr.Zero);*/
            }

            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);

            _control.SwapBuffers();
        }
    }
}
