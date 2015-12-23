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
        private int ProgramID;

        private Dictionary<string, int> _attribLocations = new Dictionary<string, int>();
        private Dictionary<string, int> _uniformLocations = new Dictionary<string, int>(); 

        public void CreateShader(string vertex, string fragment)
        {
            FragmentID = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentID, fragment);
            GL.CompileShader(FragmentID);

            VertexID = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexID, fragment);
            GL.CompileShader(VertexID);

            ProgramID = GL.CreateProgram();
            GL.AttachShader(ProgramID, FragmentID);
            GL.AttachShader(ProgramID, VertexID);
            GL.LinkProgram(ProgramID);
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
