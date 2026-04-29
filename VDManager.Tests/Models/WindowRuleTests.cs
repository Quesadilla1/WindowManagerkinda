using VDManager;
using VDManager.Models;

namespace VDManager.Tests.Models;

public class WindowRuleTests
{
    [Fact]
    public void Matches_ReturnsTrue_ForMatchingProcess()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            Enabled = true
        };
        var window = new WindowInfo
        {
            ProcessName = "chrome",
            Title = "Google"
        };

        // Act
        bool result = rule.Matches(window);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_IsCaseInsensitive()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            Enabled = true
        };
        var window = new WindowInfo
        {
            ProcessName = "Chrome",
            Title = "Google"
        };

        // Act
        bool result = rule.Matches(window);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_FiltersByTitlePattern_ContainsMatch()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            WindowTitlePattern = "GitHub",
            UseRegex = false,
            Enabled = true
        };

        var windowMatch = new WindowInfo
        {
            ProcessName = "chrome",
            Title = "GitHub - Pull Request"
        };

        var windowNoMatch = new WindowInfo
        {
            ProcessName = "chrome",
            Title = "Slack - Workspace"
        };

        // Act
        bool resultMatch = rule.Matches(windowMatch);
        bool resultNoMatch = rule.Matches(windowNoMatch);

        // Assert
        Assert.True(resultMatch);
        Assert.False(resultNoMatch);
    }

    [Fact]
    public void Matches_WithRegex_MatchesPattern()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "notepad",
            WindowTitlePattern = @"doc_v\d+\.\d+",
            UseRegex = true,
            Enabled = true
        };

        var windowMatch = new WindowInfo
        {
            ProcessName = "notepad",
            Title = "doc_v2.3.txt - Notepad"
        };

        var windowNoMatch = new WindowInfo
        {
            ProcessName = "notepad",
            Title = "doc_abc.txt - Notepad"
        };

        // Act
        bool resultMatch = rule.Matches(windowMatch);
        bool resultNoMatch = rule.Matches(windowNoMatch);

        // Assert
        Assert.True(resultMatch);
        Assert.False(resultNoMatch);
    }

    [Fact]
    public void Matches_WithPathologicalRegex_DoesNotHang()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "test",
            WindowTitlePattern = @"(a+)+$",  // Pathological regex
            UseRegex = true,
            Enabled = true
        };

        var window = new WindowInfo
        {
            ProcessName = "test",
            Title = new string('a', 30) + "b"  // Should cause catastrophic backtracking
        };

        // Act
        var startTime = DateTime.UtcNow;
        bool result = rule.Matches(window);
        var elapsed = DateTime.UtcNow - startTime;

        // Assert
        Assert.True(elapsed.TotalMilliseconds < 500,
            $"Regex matching took {elapsed.TotalMilliseconds}ms, should timeout at 200ms");
        // Falls back to contains matching, which should fail
        Assert.False(result);
    }

    [Fact]
    public void Matches_DisabledRule_ReturnsFalse()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            Enabled = false  // Disabled
        };
        var window = new WindowInfo
        {
            ProcessName = "chrome",
            Title = "Google"
        };

        // Act
        bool result = rule.Matches(window);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Matches_WildcardPattern_ReturnsTrue()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            WindowTitlePattern = "*",
            Enabled = true
        };
        var window = new WindowInfo
        {
            ProcessName = "chrome",
            Title = "Any Title Here"
        };

        // Act
        bool result = rule.Matches(window);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Matches_InvalidRegex_FallsBackToContains()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "test",
            WindowTitlePattern = "[invalid(regex",  // Invalid regex
            UseRegex = true,
            Enabled = true
        };
        var window = new WindowInfo
        {
            ProcessName = "test",
            Title = "Title with [invalid(regex pattern"
        };

        // Act
        bool result = rule.Matches(window);

        // Assert - Should fall back to contains matching
        Assert.True(result);
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            DesktopIndex = 2,
            Quadrant = Quadrant.TopLeft
        };

        // Act
        string result = rule.ToString();

        // Assert
        Assert.Equal("chrome → Desktop 3, TopLeft", result);
    }

    [Fact]
    public void ToString_WithNoQuadrant_ShowsNoPositioning()
    {
        // Arrange
        var rule = new WindowRule
        {
            ProcessName = "chrome",
            DesktopIndex = 0,
            Quadrant = Quadrant.None
        };

        // Act
        string result = rule.ToString();

        // Assert
        Assert.Equal("chrome → Desktop 1, No positioning", result);
    }
}
