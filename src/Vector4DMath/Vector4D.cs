using System;
using UnityEngine;

namespace Vector4DMath
{
    /// <summary>
    /// 真の4次元ベクトル。
    /// UnityEngine.Vector4は「同次座標(w成分)付きの3Dベクトル」であって、
    /// x,y,z,wを対等な4本の軸として扱う4次元幾何とは似て非なるもの。
    /// このVector4Dは、Vector3が持つ機能一式をそのまま4次元へ拡張したもの。
    /// </summary>
    [Serializable]
    public struct Vector4D : IEquatable<Vector4D>
    {
        public float x, y, z, w;

        public Vector4D(float x, float y, float z, float w)
        {
            this.x = x; this.y = y; this.z = z; this.w = w;
        }

        public Vector4D(Vector3 xyz, float w = 0f)
        {
            x = xyz.x; y = xyz.y; z = xyz.z; this.w = w;
        }

        public float this[int index]
        {
            readonly get
            {
                switch (index)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    case 3: return w;
                    default: throw new IndexOutOfRangeException("Vector4D index must be 0-3");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    case 3: w = value; break;
                    default: throw new IndexOutOfRangeException("Vector4D index must be 0-3");
                }
            }
        }

        // ---------------- 定数 ----------------
        public static Vector4D Zero => new Vector4D(0, 0, 0, 0);
        public static Vector4D One => new Vector4D(1, 1, 1, 1);
        public static Vector4D UnitX => new Vector4D(1, 0, 0, 0);
        public static Vector4D UnitY => new Vector4D(0, 1, 0, 0);
        public static Vector4D UnitZ => new Vector4D(0, 0, 1, 0);
        public static Vector4D UnitW => new Vector4D(0, 0, 0, 1);
        public static Vector4D PositiveInfinity => new Vector4D(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static Vector4D NegativeInfinity => new Vector4D(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        // ---------------- プロパティ ----------------
        public readonly float SqrMagnitude => x * x + y * y + z * z + w * w;
        public readonly float Magnitude => Mathf.Sqrt(SqrMagnitude);

        public readonly Vector4D Normalized
        {
            get
            {
                float m = Magnitude;
                return m > 1e-9f ? this / m : Zero;
            }
        }

        public void Set(float nx, float ny, float nz, float nw)
        {
            x = nx; y = ny; z = nz; w = nw;
        }

        public void Normalize()
        {
            float m = Magnitude;
            if (m > 1e-9f) { x /= m; y /= m; z /= m; w /= m; }
            else { x = y = z = w = 0; }
        }

        // ---------------- 静的演算 ----------------
        public static float Dot(Vector4D a, Vector4D b) => a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;

        public static float Distance(Vector4D a, Vector4D b) => (a - b).Magnitude;
        public static float SqrDistance(Vector4D a, Vector4D b) => (a - b).SqrMagnitude;

        public static Vector4D Lerp(Vector4D a, Vector4D b, float t)
        {
            t = Mathf.Clamp01(t);
            return LerpUnclamped(a, b, t);
        }

        public static Vector4D LerpUnclamped(Vector4D a, Vector4D b, float t)
        {
            return new Vector4D(
                a.x + (b.x - a.x) * t,
                a.y + (b.y - a.y) * t,
                a.z + (b.z - a.z) * t,
                a.w + (b.w - a.w) * t);
        }

        /// <summary>
        /// 球面線形補間。次元に依存しない定義(内積と角度だけ)だから、4次元でもそのまま通用する。
        /// </summary>
        public static Vector4D Slerp(Vector4D a, Vector4D b, float t)
        {
            float magA = a.Magnitude;
            float magB = b.Magnitude;
            if (magA < 1e-9f || magB < 1e-9f) return LerpUnclamped(a, b, Mathf.Clamp01(t));

            Vector4D an = a / magA;
            Vector4D bn = b / magB;
            float dot = Mathf.Clamp(Dot(an, bn), -1f, 1f);
            float theta = Mathf.Acos(dot);

            if (theta < 1e-6f) return Lerp(a, b, t); // ほぼ同方向なら通常のlerpで十分

            float tt = Mathf.Clamp01(t);
            float sinTheta = Mathf.Sin(theta);
            float wa = Mathf.Sin((1 - tt) * theta) / sinTheta;
            float wb = Mathf.Sin(tt * theta) / sinTheta;
            float mag = Mathf.Lerp(magA, magB, tt);
            return (an * wa + bn * wb) * mag;
        }

        public static Vector4D MoveTowards(Vector4D current, Vector4D target, float maxDistanceDelta)
        {
            Vector4D toVector = target - current;
            float dist = toVector.Magnitude;
            if (dist <= maxDistanceDelta || dist < 1e-9f) return target;
            return current + toVector / dist * maxDistanceDelta;
        }

        public static float Angle(Vector4D a, Vector4D b)
        {
            float denom = a.Magnitude * b.Magnitude;
            if (denom < 1e-15f) return 0f;
            float cos = Mathf.Clamp(Dot(a, b) / denom, -1f, 1f);
            return Mathf.Acos(cos) * Mathf.Rad2Deg;
        }
        // 補足: 4次元にSignedAngleは存在しない。符号付き角度は「回転面」を1つ固定して
        // 初めて意味を持つ概念で、3次元みたいに軸1本じゃ足りないんだよね。

        public static Vector4D Project(Vector4D v, Vector4D onNormal)
        {
            float sqrMag = Dot(onNormal, onNormal);
            if (sqrMag < 1e-15f) return Zero;
            return onNormal * (Dot(v, onNormal) / sqrMag);
        }

        /// <summary>
        /// 法線ベクトル1本が定義する「超平面(3次元の部分空間)」への射影。
        /// </summary>
        public static Vector4D ProjectOnHyperplane(Vector4D v, Vector4D planeNormal)
        {
            return v - Project(v, planeNormal);
        }

        public static Vector4D Reflect(Vector4D v, Vector4D normal)
        {
            return v - 2f * Dot(v, normal) * normal;
        }

        public static Vector4D ClampMagnitude(Vector4D v, float maxLength)
        {
            if (v.SqrMagnitude > maxLength * maxLength) return v.Normalized * maxLength;
            return v;
        }

        public static Vector4D Scale(Vector4D a, Vector4D b) => new Vector4D(a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
        public static Vector4D Min(Vector4D a, Vector4D b) => new Vector4D(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Min(a.z, b.z), Mathf.Min(a.w, b.w));
        public static Vector4D Max(Vector4D a, Vector4D b) => new Vector4D(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y), Mathf.Max(a.z, b.z), Mathf.Max(a.w, b.w));

        /// <summary>
        /// 4次元版の外積。3次元と違って2本のベクトルからは作れない。
        /// n次元空間で「全部に直交する1本」を作るにはn-1本のベクトルが要る、というだけの話で、
        /// 3次元の(2本→1本)はn-1=2のたまたまの特例に過ぎない。
        /// ここでは3本のベクトルに直交する4本目を4x4行列式の余因子展開で求める。
        /// </summary>
        public static Vector4D Cross(Vector4D a, Vector4D b, Vector4D c)
        {
            static float Det3(float a1, float a2, float a3, float b1, float b2, float b3, float c1, float c2, float c3)
                => a1 * (b2 * c3 - b3 * c2) - a2 * (b1 * c3 - b3 * c1) + a3 * (b1 * c2 - b2 * c1);

            float cx =  Det3(a.y, a.z, a.w, b.y, b.z, b.w, c.y, c.z, c.w);
            float cy = -Det3(a.x, a.z, a.w, b.x, b.z, b.w, c.x, c.z, c.w);
            float cz =  Det3(a.x, a.y, a.w, b.x, b.y, b.w, c.x, c.y, c.w);
            float cw = -Det3(a.x, a.y, a.z, b.x, b.y, b.z, c.x, c.y, c.z);
            return new Vector4D(cx, cy, cz, cw);
        }

        // ---------------- 演算子 ----------------
        public static Vector4D operator +(Vector4D a, Vector4D b) => new Vector4D(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        public static Vector4D operator -(Vector4D a, Vector4D b) => new Vector4D(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        public static Vector4D operator -(Vector4D a) => new Vector4D(-a.x, -a.y, -a.z, -a.w);
        public static Vector4D operator *(Vector4D a, float d) => new Vector4D(a.x * d, a.y * d, a.z * d, a.w * d);
        public static Vector4D operator *(float d, Vector4D a) => new Vector4D(a.x * d, a.y * d, a.z * d, a.w * d);
        public static Vector4D operator /(Vector4D a, float d) => new Vector4D(a.x / d, a.y / d, a.z / d, a.w / d);
        public static bool operator ==(Vector4D a, Vector4D b) => (a - b).SqrMagnitude < 1e-10f;
        public static bool operator !=(Vector4D a, Vector4D b) => !(a == b);

        public readonly bool Equals(Vector4D other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z) && w.Equals(other.w);
        public override readonly bool Equals(object obj) => obj is Vector4D other && Equals(other);

        public override readonly int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + x.GetHashCode();
                hash = hash * 31 + y.GetHashCode();
                hash = hash * 31 + z.GetHashCode();
                hash = hash * 31 + w.GetHashCode();
                return hash;
            }
        }

        public override readonly string ToString() => $"({x:F2}, {y:F2}, {z:F2}, {w:F2})";

        /// <summary>wを切り捨てて3次元へ落とす。射影ではなくただの成分カット。</summary>
        public readonly Vector3 ToVector3() => new Vector3(x, y, z);
    }
}
