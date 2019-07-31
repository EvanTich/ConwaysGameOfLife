using Microsoft.Xna.Framework.Input;

namespace ConwaysGameOfLife {

    public enum MouseButton {
        LeftClick, RightClick, MiddleButton
    }

    public class InputHelper {

        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static KeyboardState LastKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }
        public static MouseState LastMouseState { get; private set; }

        private static bool isInitialized;

        static InputHelper() {
            Initialize();
        }

        public static void Initialize() {
            if(!isInitialized) {
                isInitialized = true;

                Update(); // set current
                Update(); // set last
            }
        }

        public static void Update() {
            LastKeyboardState = CurrentKeyboardState;
            LastMouseState = CurrentMouseState;

            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
        }

        public static bool IsNewKeyPress(Keys key) {
            return CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key);
        }

        public static bool IsKeyDown(Keys key) {
            return CurrentKeyboardState.IsKeyDown(key);
        }

        public static bool IsNewKeyRelease(Keys key) {
            return LastKeyboardState.IsKeyDown(key) && CurrentKeyboardState.IsKeyUp(key);
        }

        public static bool IsNewMouseClick(MouseButton button) {
            return (button == MouseButton.LeftClick &&
                CurrentMouseState.LeftButton == ButtonState.Pressed && LastMouseState.LeftButton == ButtonState.Released) ||
                (button == MouseButton.RightClick &&
                CurrentMouseState.RightButton == ButtonState.Pressed && LastMouseState.RightButton == ButtonState.Released);
        }

        public static bool IsNewMouseRelease(MouseButton button) {
            return (button == MouseButton.LeftClick &&
                LastMouseState.LeftButton == ButtonState.Pressed && CurrentMouseState.LeftButton == ButtonState.Released) ||
                (button == MouseButton.RightClick &&
                LastMouseState.RightButton == ButtonState.Pressed && CurrentMouseState.RightButton == ButtonState.Released);
        }

        public static bool IsMouseDown(MouseButton button) {
            return (button == MouseButton.LeftClick && CurrentMouseState.LeftButton == ButtonState.Pressed) ||
                (button == MouseButton.RightClick && CurrentMouseState.RightButton == ButtonState.Pressed);
        }
    }
}
