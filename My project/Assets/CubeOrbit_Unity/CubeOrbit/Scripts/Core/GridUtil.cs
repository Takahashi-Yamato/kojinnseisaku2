using UnityEngine;

namespace CubeOrbit
{
    /// <summary>グリッド上の4方向。押し出す壁の向きなどに使う。</summary>
    public enum GridDir
    {
        [InspectorName("+X（画面の右下へ）")] PlusX,
        [InspectorName("-X（画面の左上へ）")] MinusX,
        [InspectorName("+Z（画面の左下へ）")] PlusZ,
        [InspectorName("-Z（画面の右上へ）")] MinusZ,
    }

    /// <summary>
    /// グリッド計算の共通ヘルパー。
    /// 1マス = 1ユニット。ブロックやキューブの「中心」が整数座標に来る。
    /// </summary>
    public static class GridUtil
    {
        public static readonly Vector3Int[] Directions =
        {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1),
        };

        /// <summary>スイッチ番号ごとの色（透明ブロックとスイッチで共通）</summary>
        public static readonly Color[] GroupColors =
        {
            new Color(1f, 1f, 1f),
            new Color(0.45f, 1f, 0.75f),
            new Color(1f, 0.42f, 0.85f),
            new Color(1f, 0.86f, 0.3f),
            new Color(0.45f, 0.7f, 1f),
            new Color(1f, 0.55f, 0.35f),
            new Color(0.7f, 0.5f, 1f),
        };

        public static Color GroupColor(int group)
        {
            if (group < 0) group = 0;
            return GroupColors[group % GroupColors.Length];
        }

        public static Vector3Int ToVector(this GridDir d) { return Directions[(int)d]; }

        public static GridDir FromVector(Vector3Int v)
        {
            for (int i = 0; i < Directions.Length; i++) if (Directions[i] == v) return (GridDir)i;
            return GridDir.PlusX;
        }

        public static Vector3Int ToCell(Vector3 p) { return Vector3Int.RoundToInt(p); }

        public static float EaseInOut(float t) { return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f; }

        public static float Smooth(float t) { return t * t * (3f - 2f * t); }

        /// <summary>Unityのバージョン差を吸収してシーン内のオブジェクトを全部探す</summary>
        public static T[] FindAll<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
            return Object.FindObjectsOfType<T>();
#endif
        }

        public static T FindOne<T>() where T : Object
        {
#if UNITY_2022_2_OR_NEWER
            return Object.FindAnyObjectByType<T>();
#else
            return Object.FindObjectOfType<T>();
#endif
        }
    }
}
