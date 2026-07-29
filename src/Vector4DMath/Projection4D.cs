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

        /// <summary>
        /// テッセラクトの16頂点を生成する。(±size, ±size, ±size, ±size)の
        /// 全組み合わせがそのまま頂点になる。
        /// </summary>
        public static Vector4D[] GenerateTesseractVertices(float size = 1f)
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
        public static (int a, int b)[] GenerateTesseractEdges()
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
}
