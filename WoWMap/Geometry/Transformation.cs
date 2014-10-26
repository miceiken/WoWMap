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
        public static Matrix GetTransformation(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            Matrix translation;
            if ((position.X == 0.0f) && (position.Y == 0.0f) && (position.Z == 0.0f))
                translation = Matrix.Identity;
            else
                translation = Matrix.Translation(-(position.Z - Constants.MaxXY), -(position.X - Constants.MaxXY), position.Y);

            var rotTranslation = Matrix.RotationYawPitchRoll((rotation.Y + 180).ToRadians(), rotation.X.ToRadians(), rotation.Z.ToRadians());

            if (scale < 1.0f || scale > 1.0f)
                return Matrix.Scaling(scale) * rotTranslation * translation;
            return rotTranslation * translation;
        }

        public static Matrix GetWmoDoodadTransformation(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var modfTransform = GetTransformation(modf.Position, modf.Rotation);
            var translation = Matrix.Translation(modd.Position.X, modd.Position.Y, modd.Position.Z);
            var scale = Matrix.Scaling(modd.Scale);
            var rotation = Matrix.RotationY((float)Math.PI);
            var quatRotation = Matrix.RotationQuaternion(new Quaternion(-modd.Rotation[2], modd.Rotation[3], -modd.Rotation[1], modd.Rotation[0]));

            return scale * rotation * quatRotation * modfTransform;
        }

        private static float ToRadians(this float angle)
        {
            return (float)(Math.PI / 180) * angle;
        }
    }
}
