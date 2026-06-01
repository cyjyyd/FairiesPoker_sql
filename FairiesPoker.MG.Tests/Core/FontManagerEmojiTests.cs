using FairiesPoker.MG.Core;
using Xunit;

namespace FairiesPoker.MG.Tests.Core;

public class FontManagerEmojiTests
{
    [Theory]
    [InlineData("😀")]
    [InlineData("hello 😊")]
    [InlineData("✌️")]
    public void ContainsEmoji_ShouldRecognizeEmojiText(string text)
    {
        Assert.True(FontManager.ContainsEmoji(text));
    }

    [Theory]
    [InlineData("普通文字")]
    [InlineData("room 123")]
    [InlineData("hello :)")]
    public void ContainsEmoji_ShouldIgnorePlainText(string text)
    {
        Assert.False(FontManager.ContainsEmoji(text));
    }
}
