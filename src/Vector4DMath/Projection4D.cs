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

        public static class Pentachoron
        {
            /// <summary>
            /// 正五胞体の5頂点を生成する。
            /// </summary>
            public static Vector4D[] GenerateVertices(float size = 1f)
            {
                // 5次元の単位ベクトル e0..e4 を、重心(1/5,1/5,1/5,1/5,1/5)基準に
                // 「和が0になる4次元部分空間」へ射影してから、4成分だけ取り出す。
                // （5次元目の成分は射影後常に従属するため、先頭4成分のみで4D座標として扱える）
                var verts = new Vector4D[5];
                float[][] e = new float[5][];
                for (int i = 0; i < 5; i++)
                {
                    e[i] = new float[5];
                    e[i][i] = 1f;
                }

                // 重心を引いて中心化（これで「和が0」の超平面に乗る）
                for (int i = 0; i < 5; i++)
                    for (int k = 0; k < 5; k++)
                        e[i][k] -= 0.2f; // 1/5

                // 正規化：どの頂点も原点から同じ距離になるようスケールを揃える
                float norm = Mathf.Sqrt(e[0][0] * e[0][0] + e[0][1] * e[0][1] + e[0][2] * e[0][2] + e[0][3] * e[0][3] + e[0][4] * e[0][4]);

                for (int i = 0; i < 5; i++)
                    verts[i] = new Vector4D(e[i][0] / norm * size, e[i][1] / norm * size, e[i][2] / norm * size, e[i][3] / norm * size);

                return verts;
            }

            /// <summary>
            /// 正五胞体の辺(頂点インデックスのペア10本)。単体なので全頂点対が辺になる
            /// （5頂点から2つ選ぶ組み合わせ、C(5,2)=10）。
            /// </summary>
            public static (int a, int b)[] GenerateEdges()
            {
                var edges = new System.Collections.Generic.List<(int, int)>();
                for (int i = 0; i < 5; i++)
                    for (int j = i + 1; j < 5; j++)
                        edges.Add((i, j));
                return edges.ToArray();
            }
        }

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

        /// <summary>
        /// 正二十四胞体(24-cell)。4次元にしか存在しない、3次元に対応物の無い多胞体。
        /// 頂点は「2成分が±1、残り2成分が0」の全組み合わせ、C(4,2)×4=24個。
        /// 双対を取っても自分自身になる(self-dual)という珍しい性質を持つ。
        /// </summary>
        public static class Icositetrachoron
        {
            /// <summary>
            /// 正二十四胞体の24頂点を生成する。
            /// </summary>
            public static Vector4D[] GenerateVertices(float size = 1f)
            {
                var verts = new System.Collections.Generic.List<Vector4D>();
                int[] axes = { 0, 1, 2, 3 };

                for (int a = 0; a < 4; a++)
                    for (int b = a + 1; b < 4; b++)
                        for (int sa = -1; sa <= 1; sa += 2)
                            for (int sb = -1; sb <= 1; sb += 2)
                            {
                                var v = new Vector4D(0, 0, 0, 0);
                                v[a] = sa * size;
                                v[b] = sb * size;
                                verts.Add(v);
                            }

                return verts.ToArray(); // C(4,2)=6通り × 2 × 2 = 24
            }

            /// <summary>
            /// 正二十四胞体の辺(頂点インデックスのペア96本)。
            /// 24-cellの辺長は頂点間距離√2が最小距離になるため、最小距離ペアを辺として抽出する
            /// （3D版PolygonShapeRenderer.Factory.SnubDodecahedronと同じ「最小距離ペア抽出」方式）。
            /// </summary>
            public static (int a, int b)[] GenerateEdges()
            {
                Vector4D[] verts = GenerateVertices(1f);
                int n = verts.Length;

                float minSq = float.MaxValue;
                for (int i = 0; i < n; i++)
                    for (int j = i + 1; j < n; j++)
                    {
                        float d = Vector4D.SqrDistance(verts[i], verts[j]);
                        if (d < minSq) minSq = d;
                    }

                float threshold = minSq * 1.01f;
                var edges = new System.Collections.Generic.List<(int, int)>();
                for (int i = 0; i < n; i++)
                    for (int j = i + 1; j < n; j++)
                        if (Vector4D.SqrDistance(verts[i], verts[j]) <= threshold)
                            edges.Add((i, j));

                return edges.ToArray(); // 24頂点×8本/2 = 96辺になるはず
            }
        }
    }
}