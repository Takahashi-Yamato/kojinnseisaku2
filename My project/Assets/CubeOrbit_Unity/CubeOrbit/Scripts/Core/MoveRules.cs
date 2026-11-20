using UnityEngine;

namespace CubeOrbit
{
    public enum MoveType
    {
        Wait,   // 今は動けない（動く床待ちなど）。入力は捨てずに保留
        Bump,   // 壁にぶつかる
        Climb,  // 1段よじ登る
        Roll,   // 隣へ転がる（そのまま落ちることもある）
    }

    public struct MoveResult
    {
        public MoveType type;
        public Vector3Int to;          // 転がり/よじ登りの行き先
        public bool falls;             // 転がった後に落ちるか
        public bool hasLanding;        // 着地点があるか（false なら奈落）
        public Vector3Int landing;     // 最終的に止まるマス
    }

    /// <summary>
    /// 「この方向に動いたらどうなるか」を決めるルール。
    /// ゲーム本体とステージ検証ツールの両方がこれを使うので、ルールを変えたいときはここだけ直せばよい。
    /// </summary>
    public static class MoveRules
    {
        public static MoveResult Resolve(IGridQuery q, Vector3Int p, Vector3Int d)
        {
            var r = new MoveResult();
            Vector3Int t = p + d;

            if (q.Block(t))
            {
                Vector3Int upT = t + Vector3Int.up, upP = p + Vector3Int.up;
                bool canClimb = q.Climbable(t) && !q.Block(upT) && !q.Block(upP) && !q.BodyWait(upT) && !q.BodyWait(upP);
                if (canClimb)
                {
                    r.type = MoveType.Climb; r.to = upT; r.hasLanding = true; r.landing = upT;
                    return r;
                }
                r.type = q.IsPlatform(t) ? MoveType.Wait : MoveType.Bump;
                r.to = p;
                return r;
            }

            if (q.BodyWait(t)) { r.type = MoveType.Wait; return r; }

            int y = t.y;
            while (y > q.KillY)
            {
                var c = new Vector3Int(t.x, y - 1, t.z);
                if (q.Floor(c)) break;
                if (q.FloorWait(c) || q.Block(c)) { r.type = MoveType.Wait; return r; }
                y--;
            }

            r.type = MoveType.Roll;
            r.to = t;
            if (y <= q.KillY) { r.falls = true; r.hasLanding = false; return r; }
            r.landing = new Vector3Int(t.x, y, t.z);
            r.hasLanding = true;
            r.falls = y != t.y;
            return r;
        }
    }
}
