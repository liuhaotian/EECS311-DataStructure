using System;
using Microsoft.Xna.Framework;
namespace HappyFunBlob
{
    public static class MathUtil
    {
        public static Vector3 ClipMagnitude(this Vector3 vector, float min, float max)
        {
            float magSq = vector.LengthSquared();
            if (magSq < (min * min))
                return (min / (float)Math.Sqrt(magSq)) * vector;
            else if (magSq > (max * max))
                return (max / (float)Math.Sqrt(magSq)) * vector;
            else return vector;
        }
    }
}
