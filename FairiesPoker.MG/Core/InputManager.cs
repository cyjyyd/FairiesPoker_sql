using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#if ANDROID
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace FairiesPoker.MG.Core;

/// <summary>
/// 输入管理器 - 将MonoGame轮询转换为类似WinForms的事件
/// </summary>
public class InputManager
{
    private MouseState _prevMouse;
    private MouseState _currMouse;
    private KeyboardState _prevKeyboard;
    private KeyboardState _currKeyboard;
    private Vector2 _prevPointerPosition;
    private Vector2 _currPointerPosition;
    private bool _prevPointerDown;
    private bool _currPointerDown;
    private readonly List<char> _pendingTextInput = new();
    private readonly List<char> _textInputCharacters = new();
    private GameWindow _window;

    /// <summary>当前鼠标位置</summary>
    public Vector2 MousePosition => DisplayManager.ToVirtual(RawMousePosition);

    /// <summary>当前鼠标物理窗口位置</summary>
    public Vector2 RawMousePosition => _currPointerPosition;

    /// <summary>鼠标左键是否按下(本帧新按下)</summary>
    public bool LeftMouseClicked => _currPointerDown && !_prevPointerDown;

    /// <summary>鼠标左键是否松开(本帧新松开)</summary>
    public bool LeftMouseReleased => !_currPointerDown && _prevPointerDown;

    /// <summary>鼠标左键是否持续按住</summary>
    public bool LeftMouseHeld => _currPointerDown;

    /// <summary>鼠标滚轮增量</summary>
    public int MouseWheelDelta => OperatingSystem.IsAndroid() ? 0 : _currMouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;

    /// <summary>本帧由系统文本输入/IME提交的字符。</summary>
    public IReadOnlyList<char> TextInputCharacters => _textInputCharacters;

    public bool HasTextInputCharacters => _textInputCharacters.Count > 0;

    public bool IsSystemTextInputAvailable => _window != null && !PlatformTextInputService.UsesPopupInput;

    public bool ShouldUseKeyboardTextFallback => !PlatformTextInputService.IsShowing && !IsSystemTextInputAvailable && !HasTextInputCharacters;

    /// <summary>指定键是否按下</summary>
    public bool KeyPressed(Keys key) => _currKeyboard.IsKeyDown(key) && _prevKeyboard.IsKeyUp(key);

    /// <summary>指定键是否持续按住</summary>
    public bool KeyHeld(Keys key) => _currKeyboard.IsKeyDown(key);

    /// <summary>更新输入状态(每帧调用一次)</summary>
    public void Update()
    {
        _prevMouse = _currMouse;
        _prevPointerPosition = _currPointerPosition;
        _prevPointerDown = _currPointerDown;
        _prevKeyboard = _currKeyboard;
        _currMouse = Mouse.GetState();
        _currKeyboard = Keyboard.GetState();
        _currPointerPosition = _currMouse.Position.ToVector2();
        _currPointerDown = _currMouse.LeftButton == ButtonState.Pressed;
#if ANDROID
        UpdateTouchPointer();
#endif

        _textInputCharacters.Clear();
        if (_pendingTextInput.Count > 0)
        {
            _textInputCharacters.AddRange(_pendingTextInput);
            _pendingTextInput.Clear();
        }
    }

#if ANDROID
    private void UpdateTouchPointer()
    {
        var touches = TouchPanel.GetState();
        Vector2? releasedPosition = null;

        foreach (var touch in touches)
        {
            if (touch.State == TouchLocationState.Pressed || touch.State == TouchLocationState.Moved)
            {
                _currPointerPosition = touch.Position;
                _currPointerDown = true;
                return;
            }

            if (touch.State == TouchLocationState.Released)
                releasedPosition = touch.Position;
        }

        _currPointerPosition = releasedPosition ?? _prevPointerPosition;
        _currPointerDown = false;
    }
#endif

    /// <summary>检查点是否在矩形内</summary>
    public bool IsInRect(Point pos, Rectangle rect) => rect.Contains(pos);

    public void AttachTextInput(GameWindow window)
    {
        if (_window == window)
            return;

        DetachTextInput();
        _window = window;
#if !ANDROID
        _window.TextInput += OnTextInput;
#endif
    }

    public void DetachTextInput()
    {
        if (_window == null)
            return;

#if !ANDROID
        _window.TextInput -= OnTextInput;
#endif
        _window = null;
    }

#if !ANDROID
    private void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!char.IsControl(e.Character))
            _pendingTextInput.Add(e.Character);
    }
#endif

    public void OpenIme()
    {
        if (_window == null || _window.Handle == IntPtr.Zero || !OperatingSystem.IsWindows())
            return;

        WindowsIme.OpenPreferredChineseInput(_window.Handle);
    }

    private static class WindowsIme
    {
        [DllImport("imm32.dll")]
        private static extern IntPtr ImmGetContext(IntPtr hWnd);

        [DllImport("imm32.dll")]
        private static extern bool ImmReleaseContext(IntPtr hWnd, IntPtr hIMC);

        [DllImport("imm32.dll")]
        private static extern bool ImmSetOpenStatus(IntPtr hIMC, bool fOpen);

        [DllImport("user32.dll")]
        private static extern int GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

        [DllImport("user32.dll")]
        private static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint flags);

        public static void OpenPreferredChineseInput(IntPtr windowHandle)
        {
            TryActivateChineseKeyboardLayout();
            SetOpenStatus(windowHandle, true);
        }

        public static void SetOpenStatus(IntPtr windowHandle, bool open)
        {
            IntPtr context = ImmGetContext(windowHandle);
            if (context == IntPtr.Zero)
                return;

            try
            {
                ImmSetOpenStatus(context, open);
            }
            finally
            {
                ImmReleaseContext(windowHandle, context);
            }
        }

        private static void TryActivateChineseKeyboardLayout()
        {
            int count = GetKeyboardLayoutList(0, null);
            if (count <= 0)
                return;

            var layouts = new IntPtr[count];
            int actualCount = GetKeyboardLayoutList(layouts.Length, layouts);
            for (int i = 0; i < actualCount; i++)
            {
                if (IsChineseKeyboardLayout(layouts[i]))
                {
                    ActivateKeyboardLayout(layouts[i], 0);
                    return;
                }
            }
        }

        private static bool IsChineseKeyboardLayout(IntPtr layout)
        {
            long layoutValue = layout.ToInt64();
            int langId = (int)(layoutValue & 0xffff);
            const int primaryLanguageMask = 0x03ff;
            const int chinesePrimaryLanguageId = 0x0004;
            return (langId & primaryLanguageMask) == chinesePrimaryLanguageId;
        }
    }
}
