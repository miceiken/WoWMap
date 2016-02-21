using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CASCExplorer;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using WoWMap;
using WoWMap.Chunks;
using WoWMap.Layers;
using System.Drawing;
using WoWMap.Geometry;

namespace WoWMapRenderer.Renderers
{
    public class WDTRenderer : IRenderer
    {
        public WDTRenderer(RenderView controller, string mapName, WDT wdt)
        {
            Controller = controller;
            WDT = wdt;

            if (wdt.IsGlobalModel)
                Renderer = new GlobalModelRenderer(controller, wdt);
            else
                Renderer = new ContinentRenderer(controller, mapName, wdt);
        }

        public WDTRenderer(RenderView controller, string mapName)
            : this(controller, mapName, new WDT(string.Format(@"World\Maps\{0}\{0}.wdt", mapName)))
        { }

        public RenderView Controller { get; set; }

        public WDT WDT { get; private set; }
        public IRenderer Renderer { get; private set; }

        public void Update() => Renderer.Update();
        public void Bind(Shader shader) { }
        public void Delete() => Renderer.Delete();
        public void Render(Shader shader) => Renderer.Render(shader);
    }
}
