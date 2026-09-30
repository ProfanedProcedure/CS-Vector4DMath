using UnityEngine;

namespace Vector4DMath
{
    /// <summary>
    /// 4次元の点を3次元へ投影するユーティリティ。
    /// テッセラクト(4次元超立方体)を画面に映すには、まず4D→3Dの「影」を作る
    /// 工程が要る。地味だけどここを飛ばすと何も見えないよ。
    /// </summary>
    public static class Projection4D
    {
        /// <summary>
        /// 4次元の透視投影。カメラがw軸上、原点からwDistanceだけ離れた位置にあると仮定し、
        /// wが大きい(カメラに近い)点ほど大きく見えるようx,y,zをスケールする。
        /// </summary>
        public static Vector3 ProjectPerspective(Vector4D point, float wDistance = 3f)
        {
            float denom = wDistance - point.w;
            if (Mathf.Abs(denom) < 1e-6f) denom = 1e-6f;
            float scale = wDistance / denom;
            return new Vector3(point.x * scale, point.y * scale, point.z * scale);
        }

        /// <summary>単純にwを切り捨てるだけの平行投影。奥行き感は失われるが計算は軽い。</summary>
        public static Vector3 ProjectOrthographic(Vector4D point) => new Vector3(point.x, point.y, point.z);

        public static class Tesseract
        {
            /// <summary>
            /// テッセラクトの16頂点を生成する。(±size, ±size, ±size, ±size)の
            /// 全組み合わせがそのまま頂点になる。
            /// </summary>
            public static Vector4D[] GenerateVertices(float size = 1f)
            {
                var verts = new Vector4D[16];
                for (int i = 0; i < 16; i++)
                {
                    float x = (i & 1) == 0 ? -size : size;
                    float y = (i & 2) == 0 ? -size : size;
                    float z = (i & 4) == 0 ? -size : size;
                    float w = (i & 8) == 0 ? -size : size;
                    verts[i] = new Vector4D(x, y, z, w);
                }
                return verts;
            }

            /// <summary>
            /// テッセラクトの辺(頂点インデックスのペア32本)。
            /// 隣接判定はビット表現で1ビットだけ異なる頂点同士。
            /// </summary>
            public static (int a, int b)[] GenerateEdges()
            {
                var edges = new System.Collections.Generic.List<(int, int)>();
                for (int i = 0; i < 16; i++)
                    for (int bit = 0; bit < 4; bit++)
                    {
                        int j = i ^ (1 << bit);
                        if (j > i) edges.Add((i, j));
                    }
                return edges.ToArray();
            }
        }

        /// <summary>
        /// 正十六胞体(16-cell / hyperoctahedron)。テッセラクトの双対にあたる多胞体。
        /// 頂点は4本の座標軸上に1つずつ（各軸の±方向）、計8個。
        /// テッセラクトが「全成分±1」(コーナー型)の頂点配置なのに対し、
        /// こちらは「1成分だけ非ゼロ」(軸上型)——双対関係らしい対照的な形になる。
        /// </summary>
        public static class Hexadecachoron
        {
            /// <summary>
            /// 正十六胞体の8頂点を生成する。
            /// 4本の座標軸それぞれについて、+size方向と-size方向の2点。
            /// </summary>
            public static Vector4D[] GenerateVertices(float size = 1f)
            {
                var verts = new Vector4D[8];
                for (int axis = 0; axis < 4; axis++)
                {
                    var plus = new Vector4D(0, 0, 0, 0);
                    plus[axis] = size;
                    verts[axis * 2] = plus;

                    var minus = new Vector4D(0, 0, 0, 0);
                    minus[axis] = -size;
                    verts[axis * 2 + 1] = minus;
                }
                return verts;
            }

            /// <summary>
            /// 正十六胞体の辺(頂点インデックスのペア24本)。
            /// 「同じ軸の±ペア(対蹠点)」同士だけは辺で結ばれず、それ以外の
            /// 全頂点ペアが辺になる。C(8,2)=28通りから対蹠点4組を除いて24本。
            /// GenerateVertices()の並び（軸ごとに+,-が連続）を前提に、
            /// 同じ軸ペアかどうかを i/2 == j/2 で判定している。
            /// </summary>
            public static (int a, int b)[] GenerateEdges()
            {
                var edges = new System.Collections.Generic.List<(int, int)>();
                for (int i = 0; i < 8; i++)
                    for (int j = i + 1; j < 8; j++)
                    {
                        bool isAntipodalPair = (i / 2) == (j / 2);
                        if (!isAntipodalPair) edges.Add((i, j));
                    }
                return edges.ToArray();
            }
        }
    }
}