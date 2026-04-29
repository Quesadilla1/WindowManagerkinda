using VDManager.Models;
using VDManager.Services;

namespace VDManager.Tests.Services;

public class PersistenceServiceTests
{
    private readonly string _testBasePath;
    private readonly PersistenceService _service;

    public PersistenceServiceTests()
    {
        // Use temp directory for tests
        _testBasePath = Path.Combine(Path.GetTempPath(), "VDManagerTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testBasePath);
        _service = new PersistenceService(_testBasePath);
    }

    [Fact]
    public void SaveAndLoadRules_RoundTripsCorrectly()
    {
        // Arrange
        var rules = new List<WindowRule>
        {
            new WindowRule
            {
                Id = Guid.NewGuid().ToString(),
                ProcessName = "chrome",
                WindowTitlePattern = "GitHub",
                DesktopIndex = 1,
                Quadrant = Quadrant.TopLeft,
                Priority = 10,
                Enabled = true
            },
            new WindowRule
            {
                Id = Guid.NewGuid().ToString(),
                ProcessName = "notepad",
                DesktopIndex = 0,
                Quadrant = Quadrant.RightHalf,
                Priority = 5,
                Enabled = true
            },
            new WindowRule
            {
                Id = Guid.NewGuid().ToString(),
                ProcessName = "slack",
                WindowTitlePattern = @"Workspace.*",
                UseRegex = true,
                DesktopIndex = 2,
                Quadrant = Quadrant.LeftThird,
                Priority = 15,
                Enabled = false
            }
        };

        // Act
        _service.SaveRules(rules);
        var loaded = _service.LoadRules();

        // Assert
        Assert.Equal(3, loaded.Count);

        var chromeRule = loaded.FirstOrDefault(r => r.ProcessName == "chrome");
        Assert.NotNull(chromeRule);
        Assert.Equal("GitHub", chromeRule.WindowTitlePattern);
        Assert.Equal(1, chromeRule.DesktopIndex);
        Assert.Equal(Quadrant.TopLeft, chromeRule.Quadrant);
        Assert.Equal(10, chromeRule.Priority);
        Assert.True(chromeRule.Enabled);

        var slackRule = loaded.FirstOrDefault(r => r.ProcessName == "slack");
        Assert.NotNull(slackRule);
        Assert.True(slackRule.UseRegex);
        Assert.False(slackRule.Enabled);
    }

    [Fact]
    public void LoadRules_WithNoFile_ReturnsEmptyList()
    {
        // Act
        var rules = _service.LoadRules();

        // Assert
        Assert.NotNull(rules);
        Assert.Empty(rules);
    }

    [Fact]
    public void LoadRules_WithCorruptJson_ReturnsEmptyList()
    {
        // Arrange
        var rulesPath = Path.Combine(_testBasePath, "rules.json");
        File.WriteAllText(rulesPath, "{{invalid json content}}");

        // Act
        var rules = _service.LoadRules();

        // Assert
        Assert.NotNull(rules);
        Assert.Empty(rules);
    }

    [Fact]
    public void SaveAndLoadSettings_RoundTripsCorrectly()
    {
        // Arrange
        var settings = new AppSettings
        {
            MonitoringInterval = 3000,
            MinimizeToTray = false,
            ShowBalloonTips = true,
            ThemePreference = 1,
            StartWithWindows = true
        };

        // Act
        _service.SaveSettings(settings);
        var loaded = _service.LoadSettings();

        // Assert
        Assert.Equal(3000, loaded.MonitoringInterval);
        Assert.False(loaded.MinimizeToTray);
        Assert.True(loaded.ShowBalloonTips);
        Assert.Equal(1, loaded.ThemePreference);
        Assert.True(loaded.StartWithWindows);
    }

    [Fact]
    public void LoadSettings_WithNoFile_ReturnsDefaultSettings()
    {
        // Act
        var settings = _service.LoadSettings();

        // Assert
        Assert.NotNull(settings);
        Assert.Equal(2000, settings.MonitoringInterval);  // Default value
        Assert.True(settings.MinimizeToTray);
    }

    [Fact]
    public void SaveAndLoadCustomPosition_RoundTripsCorrectly()
    {
        // Arrange
        var positions = new List<CustomPosition>
        {
            new CustomPosition
            {
                Name = "My Position",
                X = 100,
                Y = 200,
                Width = 800,
                Height = 600,
                MonitorIndex = 1,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now
            }
        };

        // Act
        _service.SaveCustomPositions(positions);
        var loaded = _service.LoadCustomPositions();

        // Assert
        Assert.Single(loaded);
        Assert.Equal("My Position", loaded[0].Name);
        Assert.Equal(100, loaded[0].X);
        Assert.Equal(200, loaded[0].Y);
        Assert.Equal(800, loaded[0].Width);
        Assert.Equal(600, loaded[0].Height);
        Assert.Equal(1, loaded[0].MonitorIndex);
    }
}
