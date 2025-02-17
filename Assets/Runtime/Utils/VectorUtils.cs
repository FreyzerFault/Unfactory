using UnityEngine;

namespace Utils
{
    public static class VectorUtils
    {
        public static Vector2 RotateVector(this Vector2 vector, float angle)
        {
            // Using Quaternions
            return Quaternion.Euler(0, 0, angle) * vector;
            
            
            // float radian = angle * Mathf.Deg2Rad;
            // float cos = Mathf.Cos(radian);
            // float sin = Mathf.Sin(radian);
            //
            // float newX = vector.x * cos - vector.y * sin;
            // float newY = vector.x * sin + vector.y * cos;
            //
            // return new Vector2(newX, newY);
        }
    }
}
