using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMapRenderer
{
    public class TextureCache
    {
        public static Dictionary<string /* fileName */, Texture> RawTextures { get; private set; }
        public static Dictionary<int /* textureUnit */, Texture> BoundTextures { get; private set; }

        private static List<int> _freeUnits = new List<int>();
        public static int Unit
        {
            get {
                return _freeUnits.FirstOrDefault();
            }
        }

        public static void UnbindTexture(int unit)
        {
            BoundTextures.Remove(unit);
            _freeUnits.Add(unit);
        }

        public static void Initialize()
        {
            RawTextures = new Dictionary<string, Texture>(10);
            BoundTextures = new Dictionary<int, Texture>();
            _freeUnits.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        public static bool AddBoundTexture(Texture tex)
        {
            if (BoundTextures.ContainsKey(tex.Unit))
                return false;

            tex.BindTexture(TextureUnit.Texture0 + Unit);
            BoundTextures[tex.Unit] = tex;
            _freeUnits.Remove(tex.Unit);
            return true;
        }

        public static void AddRawTexture(string textureName)
        {
            if (RawTextures.ContainsKey(textureName))
                return;

            RawTextures[textureName] = new Texture(textureName);
        }

        public static void RemoveRawTexture(string textureName)
        {
            if (!RawTextures.ContainsKey(textureName))
                return;
            RawTextures.Remove(textureName);
        }

        public static bool ContainsRawTexture(string textureName)
        {
            return RawTextures.ContainsKey(textureName);
        }

        public static Texture GetRawTexture(string textureName)
        {
            return RawTextures.ContainsKey(textureName) ? RawTextures[textureName] : null;
        }
    }
}
