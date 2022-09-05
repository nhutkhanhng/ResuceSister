using UnityEngine;

namespace Redcode.Extensions
{
    public static class Vector3Extensions
    {
        /// <summary>
        /// Sets value to vector's axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="axis">Axis index of the vector.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 With(this Vector3 vector, int axis, float value)
        {
            vector[axis] = value;
            return vector;
        }

        /// <summary>
        /// Sets value to vector's x axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="x">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithX(this Vector3 vector, float x) => With(vector, 0, x);

        /// <summary>
        /// Sets value to vector's y axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="y">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithY(this Vector3 vector, float y) => With(vector, 1, y);

        /// <summary>
        /// Sets value to vector's z axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="z">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithZ(this Vector3 vector, float z) => With(vector, 2, z);

        /// <summary>
        /// Sets values to vector's axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="axis1">First axis index of the vector.</param>
        /// <param name="value1">First value to set.</param>
        /// <param name="axis2">Second axis index of the vector.</param>
        /// <param name="value2">Second value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 With(this Vector3 vector, int axis1, float value1, int axis2, float value2)
        {
            vector[axis1] = value1;
            vector[axis2] = value2;

            return vector;
        }

        /// <summary>
        /// Sets values to vector's x and y axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="x">Value to set.</param>
        /// <param name="y">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithXY(this Vector3 vector, float x, float y) => With(vector, 0, x, 1, y);

        /// <summary>
        /// Sets value to vector's x and y axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithXY(this Vector3 vector, Vector2 value) => With(vector, 0, value.x, 1, value.y);

        /// <summary>
        /// Sets value to vector's x and z axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="x">Value to set.</param>
        /// <param name="z">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithXZ(this Vector3 vector, float x, float z) => With(vector, 0, x, 2, z);

        /// <summary>
        /// Sets value to vector's x and z axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithXZ(this Vector3 vector, Vector2 value) => With(vector, 0, value.x, 2, value.y);

        /// <summary>
        /// Sets value to vector's y and z axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="y">Value to set.</param>
        /// <param name="z">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithYZ(this Vector3 vector, float y, float z) => With(vector, 1, y, 2, z);

        /// <summary>
        /// Sets value to vector's y and z axis.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="value">Value to set.</param>
        /// <returns>Changed copy of the vector.</returns>
        public static Vector3 WithYZ(this Vector3 vector, Vector2 value) => With(vector, 1, value.x, 2, value.y);

        /// <summary>
        /// Gets <see cref="Vector2"/> by axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="axis1">First axis.</param>
        /// <param name="axis2">Second axis.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 Get(this Vector3 vector, int axis1, int axis2) => new Vector2(vector[axis1], vector[axis2]);

        /// <summary>
        /// Gets <see cref="Vector2"/> by x and y axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetXY(this Vector3 vector) => Get(vector, 0, 1);

        /// <summary>
        /// Gets <see cref="Vector2"/> by x and z axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetXZ(this Vector3 vector) => Get(vector, 0, 2);

        /// <summary>
        /// Gets <see cref="Vector2"/> by y and x axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetYX(this Vector3 vector) => Get(vector, 1, 0);

        /// <summary>
        /// Gets <see cref="Vector2"/> by y and z axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetYZ(this Vector3 vector) => Get(vector, 1, 2);

        /// <summary>
        /// Gets <see cref="Vector2"/> by z and x axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetZX(this Vector3 vector) => Get(vector, 2, 0);

        /// <summary>
        /// Gets <see cref="Vector2"/> by z and y axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector2"/> vector.</returns>
        public static Vector2 GetZY(this Vector3 vector) => Get(vector, 2, 1);

        /// <summary>
        /// Gets vector with swapped axes.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="axis1">First axis.</param>
        /// <param name="axis2">Second axis.</param>
        /// <param name="axis2">Third axis.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 Get(this Vector3 vector, int axis1, int axis2, int axis3) => new Vector3(vector[axis1], vector[axis2], vector[axis3]);

        /// <summary>
        /// Gets vector with order XZY.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 GetXZY(this Vector3 vector) => Get(vector, 0, 2, 1);

        /// <summary>
        /// Gets vector with order YXZ.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 GetYXZ(this Vector3 vector) => Get(vector, 1, 0, 2);

        /// <summary>
        /// Gets vector with order YZX.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 GetYZX(this Vector3 vector) => Get(vector, 1, 2, 0);

        /// <summary>
        /// Gets vector with order ZXY.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 GetZXY(this Vector3 vector) => Get(vector, 2, 0, 1);

