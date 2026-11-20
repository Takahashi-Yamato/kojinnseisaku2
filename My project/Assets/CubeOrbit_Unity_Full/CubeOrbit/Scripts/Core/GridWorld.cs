using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// ゲーム中のグリッド全体を管理する。
    /// 各 GridElement が自分を登録し、「このマスは固い？」などの問い合わせをまとめて答える。
    /// </summary>
    public static class GridWorld
    {
        static readonly List<GridElement> elements = new List<GridElement>();
        static readonly List<GridElement> temp = new List<GridElement>();

        public static IReadOnlyList<GridElement> Elements { get { return elements; } }

        /// <summary>ゲーム中に使う問い合わせ窓口</summary>
        public static readonly LiveQuery Live = new LiveQuery();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init() { elements.Clear(); }

        public static void Register(GridElement e) { if (!elements.Contains(e)) elements.Add(e); }
        public static void Unregister(GridElement e) { elements.Remove(e); }

        /// <summary>プレイヤーが cell に入ったことを、そのマスにいる要素へ伝える</summary>
        public static void NotifyEnter(PlayerCube player, Vector3Int cell, bool passingThrough)
        {
            temp.Clear(); temp.AddRange(elements);
            foreach (var e in temp)
            {
                if (e == null || !e.isActiveAndEnabled) continue;
                var h = e as IPlayerEnterHandler;
                if (h != null && e.Cell == cell) h.OnPlayerEnter(player, passingThrough);
            }
        }

        /// <summary>プレイヤーが floorCell のブロックに乗ったことを伝える</summary>
        public static void NotifyLand(PlayerCube player, Vector3Int floorCell)
        {
            temp.Clear(); temp.AddRange(elements);
            foreach (var e in temp)
            {
                var h = e as IPlayerLandHandler;
                if (h != null && e.Blocks(floorCell)) h.OnPlayerLand(player);
            }
        }

        /// <summary>落下してやり直す時に、崩れた床などを元に戻す</summary>
        public static void ResetStage()
        {
            temp.Clear(); temp.AddRange(elements);
            foreach (var e in temp)
            {
                var r = e as IStageResettable;
                if (r != null) r.OnStageReset();
            }
        }

        public static MovingPlatform PlatformAt(Vector3Int c)
        {
            foreach (var e in elements)
            {
                var mp = e as MovingPlatform;
                if (mp != null && mp.Blocks(c)) return mp;
            }
            return null;
        }

        public static Pusher PusherHeadAt(Vector3Int c)
        {
            foreach (var e in elements)
            {
                var p = e as Pusher;
                if (p != null && p.IsExtendedInto(c)) return p;
            }
            return null;
        }

        /// <summary>置かれている物の一番低い y（自動の落下ライン用）</summary>
        public static int LowestY(IEnumerable<GridElement> list)
        {
            int low = int.MaxValue;
            foreach (var e in list)
            {
                Vector3Int min, max; e.GetBounds(out min, out max);
                if (min.y < low) low = min.y;
            }
            return low == int.MaxValue ? 0 : low;
        }
    }

    /// <summary>ゲーム中の問い合わせ。登録されている全要素に順番に聞く。</summary>
    public class LiveQuery : IGridQuery
    {
        IReadOnlyList<GridElement> E { get { return GridWorld.Elements; } }

        public bool Block(Vector3Int c) { foreach (var e in E) if (e.Blocks(c)) return true; return false; }
        public bool Climbable(Vector3Int c) { foreach (var e in E) if (e.Climbable(c)) return true; return false; }
        public bool Floor(Vector3Int c) { foreach (var e in E) if (e.IsFloor(c)) return true; return false; }
        public bool BodyWait(Vector3Int c) { foreach (var e in E) if (e.BodyWait(c)) return true; return false; }
        public bool FloorWait(Vector3Int c) { foreach (var e in E) if (e.FloorWait(c)) return true; return false; }
        public bool IsPlatform(Vector3Int c) { return GridWorld.PlatformAt(c) != null; }
        public int KillY { get { return GameManager.Instance != null ? GameManager.Instance.KillY : -12; } }
    }
}
