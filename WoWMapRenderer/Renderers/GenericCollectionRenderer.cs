using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMapRenderer.Renderers
{
    public class GenericCollectionRenderer<T> : IRenderer where T : IRenderer
    {
        public GenericCollectionRenderer(RenderView controller)
        {
            Controller = controller;
        }

        public RenderView Controller { get; private set; }

        public List<T> Renderers { get; private set; } = new List<T>();
        public void Bind(Shader shader) => Renderers.ForEach(r => r.Bind(shader));
        public void Delete() => Renderers.ForEach(r => r.Delete());
        public void Render(Shader shader) => Renderers.ForEach(r => r.Render(shader));
        public void Update() => Renderers.ForEach(r => r.Update());
    }
}
