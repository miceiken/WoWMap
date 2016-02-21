using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap;
using WoWMap.Layers;

namespace WoWMapRenderer.Renderers
{
    public class ContinentRenderer : IRenderer
    {
        public ContinentRenderer(RenderView controller, string continent, WDT wdt)
        {
            Controller = controller;
            Continent = continent;
            WDT = wdt;
            ADTRenderer = new GenericCollectionRenderer<ADTRenderer>(Controller);

            InitializeView();
        }

        public RenderView Controller { get; private set; }
        public string Continent { get; private set; }
        public WDT WDT { get; private set; }
        public GenericCollectionRenderer<ADTRenderer> ADTRenderer { get; private set; }

        private void InitializeView()
        {
            int x, y;
            GetSpawnTile(out x, out y);
            var tileCenter = GetTileCenter(x, y);
            Controller.SetCamera(new Vector3(tileCenter.X, tileCenter.Y, 300.0f));
        }

        public void Update()
        {
            var centeredTile = GetTileAt(Controller.Camera.Position.Xy);

            var tileX = (int)Math.Floor(centeredTile.X);
            var tileY = (int)Math.Floor(centeredTile.Y);

            LoadTile(tileX, tileY);

            ADTRenderer.Update();
        }

        public void Bind(Shader shader) { }
        public void Delete() => ADTRenderer.Delete();
        public void Render(Shader shader) => ADTRenderer.Render(shader);

        private bool LoadTile(int tileX, int tileY)
        {
            if (!WDT.HasTile(tileX, tileY))
                return false;

            if (IsTileLoaded(tileX, tileY))
                return false;

            // Generate tile
            var tile = new ADT(Continent, tileX, tileY, WDT);
            tile.Read();
            tile.Generate();

            // Generate renderer
            var renderer = new ADTRenderer(Controller, tile);
            renderer.Generate();
            renderer.Bind(Controller.Shader);
            ADTRenderer.Renderers.Add(renderer);

            return true;
        }

        private bool IsTileLoaded(int x, int y) =>
            ADTRenderer.Renderers.Any(r => r.ADT.X == x && r.ADT.Y == y);

        private Vector2 GetTileAt(Vector2 position) =>
            new Vector2((Constants.MaxXY - position.X) / Constants.TileSize, (Constants.MaxXY - position.Y) / Constants.TileSize);

        private Vector2 GetTileCenter(int x, int y)
        {
            var tilePosition = new Vector2((32 - x) * Constants.TileSize, (32 - y) * Constants.TileSize);
            tilePosition.X -= Constants.TileSize / 2;
            tilePosition.Y -= Constants.TileSize / 2;
            return tilePosition;
        }

        private void GetSpawnTile(out int x, out int y)
        {
            var topLeft = new[] { 64, 64 };
            var bottomRight = new[] { 0, 0 };
            for (var xx = 0; xx < 64; ++xx)
            {
                for (var yy = 0; yy < 64; ++yy)
                {
                    if (!WDT.HasTile(xx, yy))
                        continue;

                    topLeft[0] = Math.Min(topLeft[0], xx);
                    topLeft[1] = Math.Min(topLeft[1], yy);
                    bottomRight[0] = Math.Max(bottomRight[0], xx);
                    bottomRight[1] = Math.Max(bottomRight[1], yy);
                }
            }

            x = (int)Math.Floor((topLeft[0] + bottomRight[0]) / 2.0f);
            y = (int)Math.Floor((topLeft[1] + bottomRight[1]) / 2.0f);
        }
    }
}
