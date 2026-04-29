using VDManager.Models;

namespace VDManager.Tests.Models;

public class AppSettingsTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var settings = new AppSettings();

        // Assert
        Assert.Equal(2000, settings.MonitoringInterval);
        Assert.True(settings.MinimizeToTray);
        Assert.True(settings.ShowBalloonTips);
        Assert.Equal(2, settings.ThemePreference); // System = 2
        Assert.False(settings.StartWithWindows);
    }

    [Fact]
    public void MonitoringInterval_CanBeModified()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.MonitoringInterval = 5000;

        // Assert
        Assert.Equal(5000, settings.MonitoringInterval);
    }

    [Fact]
    public void MinimizeToTray_CanBeToggled()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.MinimizeToTray = false;

        // Assert
        Assert.False(settings.MinimizeToTray);
    }

    [Fact]
    public void ThemePreference_CanBeSet()
    {
        // Arrange
        var settings = new AppSettings();

        // Act
        settings.ThemePreference = 0; // Light

        // Assert
        Assert.Equal(0, settings.ThemePreference);
    }
}
