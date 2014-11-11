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
        public static Matrix GetTransform(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            var translation = Matrix.Translation(-(position.X - Constants.MaxXY), -(position.Z - Constants.MaxXY), position.Y);
            // Something is wrong with the following line, but I have no idea what. 
            var rotTranslation = Matrix.RotationYawPitchRoll((rotation.X - 180.0f).ToRadians(), (rotation.Z).ToRadians(), (rotation.Y - 90.0f).ToRadians());

            return Matrix.Scaling(scale) * rotTranslation * translation;
        }

        public static Matrix GetWmoDoodadTransform(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var modfTransform = GetTransform(modf.Position, modf.Rotation);
            var translation = Matrix.Translation(modd.Position);
            var quatRotation = Matrix.RotationQuaternion(new Quaternion(-modd.Rotation[0], -modd.Rotation[1], -modd.Rotation[2], modd.Rotation[3]));

            return Matrix.Scaling(modd.Scale) * quatRotation * translation * modfTransform;
        }
    }
}
