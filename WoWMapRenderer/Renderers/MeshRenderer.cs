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
    public class MeshRenderer : IRenderer
    {
        public MeshRenderer(Mesh mesh, MeshType? overrideType = null)
        {
            MeshType = overrideType.HasValue ? overrideType.Value : mesh.Type;
            Vertices = mesh.Vertices.Select(v => new Vertex { Position = v, Type = (int)MeshType });
            Indices = mesh.Indices;
        }

        public MeshType MeshType { get; private set; }

        public int VAO { get; set; }
        public int VerticeVBO { get; set; }
        public int IndicesVBO { get; set; }

        public IEnumerable<Vertex> Vertices { get; private set; }
        public IEnumerable<uint> Indices { get; private set; }

        public int VerticeCount { get { return Vertices.Count(); } }
        public int IndiceCount { get; set; }

        public void Bind(Shader shader)
        {
            GL.BindVertexArray(VAO);

            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = Vertices.Count() * vertexSize;

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
            Vertices = null;
            Indices = null;
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
        }
    }
}
