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
        public static Dictionary<int /* textureUnit */, List<Texture>> BoundTextures { get; private set; }

        /// This makes sure that textures for a map chunk do not get pushed to the same unit.
        private static List<int> _freeUnits = new List<int>();
        public static int Unit {
            get { return _freeUnits.First(); }
        }

        public static void Initialize()
        {
            RawTextures = new Dictionary<string, Texture>(100);
            BoundTextures = new Dictionary<int, List<Texture>>();
            ResetFreeForMapChunk();
        }

        public static void ResetFreeForMapChunk()
        {
            _freeUnits.Clear();
            _freeUnits.AddRange(new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        }

        public static bool AddBoundTexture(Texture tex)
        {
            if (!BoundTextures.ContainsKey(Unit))
                BoundTextures[Unit] = new List<Texture>();

            tex.BindTexture(TextureUnit.Texture0 + Unit);

            BoundTextures[Unit].Add(tex);

            _freeUnits.Remove(Unit);
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
