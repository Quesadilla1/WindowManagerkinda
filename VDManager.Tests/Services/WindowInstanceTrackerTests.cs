using System;
using Xunit;
using VDManager;
using VDManager.Services;

namespace VDManager.Tests.Services
{
    public class WindowInstanceTrackerTests
    {
        [Fact]
        public void AssignInstance_FirstWindow_GetsInstanceOne()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window = new WindowInfo
            {
                Handle = new IntPtr(100),
                ProcessName = "notepad"
            };

            // Act
            int instance = tracker.AssignInstance(window);

            // Assert
            Assert.Equal(1, instance);
        }

        [Fact]
        public void AssignInstance_SecondWindow_GetsInstanceTwo()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "notepad" };

            // Act
            int instance1 = tracker.AssignInstance(window1);
            int instance2 = tracker.AssignInstance(window2);

            // Assert
            Assert.Equal(1, instance1);
            Assert.Equal(2, instance2);
        }

        [Fact]
        public void AssignInstance_SameHandleTwice_ReturnsSameInstance()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };

            // Act
            int instance1 = tracker.AssignInstance(window);
            int instance2 = tracker.AssignInstance(window);

            // Assert
            Assert.Equal(1, instance1);
            Assert.Equal(1, instance2);
        }

        [Fact]
        public void AssignInstance_DifferentProcesses_GetsSeparateInstances()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var notepad1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var notepad2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "notepad" };
            var chrome1 = new WindowInfo { Handle = new IntPtr(300), ProcessName = "chrome" };

            // Act
            int notepadInstance1 = tracker.AssignInstance(notepad1);
            int notepadInstance2 = tracker.AssignInstance(notepad2);
            int chromeInstance = tracker.AssignInstance(chrome1);

            // Assert
            Assert.Equal(1, notepadInstance1);
            Assert.Equal(2, notepadInstance2);
            Assert.Equal(1, chromeInstance); // Chrome starts at 1, separate from notepad
        }

        [Fact]
        public void GetInstance_ExistingHandle_ReturnsCorrectInstance()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            tracker.AssignInstance(window);

            // Act
            int? instance = tracker.GetInstance(new IntPtr(100));

            // Assert
            Assert.Equal(1, instance);
        }

        [Fact]
        public void GetInstance_NonExistingHandle_ReturnsNull()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();

            // Act
            int? instance = tracker.GetInstance(new IntPtr(999));

            // Assert
            Assert.Null(instance);
        }

        [Fact]
        public void RemoveWindow_ExistingHandle_RemovesFromTracking()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            tracker.AssignInstance(window);

            // Act
            tracker.RemoveWindow(new IntPtr(100));
            int? instance = tracker.GetInstance(new IntPtr(100));

            // Assert
            Assert.Null(instance);
        }

        [Fact]
        public void RemoveWindow_FillsGaps_NextWindowGetsLowestInstance()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "notepad" };
            var window3 = new WindowInfo { Handle = new IntPtr(300), ProcessName = "notepad" };

            tracker.AssignInstance(window1); // Instance 1
            tracker.AssignInstance(window2); // Instance 2

            // Remove instance 1
            tracker.RemoveWindow(new IntPtr(100));

            // Act - new window should get instance 1 (fill the gap)
            int newInstance = tracker.AssignInstance(window3);

            // Assert
            Assert.Equal(1, newInstance);
            Assert.Equal(2, tracker.GetInstance(new IntPtr(200))); // Window 2 still has instance 2
        }

        [Fact]
        public void GetProcessWindowCount_NoWindows_ReturnsZero()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();

            // Act
            int count = tracker.GetProcessWindowCount("notepad");

            // Assert
            Assert.Equal(0, count);
        }

        [Fact]
        public void GetProcessWindowCount_MultipleWindows_ReturnsCorrectCount()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "notepad" };
            var chrome = new WindowInfo { Handle = new IntPtr(300), ProcessName = "chrome" };

            tracker.AssignInstance(window1);
            tracker.AssignInstance(window2);
            tracker.AssignInstance(chrome);

            // Act
            int notepadCount = tracker.GetProcessWindowCount("notepad");
            int chromeCount = tracker.GetProcessWindowCount("chrome");

            // Assert
            Assert.Equal(2, notepadCount);
            Assert.Equal(1, chromeCount);
        }

        [Fact]
        public void Clear_RemovesAllTracking()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "chrome" };

            tracker.AssignInstance(window1);
            tracker.AssignInstance(window2);

            // Act
            tracker.Clear();

            // Assert
            Assert.Null(tracker.GetInstance(new IntPtr(100)));
            Assert.Null(tracker.GetInstance(new IntPtr(200)));
            Assert.Equal(0, tracker.GetProcessWindowCount("notepad"));
            Assert.Equal(0, tracker.GetProcessWindowCount("chrome"));
        }

        [Fact]
        public void GetAllTrackedWindows_ReturnsCorrectMappings()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "notepad" };

            tracker.AssignInstance(window1);
            tracker.AssignInstance(window2);

            // Act
            var allWindows = tracker.GetAllTrackedWindows();

            // Assert
            Assert.Equal(2, allWindows.Count);
            Assert.Equal(1, allWindows[new IntPtr(100)]);
            Assert.Equal(2, allWindows[new IntPtr(200)]);
        }

        [Fact]
        public void AssignInstance_ProcessNameCaseInsensitive_SharesInstances()
        {
            // Arrange
            var tracker = new WindowInstanceTracker();
            var window1 = new WindowInfo { Handle = new IntPtr(100), ProcessName = "Notepad" };
            var window2 = new WindowInfo { Handle = new IntPtr(200), ProcessName = "NOTEPAD" };
            var window3 = new WindowInfo { Handle = new IntPtr(300), ProcessName = "notepad" };

            // Act
            int instance1 = tracker.AssignInstance(window1);
            int instance2 = tracker.AssignInstance(window2);
            int instance3 = tracker.AssignInstance(window3);

            // Assert
            Assert.Equal(1, instance1);
            Assert.Equal(2, instance2);
            Assert.Equal(3, instance3);
        }
    }
}
