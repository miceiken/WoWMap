using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WoWMapRenderer;

namespace WoWMapRenderExportTester
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public bool ForceWireframe {
            get {
                return forceWirefram.Checked;
            }
        }

        private List<Vertex> _vertices = new List<Vertex>();
        private List<ushort> _indices = new List<ushort>();
        private Dictionary<int, List<byte>> _textures = new Dictionary<int, List<byte>>();

        private void LoadDump(object sender, EventArgs e)
        {
            _vertices.Clear();
            _indices.Clear();
            using (var fs = new StreamReader("terraindata.txt", false))
            {
                while (!fs.EndOfStream)
                {
                    var line = fs.ReadLine();
                    var tokens = line.Substring(2).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (line[0])
                    {
                        case 'v':
                            Vertex vert = new Vertex();
                            vert.Position = new OpenTK.Vector3
                            {
                                X = float.Parse(tokens[0]),
                                Y = float.Parse(tokens[1]),
                                Z = float.Parse(tokens[2])
                            };
                            vert.Color = new OpenTK.Vector3
                            {
                                X = float.Parse(tokens[3]),
                                Y = float.Parse(tokens[4]),
                                Z = float.Parse(tokens[5])
                            };
                            vert.TextureCoordinates = new OpenTK.Vector2
                            {
                                X = float.Parse(tokens[6]),
                                Y = float.Parse(tokens[7]),
                            };
                            _vertices.Add(vert);
                            break;
                        case 'i':
                            foreach (var i in tokens.Select(n => ushort.Parse(n)))
                                _indices.Add(i);
                            break;
                        case 't':
                            var tIndex = int.Parse(line.Substring(1, 1));
                            _textures[tIndex] = new List<byte>();
                            foreach (var b in tokens.Select(n => byte.Parse(n, System.Globalization.NumberStyles.AllowHexSpecifier)))
                                _textures[tIndex].Add(b);
                            break;
                        default:
                            break;
                    }
                }
            }

            var shader = new Shader();
            shader.CreateFromFile("shaders/vertex.glsl", "shaders/fragment.glsl");
            shader.SetCurrent();

            // Binding pass
            var VAO = GL.GenVertexArray();
            var VerticeVBO = GL.GenBuffer();
            var IndicesVBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);
            var vertexSize = Marshal.SizeOf(typeof(Vertex));
            var verticeSize = _vertices.Count * vertexSize;

            GL.BindBuffer(BufferTarget.ArrayBuffer, VerticeVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticeSize), _vertices.ToArray(), BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(shader.GetAttribLocation("vertex_shading"), 3, VertexAttribPointerType.Float, false,
                vertexSize, IntPtr.Zero);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("vertex_shading"));

            GL.VertexAttribPointer(shader.GetAttribLocation("position"), 3, VertexAttribPointerType.Float, false,
                vertexSize, sizeof(float) * 3);
            GL.EnableVertexAttribArray(shader.GetAttribLocation("position"));

            GL.VertexAttribPointer(shader.GetAttribLocation("texture_coordinates"), 2, VertexAttribPointerType.Float, false,
                vertexSize, (IntPtr)(sizeof(float) * 6));
            GL.EnableVertexAttribArray(shader.GetAttribLocation("texture_coordinates"));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(_indices.Count * sizeof(ushort)),
                _indices.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Load textures
            GL.Enable(EnableCap.Texture2D);
            var texIds = new List<int>();
            var samplers = new List<int>();
            foreach (var tkv in _textures)
            {
                var t = GL.GenTexture();
                texIds.Add(t);

                GL.ActiveTexture(TextureUnit.Texture0 + tkv.Key);
                GL.BindTexture(TextureTarget.Texture2D, t);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 256, 256, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tkv.Value.ToArray());

                var sampler = GL.GenSampler();
                samplers.Add(sampler);
                GL.BindSampler(tkv.Key, sampler);
                GL.Uniform1(shader.GetUniformLocation("texture0"), sampler);
            }

            // Render pass
            GL.Viewport(0, 0, glControl1.Width, glControl1.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PolygonMode(MaterialFace.FrontAndBack, ForceWireframe ? PolygonMode.Line : PolygonMode.Fill);

            var camera = new Camera(new Vector3(1866.667f, 1866.667f, 300.0f), -Vector3.UnitZ);
            camera.SetViewport(glControl1.Width, glControl1.Height);
            var uniform = Matrix4.Mult(camera.View, camera.Projection);
            GL.UniformMatrix4(shader.GetUniformLocation("projection_modelview"), false, ref uniform);

            GL.ClearColor(Color.White);
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, IndicesVBO);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);

            glControl1.SwapBuffers();

            // Cleanup pass
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindVertexArray(0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        private void OnLoad(object sender, EventArgs e)
        {
            GL.ClearColor(Color.White);
        }
    }
}

