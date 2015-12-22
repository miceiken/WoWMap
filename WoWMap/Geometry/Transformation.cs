using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Chunks;
using SharpDX;

namespace WoWMap.Geometry
{
    public static class Transformation
    {
        public static Matrix GetWMOTransform(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            var placementMatrix = Matrix.Identity;
            placementMatrix *= Matrix.RotationX((float)(Math.PI / 2.0f));
            placementMatrix *= Matrix.RotationY((float)(Math.PI / 2.0f));

            placementMatrix *= Matrix.Translation(Constants.MaxXY - position.X, position.Y, Constants.MaxXY - position.Z);
            placementMatrix *= Matrix.RotationY((rotation.Y - 270).ToRadians()); // Pitch
            placementMatrix *= Matrix.RotationZ((-rotation.X).ToRadians()); // Roll
            placementMatrix *= Matrix.RotationX((rotation.Z - 90).ToRadians()); // Yaw

            return Matrix.Scaling(scale) * placementMatrix;
        }

        public static Matrix GetDoodadTransform(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var placementMatrix = Matrix.Identity * GetWMOTransform(modf.Position, modf.Rotation);
            placementMatrix *= Matrix.Translation(modd.Position);
            placementMatrix *= Matrix.RotationQuaternion(modd.Rotation);
            placementMatrix *= Matrix.Scaling(modd.Scale);
            return placementMatrix;
        }
    }
}
