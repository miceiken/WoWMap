using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace WoWMapRenderer
{
    class Shader
    {
        private int VertexID;
        private int FragmentID;
        public int ProgramID;

        private Dictionary<string, int> _attribLocations = new Dictionary<string, int>();
        private Dictionary<string, int> _uniformLocations = new Dictionary<string, int>(); 

        public void CreateShader(string vertex, string fragment)
        {
            ProgramID = GL.CreateProgram();

            VertexID = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexID, vertex);
            GL.CompileShader(VertexID);
            GL.AttachShader(ProgramID, VertexID);

            FragmentID = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentID, fragment);
            GL.CompileShader(FragmentID);
            GL.AttachShader(ProgramID, FragmentID);
            
            GL.LinkProgram(ProgramID);
        }

        public void SetCurrent()
        {
            GL.UseProgram(ProgramID);
        }


        public int GetAttribLocation(string attribName)
        {
            if (!_attribLocations.ContainsKey(attribName))
                _attribLocations[attribName] = GL.GetAttribLocation(ProgramID, attribName);
            return _attribLocations[attribName];
        }

        public int GetUniformLocation(string attrName)
        {
            if (!_uniformLocations.ContainsKey(attrName))
                _uniformLocations[attrName] = GL.GetUniformLocation(ProgramID, attrName);
            return _uniformLocations[attrName];
        }
    }
}
