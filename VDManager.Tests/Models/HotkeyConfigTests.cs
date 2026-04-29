using VDManager.Models;
using VDManager.Services;

namespace VDManager.Tests.Models;

public class HotkeyConfigTests
{
    [Fact]
    public void GetDisplayString_WithModifiers_FormatsCorrectly()
    {
        // Arrange
        var hotkey = new HotkeyConfig
        {
            Modifiers = HotkeyManager.MOD_WIN | HotkeyManager.MOD_CONTROL,
            Key = System.Windows.Forms.Keys.D1,
            Name = "Switch to Desktop 1"
        };

        // Act
        var display = hotkey.GetDisplayString();

        // Assert
        Assert.Contains("Win", display);
        Assert.Contains("Ctrl", display);
        Assert.Contains("D1", display);
    }

    [Fact]
    public void GetDisplayString_WithoutModifiers_ShowsKeyOnly()
    {
        // Arrange
        var hotkey = new HotkeyConfig
        {
            Modifiers = 0,
            Key = System.Windows.Forms.Keys.F1,
            Name = "Help"
        };

        // Act
        var display = hotkey.GetDisplayString();

        // Assert
        Assert.Equal("F1", display);
    }

    [Fact]
    public void ToString_IncludesNameAndKeys()
    {
        // Arrange
        var hotkey = new HotkeyConfig
        {
            Name = "Switch Desktop",
            Modifiers = HotkeyManager.MOD_WIN,
            Key = System.Windows.Forms.Keys.D1
        };

        // Act
        var result = hotkey.ToString();

        // Assert
        Assert.Contains("Switch Desktop", result);
        Assert.Contains(":", result);
    }

    [Fact]
    public void Constructor_GeneratesUniqueId()
    {
        // Act
        var hotkey1 = new HotkeyConfig();
        var hotkey2 = new HotkeyConfig();

        // Assert
        Assert.NotEqual(hotkey1.Id, hotkey2.Id);
        Assert.NotEmpty(hotkey1.Id);
        Assert.NotEmpty(hotkey2.Id);
    }

    [Fact]
    public void Enabled_DefaultsToTrue()
    {
        // Act
        var hotkey = new HotkeyConfig();

        // Assert
        Assert.True(hotkey.Enabled);
    }
}
