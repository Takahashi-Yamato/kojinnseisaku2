using System.Collections.Generic;
using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// ステージ検証用の問い合わせ。時間を無視して
    /// 「動く床は両端にいる」「押し出す壁は引っ込んでいる」「崩れる床は崩れていない」とみなす。
    /// </summary>
    public class SolverQuery : IGridQuery
    {
        readonly IList<GridElement> elements;
        public int mask;
        readonly int killY;

        public SolverQuery(IList<GridElement> elements, int killY) { this.elements = elements; this.killY = killY; }

        public bool Block(Vector3Int c) { foreach (var e in elements) if (e.SolverSolid(c, mask)) return true; return false; }
        public bool Climbable(Vector3Int c) { return Block(c); }
        public bool Floor(Vector3Int c) { return Block(c); }
        public bool BodyWait(Vector3Int c) { return false; }
        public bool FloorWait(Vector3Int c) { return false; }
        public bool IsPlatform(Vector3Int c) { return false; }
        public int KillY { get { return killY; } }
    }

    public class SolverResult
    {
        public bool goalFound;
        public bool goalReachable;
        public List<Gem> unreachableGems = new List<Gem>();
        public int totalGems;
        public int statesSearched;
    }

    /// <summary>
    /// 幅優先探索でステージを総当たりし、ゴールとジェムにたどり着けるかを調べる。
    /// タイミング（押し出す壁の間隔など）までは見ないので、そこは実際に遊んで確認する。
    /// </summary>
    public static class GridSolver
    {
        public static SolverResult Validate(IList<GridElement> elements, Vector3Int start, int killY)
        {
            var res = new SolverResult();
            var q = new SolverQuery(elements, killY);

            var goals = new HashSet<Vector3Int>();
            var gems = new List<Gem>();
            var switches = new Dictionary<Vector3Int, int>();
            var platforms = new List<MovingPlatform>();
            foreach (var e in elements)
            {
                if (e is Goal) goals.Add(e.Cell);
                else if (e is Gem) gems.Add((Gem)e);
                else if (e is FloorSwitch) switches[e.Cell] = ((FloorSwitch)e).group;
                else if (e is MovingPlatform) platforms.Add((MovingPlatform)e);
            }
            res.goalFound = goals.Count > 0;
            res.totalGems = gems.Count;

            var reached = new HashSet<Vector3Int>();
            var seen = new HashSet<long>();
            var queue = new Queue<KeyValuePair<Vector3Int, int>>();
            queue.Enqueue(new KeyValuePair<Vector3Int, int>(start, 0));
            seen.Add(Key(start, 0));

            while (queue.Count > 0 && seen.Count < 200000)
            {
                var cur = queue.Dequeue();
                Vector3Int p = cur.Key; int m = cur.Value;
                reached.Add(p);
                if (goals.Contains(p)) continue; // ゴールに触れたらステージ終了なのでその先は探さない
                q.mask = m;

                foreach (var d in GridUtil.Directions)
                {
                    var r = MoveRules.Resolve(q, p, d);
                    if (r.type == MoveType.Bump || r.type == MoveType.Wait || !r.hasLanding) continue;
                    if (r.type == MoveType.Roll && r.falls)
                        for (int y = r.to.y; y >= r.landing.y; y--) reached.Add(new Vector3Int(r.to.x, y, r.to.z));
                    Push(queue, seen, r.landing, m, switches);
                }

                foreach (var mp in platforms)
                {
                    Vector3Int a = mp.StartCell + Vector3Int.up, b = mp.EndCell + Vector3Int.up;
                    if (p == a && !q.Block(b)) Push(queue, seen, b, m, switches);
                    if (p == b && !q.Block(a)) Push(queue, seen, a, m, switches);
                }
            }

            res.statesSearched = seen.Count;
            foreach (var g in goals) if (reached.Contains(g)) res.goalReachable = true;
            foreach (var g in gems) if (!reached.Contains(g.Cell)) res.unreachableGems.Add(g);
            return res;
        }

        static void Push(Queue<KeyValuePair<Vector3Int, int>> queue, HashSet<long> seen, Vector3Int np, int m, Dictionary<Vector3Int, int> switches)
        {
            int g;
            if (switches.TryGetValue(np, out g) && g >= 0 && g < 31) m |= 1 << g;
            long k = Key(np, m);
            if (seen.Add(k)) queue.Enqueue(new KeyValuePair<Vector3Int, int>(np, m));
        }

        static long Key(Vector3Int c, int m)
        {
            // 座標は ±1000 程度まで想定
            long x = c.x + 1024, y = c.y + 1024, z = c.z + 1024;
            return (((x * 2048 + y) * 2048 + z) << 20) ^ (long)m;
        }
    }
}
