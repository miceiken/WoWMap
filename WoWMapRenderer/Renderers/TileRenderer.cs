using System.Collections.Generic;
using System.Linq;

namespace WoWMapRenderer.Renderers
{
    class TileRenderer
    {
        public List<MapChunkRenderer> Renderers { get; private set; }

        public TileRenderer()
        {
            Renderers = new List<MapChunkRenderer>();
        }

        ~TileRenderer()
        {
            foreach (var r in Renderers)
                r.Delete();
            Renderers.Clear();
        }

        public MapChunkRenderer this[int index]
        {
            get { return Renderers.ElementAtOrDefault(index); }
        }

        public void AddMapChunk(MapChunkRenderer mapChunk)
        {
            Renderers.Add(mapChunk);
        }

        public void Delete()
        {
            foreach (var renderer in Renderers)
                renderer.Delete();
            Renderers.Clear();
        }

        public void Render(Shader shader)
        {
            foreach (var renderer in Renderers)
                renderer.Render(shader);
        }
    }
}
