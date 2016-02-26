using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Geometry;
using WoWMap.Layers;

namespace WoWMapRenderer.Renderers
{
    public class GlobalModelRenderer : IRenderer
    {
        public GlobalModelRenderer(RenderView controller, WDT wdt)
        {
            Controller = controller;

            MeshRenderers = new GenericCollectionRenderer<MeshRenderer>(Controller);

            InitializeView(wdt);
            Generate(wdt);
            MeshRenderers.Bind(Controller.Shader);
        }

        public RenderView Controller { get; private set; }

        public GenericCollectionRenderer<MeshRenderer> MeshRenderers { get; private set; }

        public void Generate(WDT wdt)
        {
            if (!wdt.IsGlobalModel) return;
            wdt.GenerateGlobalModel();

            // Terrain
            AddMeshRenderer(wdt.ModelScene.Terrain);
            // Doodads
            AddMeshRenderer(wdt.ModelScene.Doodads);
            // Liquids
            AddMeshRenderer(wdt.ModelScene.Liquids);
        }

        private void AddMeshRenderer(IEnumerable<Mesh> mesh)
        {
            var newRenderers = mesh
                .Select(m => new MeshRenderer(Controller, m))       // Create renderer
                .OfType<MeshRenderer>()                             // Eliminate null-instances
                .Where(m => m.IndiceCount > 0);                     // Eliminate empty meshes 
            MeshRenderers.Renderers.AddRange(newRenderers);         // Add to rendering loop
        }

        private void InitializeView(WDT wdt)
        {
            Controller.SetCamera(wdt.MODF.Entries[0].Position);
        }

        public void Update() => MeshRenderers.Update();
        public void Bind(Shader shader) => MeshRenderers.Bind(shader);
        public void Delete() => MeshRenderers.Delete();
        public void Render(Shader shader)
        {
            MeshRenderers.Render(shader);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
