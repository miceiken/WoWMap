using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWMapRenderer.Renderers
{
    public interface IRenderer
    {
        void Bind(Shader shader);
        void Delete();
        void Render(Shader shader);
    }
}
