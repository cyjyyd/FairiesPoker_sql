using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

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

    /// <summary>当前鼠标位置</summary>
    public Vector2 MousePosition => _currMouse.Position.ToVector2();

    /// <summary>鼠标左键是否按下(本帧新按下)</summary>
    public bool LeftMouseClicked => _currMouse.LeftButton == ButtonState.Pressed && _prevMouse.LeftButton == ButtonState.Released;

    /// <summary>鼠标左键是否松开(本帧新松开)</summary>
    public bool LeftMouseReleased => _currMouse.LeftButton == ButtonState.Released && _prevMouse.LeftButton == ButtonState.Pressed;

    /// <summary>鼠标左键是否持续按住</summary>
    public bool LeftMouseHeld => _currMouse.LeftButton == ButtonState.Pressed;

    /// <summary>鼠标滚轮增量</summary>
    public int MouseWheelDelta => _currMouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;

    /// <summary>指定键是否按下</summary>
    public bool KeyPressed(Keys key) => _currKeyboard.IsKeyDown(key) && _prevKeyboard.IsKeyUp(key);

    /// <summary>指定键是否持续按住</summary>
    public bool KeyHeld(Keys key) => _currKeyboard.IsKeyDown(key);

    /// <summary>更新输入状态(每帧调用一次)</summary>
    public void Update()
    {
        _prevMouse = _currMouse;
        _prevKeyboard = _currKeyboard;
        _currMouse = Mouse.GetState();
        _currKeyboard = Keyboard.GetState();
    }

    /// <summary>检查点是否在矩形内</summary>
    public bool IsInRect(Point pos, Rectangle rect) => rect.Contains(pos);
}
