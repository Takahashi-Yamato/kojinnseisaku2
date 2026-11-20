using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace CubeOrbit
{
    public enum GameKey { Up, Right, Down, Left, Restart, Pause, Confirm, Mute, ResetAll }

    /// <summary>
    /// 旧Input Manager と 新Input System のどちらの設定でも動くようにするための入力まとめ。
    /// （Unity 6 の新規プロジェクトは新Input Systemが標準）
    /// </summary>
    public static class InputCompat
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        static bool Pressed(GameKey k, bool down)
        {
            var kb = Keyboard.current;
            if (kb == null) return false;
            switch (k)
            {
                case GameKey.Up: return Check(kb.upArrowKey, down) || Check(kb.wKey, down);
                case GameKey.Right: return Check(kb.rightArrowKey, down) || Check(kb.dKey, down);
                case GameKey.Down: return Check(kb.downArrowKey, down) || Check(kb.sKey, down);
                case GameKey.Left: return Check(kb.leftArrowKey, down) || Check(kb.aKey, down);
                case GameKey.Restart: return Check(kb.rKey, down);
                case GameKey.Pause: return Check(kb.escapeKey, down) || Check(kb.pKey, down);
                case GameKey.Confirm: return Check(kb.enterKey, down) || Check(kb.spaceKey, down);
                case GameKey.Mute: return Check(kb.mKey, down);
                case GameKey.ResetAll: return Check(kb.deleteKey, down) || Check(kb.backspaceKey, down);
            }
            return false;
        }

        static bool Check(UnityEngine.InputSystem.Controls.KeyControl key, bool down)
        {
            return down ? key.wasPressedThisFrame : key.isPressed;
        }

        public static bool PointerDown(out Vector2 pos)
        {
            var p = Pointer.current;
            pos = p != null ? p.position.ReadValue() : Vector2.zero;
            return p != null && p.press.wasPressedThisFrame;
        }

        public static bool PointerUp(out Vector2 pos)
        {
            var p = Pointer.current;
            pos = p != null ? p.position.ReadValue() : Vector2.zero;
            return p != null && p.press.wasReleasedThisFrame;
        }
#else
        static bool Pressed(GameKey k, bool down)
        {
            switch (k)
            {
                case GameKey.Up: return Key(KeyCode.UpArrow, down) || Key(KeyCode.W, down);
                case GameKey.Right: return Key(KeyCode.RightArrow, down) || Key(KeyCode.D, down);
                case GameKey.Down: return Key(KeyCode.DownArrow, down) || Key(KeyCode.S, down);
                case GameKey.Left: return Key(KeyCode.LeftArrow, down) || Key(KeyCode.A, down);
                case GameKey.Restart: return Key(KeyCode.R, down);
                case GameKey.Pause: return Key(KeyCode.Escape, down) || Key(KeyCode.P, down);
                case GameKey.Confirm: return Key(KeyCode.Return, down) || Key(KeyCode.Space, down);
                case GameKey.Mute: return Key(KeyCode.M, down);
                case GameKey.ResetAll: return Key(KeyCode.Delete, down) || Key(KeyCode.Backspace, down);
            }
            return false;
        }

        static bool Key(KeyCode k, bool down) { return down ? Input.GetKeyDown(k) : Input.GetKey(k); }

        public static bool PointerDown(out Vector2 pos)
        {
            pos = Input.mousePosition;
            return Input.GetMouseButtonDown(0);
        }

        public static bool PointerUp(out Vector2 pos)
        {
            pos = Input.mousePosition;
            return Input.GetMouseButtonUp(0);
        }
#endif
        /// <summary>このフレームで押されたか</summary>
        public static bool Down(GameKey k) { return Pressed(k, true); }
        /// <summary>押しっぱなしか</summary>
        public static bool Held(GameKey k) { return Pressed(k, false); }
    }
}
