﻿using OpenTK;
using OpenTK.Input;

namespace MineCS.mccs
{
    public class Input
    {
        private static Client rdGame;
        private static List<Key> keysDown;
        private static List<Key> keysDownLast;
        private static List<MouseButton> buttonsDown;
        private static List<MouseButton> buttonsDownLast;
        private static int scrollWheel;
        private static Vector2 mouseCenter;
        private static Vector2 oldMouseDelta;
        public static Vector2 MousePos;
        public static Vector2 MouseDelta;

        public static void Initialize(Client game)
        {
            rdGame = game;
            keysDown = new List<Key>();
            keysDownLast = new List<Key>();
            buttonsDown = new List<MouseButton>();
            buttonsDownLast = new List<MouseButton>();

            var rect = game.Bounds;
            mouseCenter = new Vector2(rect.Left + rect.Width / 2, rect.Bottom - rect.Height / 2);

            game.KeyDown += Game_KeyDown;
            game.KeyUp += Game_KeyUp;
            game.MouseDown += Game_MouseDown;
            game.MouseUp += Game_MouseUp;
            game.MouseMove += (o, e) =>
            {
                if (rdGame.Focused)
                {
                    MouseDelta.X = e.XDelta + oldMouseDelta.X;
                    MouseDelta.Y = e.YDelta + oldMouseDelta.Y;
                }
            };
            game.MouseWheel += Game_MouseWheel;
        }

        private static void Game_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            scrollWheel = e.Delta;
        }

        private static void Game_MouseUp(object? sender, MouseButtonEventArgs e)
        {
            while (buttonsDown.Contains(e.Button))
                buttonsDown.Remove(e.Button);
        }

        private static void Game_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            buttonsDown.Add(e.Button);
        }

        private static void Game_KeyUp(object? sender, KeyboardKeyEventArgs e)
        {
            while (keysDown.Contains(e.Key))
                keysDown.Remove(e.Key);
        }

        private static void Game_KeyDown(object? sender, KeyboardKeyEventArgs e)
        {
            keysDown.Add(e.Key);
        }

        public static void UpdateInput()
        {
            if (keysDown == null) return;
            keysDownLast = new List<Key>(keysDown);
        }

        public static void UpdateCursor()
        {
            var rect = rdGame.Bounds;
            var mouseCenter = new Vector2(rect.Left + rect.Width / 2, rect.Bottom - rect.Height / 2);

            MouseDelta.X = 0;
            MouseDelta.Y = 0;
            oldMouseDelta.X = Mouse.GetCursorState().X - mouseCenter.X;
            oldMouseDelta.Y = Mouse.GetCursorState().Y - mouseCenter.Y;

            rdGame.CursorVisible = !rdGame.Focused;
            if (rdGame.Focused)
                Mouse.SetPosition(mouseCenter.X, mouseCenter.Y);

            MousePos.X = Mouse.GetCursorState().X - rect.Left;
            MousePos.Y = rect.Height - (Mouse.GetCursorState().Y - rect.Top);

            if (buttonsDown == null) return;
            buttonsDownLast = new List<MouseButton>(buttonsDown);
            scrollWheel = 0;
        }


        public static Key? GetKey() => keysDown.Count > 0 ? (keysDownLast.Contains(keysDown.Last()) ? null : keysDown.Last()) : null;
        public static bool KeyPress(Key key) => keysDown.Contains(key) && !keysDownLast.Contains(key);
        public static bool KeyRelease(Key key) => !keysDown.Contains(key) && keysDownLast.Contains(key);
        public static bool KeyDown(Key key) => keysDown.Contains(key);
        public static bool KeyUp(Key key) => !keysDown.Contains(key);

        public static bool MousePress(MouseButton button) => buttonsDown.Contains(button) && !buttonsDownLast.Contains(button);
        public static bool MouseRelease(MouseButton button) => !buttonsDown.Contains(button) && buttonsDownLast.Contains(button);
        public static bool MouseDown(MouseButton button) => buttonsDown.Contains(button);
        public static bool MouseUp(MouseButton button) => !buttonsDown.Contains(button);

        public static int MouseWheelDelta() => scrollWheel;
    }
}
