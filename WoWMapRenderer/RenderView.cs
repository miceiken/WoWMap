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
using OpenTK.Graphics;

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
                    OnProgress(args.ProgressPercentage, (string)args.UserState);
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
            var worker = new BackgroundWorkerEx();
            worker.DoWork += (sender, e) =>
            {
                Camera = new Camera(pos, -Vector3.UnitZ);
                Camera.SetViewport(Control.Width, Control.Height);
                GL.Viewport(0, 0, Control.Width, Control.Height);
                Camera.OnMovement += () =>
                {
                    Renderer.Update();
                    Render();
                };
            };
            worker.RunWorkerAsync();
        }

        private void SetView()
        {
            var modelView = Camera.View;
            GL.UniformMatrix4(Shader.GetUniformLocation("modelview_matrix"), false, ref modelView);
            var projection = Camera.Projection;
            GL.UniformMatrix4(Shader.GetUniformLocation("projection_matrix"), false, ref projection);
        }

        private void SetLighting()
        {            
            GL.Light(LightName.Light0, LightParameter.Position, new Vector4(Camera.Position, 1.0f));
            GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0, 0, 0, 1));
            GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1, 1, 1, 1));
            GL.Light(LightName.Light0, LightParameter.Specular, new Vector4(1, 1, 1, 1));
            GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.2f, 0.2f, 0.2f, 1f });
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            
            GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, new Vector4(1, 1, 1, 1));
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Emission, new Vector4(0, 0, 0, 1));
            GL.Enable(EnableCap.ColorMaterial);
        }

        private void Render()
        {
            GL.ClearColor(Color.White);
            GL.Viewport(0, 0, Control.Width, Control.Height);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, Options[RenderOptions.ForceWireframe] ? PolygonMode.Line : PolygonMode.Fill);

            SetView();
            SetLighting();
            GL.Enable(EnableCap.DepthTest);

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
