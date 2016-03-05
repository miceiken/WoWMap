using OpenTK;
using OpenTK.Graphics;
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
        public MeshRenderer(RenderView controller, Mesh mesh, MeshType? overrideType = null)
        {
            Controller = controller;

            MeshType = overrideType.HasValue ? overrideType.Value : mesh.Type;
            Indices = mesh.Indices;
            Vertices = mesh.Vertices;
            Normals = mesh.Normals;
            IndiceCount = Indices.Length;
        }

        public RenderView Controller { get; private set; }

        public MeshType MeshType { get; private set; }

        public int VAO { get; set; }
        public int IndicesVBO { get; set; }
        public int VerticesVBO { get; set; }
        public int NormalsVBO { get; set; }

        public uint[] Indices { get; private set; }
        public Vector3[] Vertices { get; private set; }
        public Vector3[] Normals { get; private set; }

        public int IndiceCount { get; set; }

        public void Update() { }

        public void Bind(Shader shader)
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            // Vertices VBO
            VerticesVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VerticesVBO);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(Vertices.Length * Vector3.SizeInBytes),
                Vertices, BufferUsageHint.StaticDraw);

            // Vertices VAO
            GL.VertexAttribPointer(shader.GetAttribLocation("in_position"),
                3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("in_position"));

            // Normals VBO
            NormalsVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, NormalsVBO);
            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer,
                new IntPtr(Normals.Length * Vector3.SizeInBytes),
                Normals, BufferUsageHint.StaticDraw);

            // Normals VAO
            GL.VertexAttribPointer(shader.GetAttribLocation("in_normal"),
                3, VertexAttribPointerType.Float, true, Vector3.SizeInBytes, 0);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("in_normal"));

            IndicesVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                (IntPtr)(IndiceCount * sizeof(uint)),
                Indices, BufferUsageHint.StaticDraw);

            // Not needed anymore
            Vertices = null;
            Normals = null;
            Indices = null;
        }

        public void Delete()
        {
            GL.DeleteBuffer(IndicesVBO);
            GL.DeleteBuffer(VerticesVBO);
            GL.DeleteBuffer(NormalsVBO);
            GL.DeleteVertexArray(VAO);
        }

        public void Render(Shader shader)
        {
            if (!Controller.DrawMeshTypeEnabled[MeshType]) return;

            GL.Uniform4(shader.GetUniformLocation("meshColor"), MeshTypeColorMap[MeshType]);
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.DrawElements(PrimitiveType.Triangles, IndiceCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        private static Dictionary<MeshType, Color4> MeshTypeColorMap = new Dictionary<MeshType, Color4>()
        {
            [MeshType.Terrain] = new Color4(138, 185, 0, 255),
            [MeshType.WorldModelObject] = new Color4(182, 25, 25, 255),
            [MeshType.Doodad] = Color4.Yellow,
            [MeshType.Liquid] = Color4.Blue,
        };
    }
}
