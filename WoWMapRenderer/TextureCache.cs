using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMapRenderer
{
    public class TextureCache
    {
        public static Dictionary<string, Texture> Storage { get; private set; }

        private static int _unit = 0;
        public static int Unit
        {
            get { return _unit; }
            set { _unit = value; }
        }

        public static void Initialize()
        {
            Storage = new Dictionary<string, Texture>(10);
        }

        public static void AddTexture(string textureName)
        {
            if (Storage.ContainsKey(textureName))
                return;

            Storage[textureName] = new Texture(textureName);
        }

        public static void RemoveTexture(string textureName)
        {
            if (!Storage.ContainsKey(textureName))
                return;
            Storage.Remove(textureName);
        }

        public static bool ContainsKey(string textureName)
        {
            return Storage.ContainsKey(textureName);
        }

        public static int GetTextureID(string textureName)
        {
            if (!ContainsKey(textureName))
                return -1;
            return Storage[textureName].ID;
        }

        public static Texture GetTexture(string textureName)
        {
            return Storage.ContainsKey(textureName) ? Storage[textureName] : null;
        }
    }
}
