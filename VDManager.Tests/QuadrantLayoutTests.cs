using System.Drawing;
using VDManager;

namespace VDManager.Tests;

public class QuadrantLayoutTests
{
    [Fact]
    public void GetQuadrantBounds_TopLeft_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.TopLeft);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_TopRight_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.TopRight);

        // Assert
        Assert.Equal(960, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_BottomLeft_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.BottomLeft);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(540, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_BottomRight_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.BottomRight);

        // Assert
        Assert.Equal(960, bounds.X);
        Assert.Equal(540, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_LeftThird_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.LeftThird);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(640, bounds.Width);  // 1920 / 3 = 640
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_CenterThird_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.CenterThird);

        // Assert
        Assert.Equal(640, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(640, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_RightThird_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.RightThird);

        // Assert
        Assert.Equal(1280, bounds.X);  // (1920 * 2) / 3 = 1280
        Assert.Equal(0, bounds.Y);
        Assert.Equal(640, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_LeftHalf_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.LeftHalf);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_RightHalf_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.RightHalf);

        // Assert
        Assert.Equal(960, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_TopHalf_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.TopHalf);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(1920, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_BottomHalf_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.BottomHalf);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(540, bounds.Y);
        Assert.Equal(1920, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_LeftTwoThirds_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.LeftTwoThirds);

        // Assert
        Assert.Equal(0, bounds.X);
        Assert.Equal(0, bounds.Y);
        Assert.Equal(1280, bounds.Width);  // (1920 * 2) / 3 = 1280
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_RightTwoThirds_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.RightTwoThirds);

        // Assert
        Assert.Equal(640, bounds.X);  // 1920 / 3 = 640
        Assert.Equal(0, bounds.Y);
        Assert.Equal(1280, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_CenterHalf_ReturnsCorrectBounds()
    {
        // Arrange
        var screenBounds = new Rectangle(0, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.CenterHalf);

        // Assert
        Assert.Equal(480, bounds.X);  // 1920 / 4 = 480
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(1080, bounds.Height);
    }

    [Fact]
    public void GetQuadrantBounds_WithOffset_CalculatesCorrectly()
    {
        // Arrange - Secondary monitor at x=1920
        var screenBounds = new Rectangle(1920, 0, 1920, 1080);
        var layout = new QuadrantLayout(screenBounds);

        // Act
        var bounds = layout.GetQuadrantBounds(Quadrant.TopLeft);

        // Assert
        Assert.Equal(1920, bounds.X);  // Starts at offset
        Assert.Equal(0, bounds.Y);
        Assert.Equal(960, bounds.Width);
        Assert.Equal(540, bounds.Height);
    }

    [Theory]
    [InlineData(0, Quadrant.None)]
    [InlineData(1, Quadrant.TopLeft)]
    [InlineData(2, Quadrant.TopRight)]
    [InlineData(3, Quadrant.BottomLeft)]
    [InlineData(4, Quadrant.BottomRight)]
    [InlineData(5, Quadrant.LeftHalf)]
    [InlineData(9, Quadrant.Maximized)]
    public void GetQuadrantFromIndex_ReturnsCorrectQuadrant(int index, Quadrant expected)
    {
        // Act
        var result = QuadrantLayout.GetQuadrantFromIndex(index);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Quadrant.None, 0)]
    [InlineData(Quadrant.TopLeft, 1)]
    [InlineData(Quadrant.TopRight, 2)]
    [InlineData(Quadrant.LeftHalf, 5)]
    [InlineData(Quadrant.Maximized, 9)]
    public void GetIndexFromQuadrant_ReturnsCorrectIndex(Quadrant quadrant, int expected)
    {
        // Act
        var result = QuadrantLayout.GetIndexFromQuadrant(quadrant);

        // Assert
        Assert.Equal(expected, result);
    }
}