        /// <summary>
        /// Gets vector with order ZYX.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector3 GetZYX(this Vector3 vector) => Get(vector, 2, 1, 0);

        /// <summary>
        /// Inserts value to x axis and extends vector to 4-dimensional.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="x">Target vector.</param>
        /// <returns><see cref="Vector3"/> vector.</returns>
        public static Vector4 InsertX(this Vector3 vector, float x = 0) => new Vector4(x, vector.x, vector.y, vector.z);

        /// <summary>
        /// Inserts value to y axis and extends vector to 4-dimensional.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="y">Target vector.</param>
        /// <returns><see cref="Vector4"/> vector.</returns>
        public static Vector4 InsertY(this Vector3 vector, float y = 0) => new Vector4(vector.x, y, vector.y, vector.z);

        /// <summary>
        /// Inserts value to z axis and extends vector to 4-dimensional.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="z">Target vector.</param>
        /// <returns><see cref="Vector4"/> vector.</returns>
        public static Vector4 InsertZ(this Vector3 vector, float z = 0) => new Vector4(vector.x, vector.y, z, vector.z);

        /// <summary>
        /// Inserts value to w axis and extends vector to 4-dimensional.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="w">Target vector.</param>
        /// <returns><see cref="Vector4"/> vector.</returns>
        public static Vector4 InsertW(this Vector3 vector, float w = 0) => new Vector4(vector.x, vector.y, vector.z, w);

        private static void Compare(Vector3 vector, ref int index, int compareIndex, int result)
        {
            if (vector[compareIndex].CompareTo(vector[index]) == result)
                index = compareIndex;
        }

        private static int CompareAllComponents(Vector3 vector, int result)
        {
            var index = 0;

            Compare(vector, ref index, 1, result);
            Compare(vector, ref index, 2, result);

            return index;
        }

        /// <summary>
        /// Gets max component index from vector.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Vector's max component index.</returns>
        public static int MaxComponentIndex(this Vector3 vector) => CompareAllComponents(vector, 1);

        /// <summary>
        /// Gets min component from vector.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Vector's min component</returns>
        public static float MaxComponent(this Vector3 vector) => vector[MaxComponentIndex(vector)];

        /// <summary>
        /// Gets min component index from vector.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Vector's min component index.</returns>
        public static int MinComponentIndex(this Vector3 vector) => CompareAllComponents(vector, -1);

        /// <summary>
        /// Gets min component from vector.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Vector's min component</returns>
        public static float MinComponent(this Vector3 vector) => vector[MinComponentIndex(vector)];

        /// <summary>
        /// Remaps all vector's components from one interval to other.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="min1">Min value of the beginning interval.</param>
        /// <param name="max1">Max value of the beginning interval.</param>
        /// <param name="min2">Min value of the target interval.</param>
        /// <param name="max2">Max value of the target interval.</param>
        /// <returns>Remaped vector.</returns>
        public static Vector3 Remap(this Vector3 vector, float min1, float max1, float min2, float max2)
        {
            return new Vector3(vector.x.Remap(min1, max1, min2, max2), vector.y.Remap(min1, max1, min2, max2), vector.z.Remap(min1, max1, min2, max2));
        }

        /// <summary>
        /// Creates new vector with absolute components.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Vector with absolute components.</returns>
        public static Vector3 Abs(this Vector3 vector) => new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));

        /// <summary>
        /// Creates new vector with clamped components.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="min">The minimum floating value to campare agains.</param>
        /// <param name="max">The maximum floating value to campare agains.</param>
        /// <returns>Clamped vector.</returns>
        public static Vector3 Clamp(this Vector3 vector, float min, float max)
        {
            return new Vector3(Mathf.Clamp(vector.x, min, max), Mathf.Clamp(vector.y, min, max), Mathf.Clamp(vector.z, min, max));
        }

        /// <summary>
        /// Creates and returns a vector whose components are limited to 0 and 1.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <returns>Clamped vector.</returns>
        public static Vector3 Clamp01(this Vector3 vector)
        {
            return new Vector3(Mathf.Clamp01(vector.x), Mathf.Clamp01(vector.y), Mathf.Clamp01(vector.z));
        }

        /// <summary>
        /// Creates and returns a vector whose components are divided by the value.
        /// </summary>
        /// <param name="vector">Target vector.</param>
        /// <param name="other">Vector on which divide</param>
        /// <returns>Divided vector.</returns>
        public static Vector3 Divide(this Vector3 vector, Vector3 other)
        {
            return new Vector3(vector.x / other.x, vector.y / other.y, vector.z / other.z);
        }
    }
}