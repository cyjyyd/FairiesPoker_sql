using FairiesPoker.MG.UI;
using Xunit;
using XnaKeys = Microsoft.Xna.Framework.Input.Keys;

namespace FairiesPoker.MG.Tests.UI;

public class UITextBoxTests
{
    [Theory]
    [InlineData(XnaKeys.NumPad0, '0')]
    [InlineData(XnaKeys.NumPad1, '1')]
    [InlineData(XnaKeys.NumPad2, '2')]
    [InlineData(XnaKeys.NumPad3, '3')]
    [InlineData(XnaKeys.NumPad4, '4')]
    [InlineData(XnaKeys.NumPad5, '5')]
    [InlineData(XnaKeys.NumPad6, '6')]
    [InlineData(XnaKeys.NumPad7, '7')]
    [InlineData(XnaKeys.NumPad8, '8')]
    [InlineData(XnaKeys.NumPad9, '9')]
    public void KeyToChar_ShouldMapNumpadDigits(XnaKeys key, char expected)
    {
        Assert.Equal(expected, UITextBox.KeyToChar(key, shift: false));
    }

    [Theory]
    [InlineData(XnaKeys.Add, '+')]
    [InlineData(XnaKeys.Subtract, '-')]
    [InlineData(XnaKeys.Multiply, '*')]
    [InlineData(XnaKeys.Divide, '/')]
    [InlineData(XnaKeys.Decimal, '.')]
    public void KeyToChar_ShouldMapNumpadOperators(XnaKeys key, char expected)
    {
        Assert.Equal(expected, UITextBox.KeyToChar(key, shift: false));
    }

    [Theory]
    [InlineData("你好", "你")]
    [InlineData("abc", "ab")]
    [InlineData("😀", "")]
    public void RemoveLastTextElement_ShouldRemoveCompleteCharacter(string text, string expected)
    {
        Assert.Equal(expected, UITextBox.RemoveLastTextElement(text));
    }
}
