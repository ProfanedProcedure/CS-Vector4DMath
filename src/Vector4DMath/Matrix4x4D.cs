using System;
using UnityEngine;

namespace Vector4DMath
{
    /// <summary>
    /// 4x4行列。4次元空間の線形変換(回転・スケーリング・せん断)を表す。
    /// 平行移動まで含めるには5x5の同次座標行列が要るけど、このライブラリではそこまでは扱わない。
    /// </summary>
    [Serializable]
    public struct Matrix4x4D
    {
        // row-major: m[row][col]
        public float m00, m01, m02, m03;
        public float m10, m11, m12, m13;
        public float m20, m21, m22, m23;
        public float m30, m31, m32, m33;

        public static Matrix4x4D identity => new Matrix4x4D { m00 = 1, m11 = 1, m22 = 1, m33 = 1 };
        public static Matrix4x4D zero => new Matrix4x4D();

        public float this[int row, int col]
        {
            get
            {
                switch (row)
                {
                    case 0: return col == 0 ? m00 : col == 1 ? m01 : col == 2 ? m02 : m03;
                    case 1: return col == 0 ? m10 : col == 1 ? m11 : col == 2 ? m12 : m13;
                    case 2: return col == 0 ? m20 : col == 1 ? m21 : col == 2 ? m22 : m23;
                    case 3: return col == 0 ? m30 : col == 1 ? m31 : col == 2 ? m32 : m33;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (row)
                {
                    case 0: if (col == 0) m00 = value; else if (col == 1) m01 = value; else if (col == 2) m02 = value; else m03 = value; break;
                    case 1: if (col == 0) m10 = value; else if (col == 1) m11 = value; else if (col == 2) m12 = value; else m13 = value; break;
                    case 2: if (col == 0) m20 = value; else if (col == 1) m21 = value; else if (col == 2) m22 = value; else m23 = value; break;
                    case 3: if (col == 0) m30 = value; else if (col == 1) m31 = value; else if (col == 2) m32 = value; else m33 = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public static Matrix4x4D operator *(Matrix4x4D a, Matrix4x4D b)
        {
            var r = new Matrix4x4D();
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 4; col++)
                {
                    float sum = 0;
                    for (int k = 0; k < 4; k++) sum += a[row, k] * b[k, col];
                    r[row, col] = sum;
                }
            return r;
        }

        public static Vector4D operator *(Matrix4x4D a, Vector4D v)
        {
            return new Vector4D(
                a.m00 * v.x + a.m01 * v.y + a.m02 * v.z + a.m03 * v.w,
                a.m10 * v.x + a.m11 * v.y + a.m12 * v.z + a.m13 * v.w,
                a.m20 * v.x + a.m21 * v.y + a.m22 * v.z + a.m23 * v.w,
                a.m30 * v.x + a.m31 * v.y + a.m32 * v.z + a.m33 * v.w);
        }

        public Matrix4x4D Transpose()
        {
            return new Matrix4x4D
            {
                m00 = m00, m01 = m10, m02 = m20, m03 = m30,
                m10 = m01, m11 = m11, m12 = m21, m13 = m31,
                m20 = m02, m21 = m12, m22 = m22, m23 = m32,
                m30 = m03, m31 = m13, m32 = m23, m33 = m33
            };
        }

        public static Matrix4x4D Scale(Vector4D s) => new Matrix4x4D { m00 = s.x, m11 = s.y, m22 = s.z, m33 = s.w };

        // ---------------- 6つの基本回転面 ----------------
        // 4次元に「回転軸」は存在しない。3次元の回転軸は実は「回転しない平面の法線」を
        // 表現するための便宜的な代用品で、次元が上がると軸そのものが意味を失う。
        // 回転は常に「平面」を基準に起きるもので、4次元には独立な2次元平面が
        // 6つ(XY, XZ, XW, YZ, YW, ZW)ある。それぞれが単独で回転を持てる。

        public static Matrix4x4D RotationXY(float angleDeg) => PlaneRotation(0, 1, angleDeg);
        public static Matrix4x4D RotationXZ(float angleDeg) => PlaneRotation(0, 2, angleDeg);
        public static Matrix4x4D RotationXW(float angleDeg) => PlaneRotation(0, 3, angleDeg);
        public static Matrix4x4D RotationYZ(float angleDeg) => PlaneRotation(1, 2, angleDeg);
        public static Matrix4x4D RotationYW(float angleDeg) => PlaneRotation(1, 3, angleDeg);
        public static Matrix4x4D RotationZW(float angleDeg) => PlaneRotation(2, 3, angleDeg);

        private static Matrix4x4D PlaneRotation(int axisA, int axisB, float angleDeg)
        {
            var r = identity;
            float rad = angleDeg * Mathf.Deg2Rad;
            float c = Mathf.Cos(rad);
            float s = Mathf.Sin(rad);
            r[axisA, axisA] = c; r[axisA, axisB] = -s;
            r[axisB, axisA] = s; r[axisB, axisB] = c;
            return r;
        }

        /// <summary>
        /// 「二重回転」。SO(4)の一般の回転は、互いに直交する2つの回転面それぞれに
        /// 独立した回転角を持つ形に分解できる、というのが4次元回転論のキモ。
        /// ここでは2つの平面回転を単純合成するだけの簡易版。厳密に任意の回転から
        /// 直交補平面ペアを自動抽出したいなら固有値分解が必要になる。
        /// </summary>
        public static Matrix4x4D DoubleRotation(int planeA0, int planeA1, float angleA,
                                                  int planeB0, int planeB1, float angleB)
        {
            return PlaneRotation(planeB0, planeB1, angleB) * PlaneRotation(planeA0, planeA1, angleA);
        }

        public override string ToString()
        {
            return $"[{m00:F2} {m01:F2} {m02:F2} {m03:F2}]\n" +
                   $"[{m10:F2} {m11:F2} {m12:F2} {m13:F2}]\n" +
                   $"[{m20:F2} {m21:F2} {m22:F2} {m23:F2}]\n" +
                   $"[{m30:F2} {m31:F2} {m32:F2} {m33:F2}]";
        }
    }
}
