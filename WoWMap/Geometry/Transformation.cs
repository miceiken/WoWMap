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
        public static Matrix GetWmoTransform(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            Matrix translation;
            if ((position.X == 0.0f) && (position.Y == 0.0f) && (position.Z == 0.0f))
                translation = Matrix.Identity;
            else
                translation = Matrix.Translation(position.X, position.Z, position.Y);
            var rotTranslation = Matrix.RotationZ((rotation.Y+90.0f).ToRadians()) * Matrix.RotationY((rotation.X+180.0f).ToRadians()) * Matrix.RotationX(rotation.Z.ToRadians());

            if (scale < 1.0f || scale > 1.0f)
                return Matrix.Scaling(scale) * rotTranslation * translation;
            return rotTranslation * translation;
        }

        public static Matrix GetDoodadTransform(Vector3 position, Vector3 rotation, float scale = 1.0f)
        {
            Matrix translation;
            if ((position.X == 0.0f) && (position.Y == 0.0f) && (position.Z == 0.0f))
                translation = Matrix.Identity;
            else
                translation = Matrix.Translation(position.X, position.Z, position.Y);
            var rotTranslation = Matrix.RotationZ((rotation.Y-90.0f).ToRadians()) * Matrix.RotationY(rotation.X.ToRadians()) * Matrix.RotationX(rotation.Z.ToRadians());

            if (scale < 1.0f || scale > 1.0f)
                return Matrix.Scaling(scale) * rotTranslation * translation;
            return rotTranslation * translation;
        }

        public static Matrix GetWmoDoodadTransformation(MODD.MODDEntry modd, MODF.MODFEntry modf)
        {
            var modfTransform = GetWmoTransform(modf.Position, modf.Rotation);
            var translation = Matrix.Translation(modd.Position.X, modd.Position.Y, modd.Position.Z);
            var quatRotation = Matrix.RotationQuaternion(new Quaternion(-modd.Rotation[2], modd.Rotation[3], -modd.Rotation[1], modd.Rotation[0]));

            return Matrix.Scaling(modd.Scale) * Matrix.RotationY((float)Math.PI) * quatRotation * modfTransform;
        }

        /*
         *  public Matrix GetTranform()
        {
            return Matrix.Scaling(Scale) * RotationMatrix * Matrix.Translation(Position);
        }
         
         protected override Matrix RotationMatrix
        {
            get { return Matrix.RotationQuaternion(Rotation); }
        }*/

        private static float ToRadians(this float angle)
        {
            return (float)(Math.PI / 180) * angle;
        }
    }
}
