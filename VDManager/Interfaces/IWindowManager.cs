using System.Collections.Generic;
using System.Drawing;
using VDManager.Models;

namespace VDManager.Services
{
    public interface IWindowManager
    {
        int DesktopSwitchTimeoutMs { get; set; }

        List<WindowInfo> GetAllWindows();
        List<System.IntPtr> GetAllWindowHandles();
        WindowInfo? GetWindowInfo(System.IntPtr hwnd);
        List<WindowInfo> GetWindowsForProcess(string processName);
        List<WindowInfo> GetWindowsOnDesktop(int desktopNumber);

        bool MoveWindowToDesktop(System.IntPtr hwnd, int desktopNumber);
        bool MoveWindowToDesktop(WindowInfo window, int desktopNumber);
        bool SwitchToDesktop(int desktopNumber);
        int GetCurrentDesktop();
        int GetDesktopCount();

        bool PositionWindowInQuadrant(System.IntPtr hwnd, Quadrant quadrant, int monitorIndex = 0);
        bool PositionWindowInQuadrant(WindowInfo window, Quadrant quadrant, int monitorIndex = 0);
        bool MoveAndPositionWindow(System.IntPtr hwnd, int desktopNumber, Quadrant quadrant, int monitorIndex = 0);
        bool MoveAndPositionWindow(WindowInfo window, int desktopNumber, Quadrant quadrant, int monitorIndex = 0);

        bool PinWindow(System.IntPtr hwnd);
        bool IsWindowPinned(System.IntPtr hwnd);
    }
}
