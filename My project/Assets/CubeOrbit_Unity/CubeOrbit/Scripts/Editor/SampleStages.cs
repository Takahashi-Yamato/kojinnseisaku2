// このファイルはWeb版の15ステージから自動生成したデータです。
// 「ステージエディタ > サンプルステージを生成」でシーンに変換されます。
namespace CubeOrbit.EditorTools
{
    public class StageData
    {
        public string name, hint;
        public float par;
        public int[] start, goal;            // プレイヤー開始マス / ゴールのマス
        public int[][] blocks;               // x, y, z, sizeX, sizeY, sizeZ
        public int[][] ghosts;               // x, y, z, sizeX, sizeY, sizeZ, group
        public int[][] gems;                 // x, y, z
        public int[][] switches;             // x, y, z, group
        public int[][] checkpoints;          // x, y, z
        public float[][] platforms;          // x, y, z, offsetX, offsetY, offsetZ, speed, pause, timeOffset
        public float[][] pushers;            // x, y, z, dirX, dirZ, period, timeOffset
        public int[][] crumbles;             // x, y, z
    }

    public static class SampleStages
    {
        public static readonly StageData[] All =
        {
            new StageData {
                name = "はじまりの軌道", hint = "矢印キー / WASD（スマホはスワイプ）で転がる。白いスイッチで道が出る。色の台がゴール", par = 30.0f,
                start = new[]{1,1,11}, goal = new[]{11,4,-8},
                blocks = new int[][]{new[]{0,-3,10,3,4,3},new[]{1,-3,4,1,4,6},new[]{1,-3,2,1,5,2},new[]{2,-3,2,5,5,1},new[]{4,-3,3,1,5,1},new[]{10,-3,0,3,5,5},new[]{9,-3,-6,2,8,6},new[]{11,-3,-4,1,5,4},new[]{11,-3,-5,1,6,1},new[]{11,-3,-6,1,7,1},new[]{10,-3,-9,3,7,3},new[]{-1,-3,8,1,7,6}},
                ghosts = new int[][]{new[]{7,1,2,3,1,1,1}},
                gems = new int[][]{new[]{1,1,7},new[]{4,2,2},new[]{8,2,2},new[]{12,2,4},new[]{11,2,-2}},
                switches = new int[][]{new[]{4,2,3,1}},
                checkpoints = new int[][]{new[]{10,2,2}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "チェッカー星雲", hint = "", par = 40.0f,
                start = new[]{1,1,1}, goal = new[]{14,5,-8},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{6,-3,-9,3,4,3},new[]{5,-3,-12,5,8,3},new[]{13,-3,-10,3,8,5},new[]{-1,-3,-3,1,7,7},new[]{3,-3,-6,1,4,1},new[]{5,-3,-6,1,4,1},new[]{7,-3,-6,1,4,1},new[]{9,-3,-6,1,4,1},new[]{3,-3,-5,1,4,1},new[]{4,-3,-5,1,4,1},new[]{7,-3,-5,1,4,1},new[]{9,-3,-5,1,4,1},new[]{4,-3,-4,1,4,1},new[]{6,-3,-4,1,4,1},new[]{7,-3,-4,1,4,1},new[]{8,-3,-4,1,4,1},new[]{4,-3,-3,1,4,1},new[]{5,-3,-3,1,4,1},new[]{6,-3,-3,1,4,1},new[]{8,-3,-3,1,4,1},new[]{3,-3,-2,1,4,1},new[]{6,-3,-2,1,4,1},new[]{8,-3,-2,1,4,1},new[]{9,-3,-2,1,4,1},new[]{3,-3,-1,1,4,1},new[]{5,-3,-1,1,4,1},new[]{6,-3,-1,1,4,1},new[]{8,-3,-1,1,4,1},new[]{3,-3,0,1,4,1},new[]{4,-3,0,1,4,1},new[]{5,-3,0,1,4,1},new[]{7,-3,0,1,4,1},new[]{8,-3,0,1,4,1},new[]{4,-3,1,1,4,1},new[]{7,-3,1,1,4,1},new[]{9,-3,1,1,4,1},new[]{3,-3,2,1,4,1},new[]{4,-3,2,1,4,1},new[]{6,-3,2,1,4,1},new[]{9,-3,2,1,4,1}},
                ghosts = new int[][]{new[]{9,1,-8,1,1,1,1},new[]{10,2,-8,1,1,1,1},new[]{11,3,-8,1,1,1,1},new[]{12,4,-8,1,1,1,1}},
                gems = new int[][]{new[]{8,1,0},new[]{9,1,-2},new[]{4,1,-5},new[]{11,4,-8}},
                switches = new int[][]{new[]{3,1,-6,1}},
                checkpoints = new int[][]{new[]{7,1,-8}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "タワーの谷", hint = "", par = 55.0f,
                start = new[]{1,3,1}, goal = new[]{16,4,-19},
                blocks = new int[][]{new[]{0,-3,0,3,6,3},new[]{3,-3,0,4,4,2},new[]{7,-3,0,1,5,2},new[]{8,-3,0,1,6,2},new[]{9,-3,0,1,7,2},new[]{10,-3,-3,3,7,5},new[]{11,-3,-8,1,7,5},new[]{12,-3,-6,1,8,1},new[]{10,-3,-12,3,4,4},new[]{10,-3,-20,3,4,4},new[]{15,-3,-21,3,7,7},new[]{-1,-3,-3,1,10,7},new[]{9,-3,-12,1,9,4},new[]{8,-3,-22,2,9,6}},
                ghosts = new int[][]{new[]{11,0,-16,1,1,4,1},new[]{13,0,-18,1,2,1,2},new[]{14,0,-18,1,3,1,2}},
                gems = new int[][]{new[]{11,4,1},new[]{12,5,-6},new[]{11,1,-15},new[]{10,1,-17},new[]{16,4,-16}},
                switches = new int[][]{new[]{10,1,-12,1},new[]{10,1,-20,2}},
                checkpoints = new int[][]{new[]{11,1,-10},new[]{11,1,-18}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "スイッチ回廊", hint = "", par = 60.0f,
                start = new[]{1,1,1}, goal = new[]{16,5,5},
                blocks = new int[][]{new[]{0,-3,0,4,4,3},new[]{8,-3,-1,3,4,5},new[]{8,-3,-8,3,7,4},new[]{7,-3,-9,1,10,6},new[]{15,-3,-8,3,7,5},new[]{15,-3,-3,3,5,4},new[]{15,-3,4,3,8,3},new[]{14,-3,-12,5,10,3},new[]{-1,-3,-2,1,8,6}},
                ghosts = new int[][]{new[]{4,0,1,4,1,1,1},new[]{9,0,-2,1,2,1,2},new[]{9,0,-3,1,3,1,2},new[]{9,0,-4,1,4,1,2},new[]{11,3,-6,4,1,1,3},new[]{16,0,1,1,3,1,4},new[]{16,0,2,1,4,1,4},new[]{16,0,3,1,5,1,4}},
                gems = new int[][]{new[]{8,1,3},new[]{5,1,1},new[]{10,4,-8},new[]{13,4,-6},new[]{17,2,-3}},
                switches = new int[][]{new[]{3,1,0,1},new[]{10,1,3,2},new[]{8,4,-8,3},new[]{17,2,0,4}},
                checkpoints = new int[][]{new[]{9,4,-6},new[]{16,2,-2}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "ギャラクシー", hint = "", par = 70.0f,
                start = new[]{1,1,1}, goal = new[]{20,5,-21},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{1,-3,-6,1,4,6},new[]{4,-3,-16,3,5,4},new[]{11,-3,-15,3,4,3},new[]{11,-3,-19,3,9,3},new[]{14,-3,-14,5,4,1},new[]{19,-3,-15,3,5,3},new[]{20,-3,-16,1,6,1},new[]{20,-3,-17,1,7,1},new[]{20,-3,-18,1,8,1},new[]{19,-3,-22,3,8,4},new[]{-2,-3,-12,1,9,10},new[]{17,-3,-23,2,10,6},new[]{0,-3,-12,1,4,1},new[]{2,-3,-12,1,4,1},new[]{4,-3,-12,1,4,1},new[]{0,-3,-11,1,4,1},new[]{2,-3,-11,1,4,1},new[]{3,-3,-11,1,4,1},new[]{4,-3,-11,1,4,1},new[]{0,-3,-10,1,4,1},new[]{1,-3,-10,1,4,1},new[]{2,-3,-10,1,4,1},new[]{4,-3,-10,1,4,1},new[]{2,-3,-9,1,5,1},new[]{4,-3,-9,1,4,1},new[]{1,-3,-8,1,4,1},new[]{2,-3,-8,1,4,1},new[]{4,-3,-8,1,4,1},new[]{1,-3,-7,1,4,1}},
                ghosts = new int[][]{new[]{7,1,-14,1,1,1,1},new[]{8,2,-14,2,1,1,1},new[]{10,1,-14,1,1,1,1}},
                gems = new int[][]{new[]{1,1,-4},new[]{0,1,-12},new[]{4,1,-8},new[]{9,3,-14},new[]{16,1,-14},new[]{21,2,-13},new[]{19,5,-22}},
                switches = new int[][]{new[]{6,2,-16,1}},
                checkpoints = new int[][]{new[]{12,1,-14},new[]{20,2,-14}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "浮遊リフト", hint = "青い動く床は、止まっている間に乗り降りできる", par = 45.0f,
                start = new[]{1,1,1}, goal = new[]{8,1,-14},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{0,-3,-7,3,4,3},new[]{7,-3,-8,3,4,5},new[]{7,-3,-15,3,4,3},new[]{-2,-3,-8,1,8,12},new[]{4,-3,-11,3,9,3}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{2,1,2},new[]{0,1,-7},new[]{9,1,-4},new[]{7,1,-15}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{8,1,-6}},
                platforms = new float[][]{new[]{1.0f,0.0f,-1.0f,0.0f,0.0f,-3.0f,2.2f,1.4f,0.0f},new[]{3.0f,0.0f,-6.0f,3.0f,0.0f,0.0f,2.2f,1.4f,0.8f},new[]{8.0f,0.0f,-9.0f,0.0f,0.0f,-3.0f,2.2f,1.4f,1.6f}},
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "エレベーター", hint = "縦に動く床に乗れば高い段へ行ける", par = 45.0f,
                start = new[]{1,1,1}, goal = new[]{12,6,-8},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{1,-3,-4,1,4,4},new[]{0,-3,-9,3,7,4},new[]{3,-3,-7,4,7,1},new[]{7,-3,-9,3,4,5},new[]{11,-3,-9,3,9,5},new[]{-2,-3,-10,1,9,14},new[]{6,-3,-13,5,11,3}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{1,1,-2},new[]{5,4,-7},new[]{7,1,-5},new[]{13,6,-5}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{8,1,-7}},
                platforms = new float[][]{new[]{1.0f,0.0f,-5.0f,0.0f,3.0f,0.0f,2.2f,1.4f,0.0f},new[]{10.0f,0.0f,-7.0f,0.0f,5.0f,0.0f,2.2f,1.4f,1.0f}},
                pushers = new float[0][],
                crumbles = new int[0][],
            },
            new StageData {
                name = "崩れる足場", hint = "ひびの入った床は乗ると崩れる。立ち止まらずに進もう", par = 40.0f,
                start = new[]{1,1,1}, goal = new[]{9,4,-15},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{0,-3,-9,3,4,4},new[]{8,-3,-9,3,4,5},new[]{8,-3,-16,3,7,4},new[]{-2,-3,-10,1,8,14},new[]{4,-3,-13,3,9,3}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{1,1,-3},new[]{5,1,-8},new[]{6,1,-7},new[]{10,1,-5},new[]{10,4,-16}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{1,1,-7},new[]{9,1,-7}},
                platforms = new float[0][],
                pushers = new float[0][],
                crumbles = new int[][]{new[]{1,0,-1},new[]{1,0,-2},new[]{1,0,-3},new[]{1,0,-4},new[]{1,0,-5},new[]{3,0,-7},new[]{4,0,-7},new[]{5,0,-7},new[]{6,0,-7},new[]{7,0,-7},new[]{3,0,-8},new[]{4,0,-8},new[]{5,0,-8},new[]{6,0,-8},new[]{7,0,-8},new[]{9,1,-10},new[]{9,2,-11},new[]{9,3,-12}},
            },
            new StageData {
                name = "押し出す壁", hint = "オレンジの壁が飛び出してくる。引っ込んだ瞬間に通り抜けよう", par = 45.0f,
                start = new[]{1,1,1}, goal = new[]{12,1,-15},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{1,-3,-12,1,4,12},new[]{0,-3,-12,1,7,12},new[]{0,-3,-16,4,4,4},new[]{4,-3,-15,7,4,1},new[]{4,-3,-16,7,7,1},new[]{11,-3,-17,3,4,5}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{1,1,-8},new[]{8,1,-15},new[]{13,1,-13}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{1,1,-14}},
                platforms = new float[0][],
                pushers = new float[][]{new[]{0.0f,1.0f,-3.0f,1.0f,0.0f,2.2f,0.0f},new[]{0.0f,1.0f,-6.0f,1.0f,0.0f,2.2f,0.7f},new[]{0.0f,1.0f,-9.0f,1.0f,0.0f,2.2f,1.4f},new[]{5.0f,1.0f,-16.0f,0.0f,1.0f,2.2f,0.0f},new[]{7.0f,1.0f,-16.0f,0.0f,1.0f,2.2f,0.7f},new[]{9.0f,1.0f,-16.0f,0.0f,1.0f,2.2f,1.4f}},
                crumbles = new int[0][],
            },
            new StageData {
                name = "リフトと崩落", hint = "", par = 50.0f,
                start = new[]{1,1,1}, goal = new[]{8,3,-15},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{7,-3,-6,3,4,4},new[]{7,-3,-16,3,6,4},new[]{-2,-3,-6,1,8,10},new[]{4,-3,-12,2,9,5}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{8,1,-1},new[]{9,1,-6},new[]{7,3,-16}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{8,1,-4}},
                platforms = new float[][]{new[]{3.0f,0.0f,1.0f,4.0f,0.0f,0.0f,2.2f,1.4f,0.0f},new[]{8.0f,0.0f,-7.0f,0.0f,0.0f,-3.0f,2.2f,1.4f,1.2f}},
                pushers = new float[0][],
                crumbles = new int[][]{new[]{8,0,1},new[]{8,0,0},new[]{8,0,-1},new[]{8,0,-2},new[]{8,1,-11},new[]{8,2,-12}},
            },
            new StageData {
                name = "ピストン工場", hint = "", par = 60.0f,
                start = new[]{1,1,1}, goal = new[]{11,4,-18},
                blocks = new int[][]{new[]{0,-3,0,4,4,3},new[]{4,-3,0,6,7,1},new[]{10,-3,-2,3,4,5},new[]{10,-3,-10,3,7,5},new[]{11,-3,-15,1,7,5},new[]{10,-3,-15,1,10,5},new[]{10,-3,-19,3,7,4},new[]{-1,-3,-2,1,8,6}},
                ghosts = new int[][]{new[]{4,0,1,6,1,1,1},new[]{11,0,-3,1,2,1,2},new[]{11,0,-4,1,3,1,2},new[]{11,0,-5,1,4,1,2}},
                gems = new int[][]{new[]{7,1,1},new[]{12,4,-8},new[]{11,4,-13},new[]{10,4,-19}},
                switches = new int[][]{new[]{3,1,0,1},new[]{12,1,-2,2}},
                checkpoints = new int[][]{new[]{11,1,0},new[]{11,4,-8}},
                platforms = new float[0][],
                pushers = new float[][]{new[]{5.0f,1.0f,0.0f,0.0f,1.0f,2.2f,0.0f},new[]{8.0f,1.0f,0.0f,0.0f,1.0f,2.2f,1.0f},new[]{10.0f,4.0f,-12.0f,1.0f,0.0f,2.2f,0.3f},new[]{10.0f,4.0f,-14.0f,1.0f,0.0f,2.2f,1.3f}},
                crumbles = new int[0][],
            },
            new StageData {
                name = "崩れる塔", hint = "", par = 55.0f,
                start = new[]{1,1,1}, goal = new[]{9,4,-17},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{4,-3,-1,3,8,5},new[]{4,-3,-10,3,8,4},new[]{8,-3,-11,3,4,5},new[]{8,-3,-18,3,7,4},new[]{-2,-3,-4,1,10,8},new[]{1,-3,-12,2,11,5}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{4,5,3},new[]{5,5,-4},new[]{10,1,-7},new[]{8,4,-18}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{5,5,-8},new[]{9,1,-9}},
                platforms = new float[][]{new[]{3.0f,0.0f,1.0f,0.0f,4.0f,0.0f,2.2f,1.4f,0.0f},new[]{7.0f,0.0f,-9.0f,0.0f,4.0f,0.0f,2.2f,1.4f,1.5f}},
                pushers = new float[0][],
                crumbles = new int[][]{new[]{5,4,-2},new[]{5,4,-3},new[]{5,4,-4},new[]{5,4,-5},new[]{5,4,-6},new[]{9,1,-12},new[]{9,2,-13},new[]{9,3,-14}},
            },
            new StageData {
                name = "連絡船", hint = "", par = 55.0f,
                start = new[]{1,1,1}, goal = new[]{14,1,-12},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{0,-3,-6,3,4,2},new[]{7,-3,-7,2,4,3},new[]{8,-3,-12,1,4,5},new[]{7,-3,-12,1,7,5},new[]{13,-3,-14,3,4,5},new[]{-2,-3,-8,1,8,12},new[]{9,-3,-18,4,10,3}},
                ghosts = new int[0][],
                gems = new int[][]{new[]{0,1,-6},new[]{8,1,-5},new[]{8,1,-10},new[]{15,1,-10}},
                switches = new int[0][],
                checkpoints = new int[][]{new[]{7,1,-6}},
                platforms = new float[][]{new[]{1.0f,0.0f,-1.0f,0.0f,0.0f,-3.0f,2.2f,1.4f,0.0f},new[]{3.0f,0.0f,-6.0f,3.0f,0.0f,0.0f,2.2f,1.4f,1.0f},new[]{9.0f,0.0f,-12.0f,3.0f,0.0f,0.0f,2.2f,1.4f,0.5f}},
                pushers = new float[][]{new[]{7.0f,1.0f,-9.0f,1.0f,0.0f,2.2f,0.0f},new[]{7.0f,1.0f,-11.0f,1.0f,0.0f,2.2f,1.1f}},
                crumbles = new int[0][],
            },
            new StageData {
                name = "総合演習", hint = "", par = 60.0f,
                start = new[]{1,1,1}, goal = new[]{11,6,-17},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{0,-3,-6,3,4,3},new[]{3,-3,-6,3,7,1},new[]{6,-3,-7,3,4,5},new[]{10,-3,-8,3,7,7},new[]{10,-3,-18,3,9,4},new[]{-2,-3,-8,1,8,12},new[]{7,-3,-14,3,11,5}},
                ghosts = new int[][]{new[]{3,0,-5,3,1,1,1}},
                gems = new int[][]{new[]{1,1,-2},new[]{5,1,-5},new[]{12,4,-2},new[]{10,6,-18}},
                switches = new int[][]{new[]{0,1,-6,1}},
                checkpoints = new int[][]{new[]{1,1,-5},new[]{7,1,-5},new[]{11,4,-4}},
                platforms = new float[][]{new[]{9.0f,0.0f,-5.0f,0.0f,3.0f,0.0f,2.2f,1.4f,0.5f},new[]{11.0f,3.0f,-9.0f,0.0f,0.0f,-3.0f,2.2f,1.4f,1.2f}},
                pushers = new float[][]{new[]{4.0f,1.0f,-6.0f,0.0f,1.0f,2.2f,0.0f}},
                crumbles = new int[][]{new[]{1,0,-1},new[]{1,0,-2},new[]{1,0,-3},new[]{11,4,-13},new[]{11,5,-14}},
            },
            new StageData {
                name = "銀河の果て", hint = "", par = 80.0f,
                start = new[]{1,1,1}, goal = new[]{20,6,-25},
                blocks = new int[][]{new[]{0,-3,0,3,4,3},new[]{2,-3,-8,3,4,4},new[]{9,-3,-9,3,4,5},new[]{9,-3,-15,3,6,4},new[]{10,-3,-21,1,6,6},new[]{9,-3,-21,1,9,6},new[]{9,-3,-26,3,9,4},new[]{14,-3,-25,1,9,1},new[]{19,-3,-27,3,9,5},new[]{-2,-3,-8,1,8,12},new[]{6,-3,-14,3,10,5},new[]{7,-3,-30,2,12,7}},
                ghosts = new int[][]{new[]{10,0,-10,1,2,1,1},new[]{10,0,-11,1,3,1,1}},
                gems = new int[][]{new[]{2,1,-2},new[]{11,1,-5},new[]{11,3,-15},new[]{10,3,-18},new[]{13,6,-25},new[]{21,6,-23}},
                switches = new int[][]{new[]{11,1,-9,1}},
                checkpoints = new int[][]{new[]{3,1,-6},new[]{10,1,-7},new[]{10,3,-14},new[]{10,6,-24}},
                platforms = new float[][]{new[]{5.0f,0.0f,-7.0f,3.0f,0.0f,0.0f,2.2f,1.4f,0.0f},new[]{10.0f,2.0f,-22.0f,0.0f,3.0f,0.0f,2.2f,1.4f,0.8f},new[]{15.0f,5.0f,-25.0f,3.0f,0.0f,0.0f,2.2f,1.4f,0.3f}},
                pushers = new float[][]{new[]{9.0f,3.0f,-17.0f,1.0f,0.0f,2.2f,0.0f},new[]{9.0f,3.0f,-19.0f,1.0f,0.0f,2.2f,1.1f}},
                crumbles = new int[][]{new[]{1,0,-1},new[]{1,0,-2},new[]{2,0,-2},new[]{3,0,-2},new[]{3,0,-3},new[]{3,0,-4},new[]{12,5,-25},new[]{13,5,-25}},
            },
        };
    }
}
