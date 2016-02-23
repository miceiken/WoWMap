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
            Generate(wdt);
        }

        public RenderView Controller { get; private set; }

        public int VAO { get; set; }
        public int VerticeVBO { get; set; }
        public int IndicesVBO { get; set; }

        public List<Vertex> Vertices { get; private set; } = new List<Vertex>();
        public List<uint> Indices { get; private set; } = new List<uint>();

        public int VerticeCount { get { return Vertices.Count; } }
        public int IndiceCount { get; set; }

        public void Generate(WDT wdt)
        {
            if (!wdt.IsGlobalModel) return;
            wdt.GenerateGlobalModel();

            //var mesh = wdt.ModelScene.Flatten();
            //Vertices = mesh.Vertices.Select(v => new Vertex { Position = v, Type = (int)mesh.Type }).ToList();
            //Indices = mesh.Indices.ToList();

            Bind(Controller.Shader);
        }

        private void InitializeView()
        {
            Controller.SetCamera(Vertices.FirstOrDefault().Position);
        }

        public void Update()
        { }

        public void Bind(Shader shader)
        {
            VerticeVBO = GL.GenBuffer();
            IndicesVBO = GL.GenBuffer();
            VAO = GL.GenVertexArray();

            IndiceCount = Indices.Count;

            GL.BindVertexArray(VAO);

            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = Vertices.Count * vertexSize;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VerticeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), Vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(shader.GetAttribLocation("position"), 3, VertexAttribPointerType.Float, false,
                vertexSize, sizeof(int));
            GL.EnableVertexAttribArray(shader.GetAttribLocation("position"));

            GL.VertexAttribIPointer(shader.GetAttribLocation("type"), 1, VertexAttribIntegerType.Int,
                vertexSize, IntPtr.Zero);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("type"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(IndiceCount * sizeof(uint)),
                Indices.ToArray(), BufferUsageHint.StaticDraw);

            // Not needed anymore
            Vertices.Clear();
            Indices.Clear();

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void Delete()
        {
            GL.DeleteBuffer(IndicesVBO);
            GL.DeleteBuffer(VerticeVBO);
            GL.DeleteVertexArray(VAO);
        }

        public void Render(Shader shader)
        {
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);

            GL.DrawElements(PrimitiveType.Triangles, IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }        
    }
}
