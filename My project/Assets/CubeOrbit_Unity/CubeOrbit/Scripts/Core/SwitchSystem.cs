using System;
using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// スイッチ番号（グループ）のON/OFFを管理する。
    /// スイッチを踏むと Activate され、同じ番号の透明ブロックが実体化する。
    /// </summary>
    public static class SwitchSystem
    {
        static readonly HashSet<int> active = new HashSet<int>();

        /// <summary>グループがONになった時（番号, 踏んだ位置）</summary>
        public static event Action<int, Vector3> Activated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() { active.Clear(); Activated = null; }

        public static bool IsActive(int group) { return active.Contains(group); }

        public static void Activate(int group, Vector3 from)
        {
            if (active.Add(group) && Activated != null) Activated(group, from);
        }

        public static void ResetAll() { active.Clear(); }

        /// <summary>ONのグループをビットにしたもの（検証ツール用）</summary>
        public static int Mask
        {
            get
            {
                int m = 0;
                foreach (var g in active) if (g >= 0 && g < 31) m |= 1 << g;
                return m;
            }
        }
    }
}
