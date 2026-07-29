using UnityEngine;

namespace Vector4DMath
{
    /// <summary>
    /// 4次元回転を「2つのクォータニオンの組」で表現するローター。
    /// R^4は四元数体Hと同一視できて、v' = p * v * q^-1 というサンドイッチ積で
    /// SO(4)の任意の回転(二重回転)を表現できる。これはUnityのQuaternionが
    /// そのままHamilton積として使えることを利用したトリック。
    ///
    /// p, qが等しければ全ての点が同じ角度で回る「等傾回転(isoclinic rotation)」、
    /// qを恒等元にすれば見慣れた3次元的な回転に帰着する。
    /// </summary>
    [System.Serializable]
    public struct Rotor4D
    {
        public Quaternion left;   // p
        public Quaternion right;  // q

        public Rotor4D(Quaternion left, Quaternion right)
        {
            this.left = left;
            this.right = right;
        }

        public static Rotor4D identity => new Rotor4D(Quaternion.identity, Quaternion.identity);

        /// <summary>左等傾回転のみ(q = identity)。3次元的な直感に一番近い回転。</summary>
        public static Rotor4D LeftIsoclinic(Quaternion p) => new Rotor4D(p, Quaternion.identity);

        /// <summary>右等傾回転のみ(p = identity)。</summary>
        public static Rotor4D RightIsoclinic(Quaternion q) => new Rotor4D(Quaternion.identity, q);

        /// <summary>
        /// 完全な等傾回転。空間全体が同じ角度でねじれる、4次元にしか存在しない回転の形。
        /// </summary>
        public static Rotor4D Isoclinic(Quaternion p) => new Rotor4D(p, p);

        public Vector4D Rotate(Vector4D v)
        {
            Quaternion qv = new Quaternion(v.x, v.y, v.z, v.w);
            Quaternion result = left * qv * Quaternion.Inverse(right);
            return new Vector4D(result.x, result.y, result.z, result.w);
        }

        /// <summary>this の後に other を適用する合成(先に other、次に this)。</summary>
        public Rotor4D Compose(Rotor4D other)
        {
            return new Rotor4D(left * other.left, other.right * right);
        }

        public Matrix4x4D ToMatrix()
        {
            Vector4D e0 = Rotate(new Vector4D(1, 0, 0, 0));
            Vector4D e1 = Rotate(new Vector4D(0, 1, 0, 0));
            Vector4D e2 = Rotate(new Vector4D(0, 0, 1, 0));
            Vector4D e3 = Rotate(new Vector4D(0, 0, 0, 1));

            var m = new Matrix4x4D();
            m.m00 = e0.x; m.m10 = e0.y; m.m20 = e0.z; m.m30 = e0.w;
            m.m01 = e1.x; m.m11 = e1.y; m.m21 = e1.z; m.m31 = e1.w;
            m.m02 = e2.x; m.m12 = e2.y; m.m22 = e2.z; m.m32 = e2.w;
            m.m03 = e3.x; m.m13 = e3.y; m.m23 = e3.z; m.m33 = e3.w;
            return m;
        }
    }
}
