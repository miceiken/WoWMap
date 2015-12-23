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
            placementMatrix = Matrix.Multiply(Matrix.RotationX((float)(Math.PI / 2.0f)), placementMatrix);
            placementMatrix = Matrix.Multiply(Matrix.RotationY((float)(Math.PI / 2.0f)), placementMatrix);

            placementMatrix = Matrix.Multiply(Matrix.Translation(Constants.MaxXY - position.X, position.Y, Constants.MaxXY - position.Z), placementMatrix);
            placementMatrix = Matrix.Multiply(Matrix.RotationY((rotation.Y - 270).ToRadians()), placementMatrix); // Pitch
            placementMatrix = Matrix.Multiply(Matrix.RotationZ((-rotation.X).ToRadians()), placementMatrix); // Roll
            placementMatrix = Matrix.Multiply(Matrix.RotationX((rotation.Z - 90).ToRadians()), placementMatrix); // Yaw
            placementMatrix = Matrix.Multiply(Matrix.Scaling(scale), placementMatrix);

            return placementMatrix;
        }

        public static Matrix GetDoodadTransform(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var placementMatrix = Matrix.Identity;

            placementMatrix = Matrix.Multiply(GetWMOTransform(modf.Position, modf.Rotation), placementMatrix); ;
            placementMatrix = Matrix.Multiply(Matrix.Translation(modd.Position), placementMatrix); ;
            placementMatrix = Matrix.Multiply(Matrix.RotationQuaternion(modd.Rotation), placementMatrix); ;
            placementMatrix = Matrix.Multiply(Matrix.Scaling(modd.Scale), placementMatrix); ;
            
            return placementMatrix;
        }
    }
}
