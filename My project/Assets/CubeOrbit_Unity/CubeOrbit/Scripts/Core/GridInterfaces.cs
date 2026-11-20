using UnityEngine;

namespace CubeOrbit
{
    /// <summary>
    /// マスの状態を問い合わせる窓口。
    /// ゲーム中は GridWorld.Live（時間で動くギミック込み）、
    /// エディタの検証では SolverQuery（時間を無視した簡易版）を使う。
    /// </summary>
    public interface IGridQuery
    {
        /// <summary>そのマスに何か固いものがあるか（進めない）</summary>
        bool Block(Vector3Int c);
        /// <summary>そのマスによじ登れるか</summary>
        bool Climbable(Vector3Int c);
        /// <summary>そのマスの上に安全に立てるか</summary>
        bool Floor(Vector3Int c);
        /// <summary>動く床が通過するかもしれないので、体を入れるのを待つべきか</summary>
        bool BodyWait(Vector3Int c);
        /// <summary>足場になる予定だが今は動く床がいないので待つべきか</summary>
        bool FloorWait(Vector3Int c);
        /// <summary>そのマスが止まっている動く床か</summary>
        bool IsPlatform(Vector3Int c);
        /// <summary>これより下に落ちたらアウトになる高さ</summary>
        int KillY { get; }
    }

    /// <summary>プレイヤーがそのマスに入った時に呼ばれる（ジェム、スイッチ、ゴールなど）</summary>
    public interface IPlayerEnterHandler
    {
        /// <param name="passingThrough">落下中に通過しただけなら true</param>
        void OnPlayerEnter(PlayerCube player, bool passingThrough);
    }

    /// <summary>プレイヤーがこのブロックの上に乗った時に呼ばれる（崩れる床など）</summary>
    public interface IPlayerLandHandler
    {
        void OnPlayerLand(PlayerCube player);
    }

    /// <summary>落下してやり直した時に元に戻すもの</summary>
    public interface IStageResettable
    {
        void OnStageReset();
    }
}
