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
using WoWMap.Geometry;
using WoWMapRenderer.Renderers;

namespace WoWMapRenderer
{
    public class RenderView
    {
        public RenderView(GLControl control)
        {
            Control = control;

            Control.Resize += (sender, args) => GL.Viewport(0, 0, Control.Width, Control.Height);
            Control.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                    OnRightClick(UnprojectCoordinates(args.X, args.Y));
            };
            Control.KeyPress += (sender, args) => Camera?.Update();
            Control.MouseMove += (sender, args) => Camera?.Update();
        }

        private GLControl Control { get; set; }

        public Camera Camera { get; set; }
        public IRenderer Renderer { get; set; }

        public Shader Shader { get; private set; }

        private BackgroundWorkerEx _loader;

        public delegate void ProgressHandler(int progress, string state);
        public event ProgressHandler OnProgress;

        public Dictionary<RenderOptions, bool> Options { get; set; } = new Dictionary<RenderOptions, bool>
        {
            [RenderOptions.ForceWireframe] = false,
        };

        public Dictionary<MeshType, bool> DrawMeshTypeEnabled { get; private set; } = new Dictionary<MeshType, bool>()
        {
            [MeshType.Terrain] = true,
            [MeshType.WorldModelObject] = true,
            [MeshType.Doodad] = true,
            [MeshType.Liquid] = true,
        };

        public void LoadMap(string mapName)
        {
            _loader = new BackgroundWorkerEx();
            _loader.DoWork += (sender, e) =>
            {
                _loader.ReportProgress(1, "Loading WDT...");
                var wdt = new WDT(string.Format(@"World\Maps\{0}\{0}.wdt", mapName));
                Renderer = new WDTRenderer(this, mapName, wdt);
                _loader.ReportProgress(100, "Map loaded");
            };
            _loader.ProgressChanged += (sender, args) =>
            {
                if (OnProgress != null)
                    OnProgress(args.ProgressPercentage, "Loading WDT...");
            };
            _loader.RunWorkerCompleted += (sender, e) => InitializeView();
            _loader.RunWorkerAsync();
        }

        private void InitializeView()
        {
            Shader = new Shader();
            Shader.CreateFromFile("shaders/vertex.glsl", "shaders/fragment.glsl");
            Shader.SetCurrent();

            Renderer.Bind(Shader);

            Control.Resize += (sender, args) => Render();
            Control.Paint += (sender, args) => Render();

            Renderer.Update();
            Render();
        }

        public void SetCamera(Vector3 pos)
        {
            Camera = new Camera(pos, -Vector3.UnitZ);
            Camera.SetViewport(Control.Width, Control.Height);
            GL.Viewport(0, 0, Control.Width, Control.Height);
            Camera.OnMovement += () =>
            {
                Renderer.Update();
                Render();
            };
        }

        private void Render()
        {
            GL.ClearColor(Color.White);
            GL.Viewport(0, 0, Control.Width, Control.Height);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, Options[RenderOptions.ForceWireframe] ? PolygonMode.Line : PolygonMode.Fill);

            var uniform = Matrix4.Mult(Camera.View, Camera.Projection);
            GL.UniformMatrix4(Shader.GetUniformLocation("projection_modelview"), false, ref uniform);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.DepthTest);
            //GL.DepthFunc(DepthFunction.Less);

            Renderer.Render(Shader);

            Control.SwapBuffers();
        }

        private Vector3 UnprojectCoordinates(float x, float y)
        {
            if (Camera == null)
                return Vector3.Zero;
            var mouse = new Vector2(x, y);
            var mat4 = mouse.UnProject(Camera.Projection, Camera.View, new Size(Control.Width, Control.Height));
            return mat4.Xyz;
        }

        private void OnRightClick(Vector3 terrainCoordinates)
        {
            Debug.WriteLine($"Clicked coordinates [ {terrainCoordinates.X} {terrainCoordinates.Y} {terrainCoordinates.Z} ]");
        }

        public enum RenderOptions
        {
            ForceWireframe,
        };
    }
}
