using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWMap.Layers;
using WoWMap.Chunks;
using OpenTK;

namespace WoWMap.Geometry
{
    public static class Transformation
    {
        public static Matrix4 GetWMOTransform(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            var placementMatrix = Matrix4.Identity;
            placementMatrix = Matrix4.Mult(Matrix4.CreateRotationX((float)(Math.PI / 2.0f)), placementMatrix);
            placementMatrix = Matrix4.Mult(Matrix4.CreateRotationY((float)(Math.PI / 2.0f)), placementMatrix);

            placementMatrix = Matrix4.Mult(Matrix4.CreateTranslation(Constants.MaxXY - position.X, position.Y, Constants.MaxXY - position.Z), placementMatrix);
            placementMatrix = Matrix4.Mult(Matrix4.CreateRotationY((rotation.Y - 270).ToRadians()), placementMatrix); // Pitch
            placementMatrix = Matrix4.Mult(Matrix4.CreateRotationZ((-rotation.X).ToRadians()), placementMatrix); // Roll
            placementMatrix = Matrix4.Mult(Matrix4.CreateRotationX((rotation.Z - 90).ToRadians()), placementMatrix); // Yaw
            placementMatrix = Matrix4.Mult(Matrix4.CreateScale(scale), placementMatrix);

            return placementMatrix;
        }

        public static Matrix4 GetDoodadTransform(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var placementMatrix = Matrix4.Identity;

            placementMatrix = Matrix4.Mult(GetWMOTransform(modf.Position, modf.Rotation), placementMatrix);
            placementMatrix = Matrix4.Mult(Matrix4.CreateTranslation(modd.Position), placementMatrix);
            placementMatrix = Matrix4.Mult(Matrix4.CreateFromQuaternion(modd.Rotation), placementMatrix);
            placementMatrix = Matrix4.Mult(Matrix4.CreateScale(modd.Scale), placementMatrix);
            
            return placementMatrix;
        }
    }
}
