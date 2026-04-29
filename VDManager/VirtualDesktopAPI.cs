using System;
using System.Runtime.InteropServices;

namespace VDManager
{
    /// <summary>
    /// P/Invoke wrapper for VirtualDesktopAccessor.dll
    /// Provides access to Windows Virtual Desktop functionality
    /// </summary>
    public static class VirtualDesktopAPI
    {
        private const string DllName = "VirtualDesktopAccessor.dll";

        /// <summary>
        /// Get the total number of virtual desktops
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDesktopCount();

        /// <summary>
        /// Get the index of the current desktop (0-based)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetCurrentDesktopNumber();

        /// <summary>
        /// Switch to the specified desktop by index (0-based)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void GoToDesktopNumber(int desktopNumber);

        /// <summary>
        /// Move a window to the specified desktop
        /// </summary>
        /// <param name="hwnd">Handle to the window</param>
        /// <param name="desktopNumber">Target desktop index (0-based)</param>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int MoveWindowToDesktopNumber(IntPtr hwnd, int desktopNumber);

        /// <summary>
        /// Get the desktop number for a specific window
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetWindowDesktopNumber(IntPtr hwnd);

        /// <summary>
        /// Check if a window is on the current desktop
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsWindowOnCurrentVirtualDesktop(IntPtr hwnd);

        /// <summary>
        /// Check if a window is pinned (appears on all desktops)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsPinnedWindow(IntPtr hwnd);

        /// <summary>
        /// Pin a window to all desktops
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int PinWindow(IntPtr hwnd);

        /// <summary>
        /// Unpin a window from all desktops
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int UnPinWindow(IntPtr hwnd);

        /// <summary>
        /// Create a new virtual desktop (Win11 only)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateDesktop();

        /// <summary>
        /// Remove a virtual desktop (Win11 only)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int RemoveDesktop(int removeDesktopNumber, int fallbackDesktopNumber);

        /// <summary>
        /// Check if a window is on a specific desktop number
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int IsWindowOnDesktopNumber(IntPtr hwnd, int desktopNumber);

        /// <summary>
        /// Register a window to receive virtual desktop change notifications via PostMessage.
        /// The DLL posts (messageOffset + N) to listenerHwnd for each event type:
        ///   +0 = CurrentDesktopChanged  (wParam=oldIdx,       lParam=newIdx)
        ///   +2 = VirtualDesktopDestroyed     (wParam=destroyedIdx, lParam=fallbackIdx)
        ///   +4 = VirtualDesktopDestroyBegin  (wParam=destroyedIdx, lParam=fallbackIdx)
        ///   +5 = VirtualDesktopCreated       (wParam=newIdx,        lParam=0)
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int RegisterPostMessageHook(IntPtr listenerHwnd, int messageOffset);

        /// <summary>
        /// Unregister a previously registered post-message hook.
        /// Call this on form close to avoid stale notifications.
        /// </summary>
        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int UnregisterPostMessageHook(IntPtr listenerHwnd);

        // Thread-safe lazy initialization - the Lazy<T> handles all synchronization
        private static readonly Lazy<bool> _isAvailableLazy = new Lazy<bool>(InitIsAvailable);

        private static bool InitIsAvailable()
        {
            try
            {
                GetDesktopCount();
                return true;
            }
            catch (DllNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualDesktopAPI] DLL Not Found: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VirtualDesktopAPI] DLL Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if the VirtualDesktopAccessor DLL is available and loaded.
        /// Result is cached after the first call (thread-safe).
        /// </summary>
        public static bool IsAvailable()
        {
            return _isAvailableLazy.Value;
        }

        /// <summary>
        /// Reset the cached availability flag (useful after updating the DLL at runtime).
        /// Creates a new Lazy instance to force re-evaluation.
        /// </summary>
        public static void ResetAvailabilityCache()
        {
            // Create a new Lazy<bool> to force re-evaluation on next IsAvailable() call
            // Note: This uses reflection to replace the static readonly field
            var field = typeof(VirtualDesktopAPI).GetField("_isAvailableLazy",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            field?.SetValue(null, new Lazy<bool>(InitIsAvailable));
        }

        /// <summary>
        /// Get detailed error information about DLL loading
        /// </summary>
        public static string GetLoadError()
        {
            try
            {
                GetDesktopCount();
                return "DLL loaded successfully";
            }
            catch (DllNotFoundException ex)
            {
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string exeDir = System.IO.Path.GetDirectoryName(exePath) ?? "";
                string dllPath = System.IO.Path.Combine(exeDir, DllName);
                bool fileExists = System.IO.File.Exists(dllPath);

                return $"DLL Not Found\n" +
                       $"Expected path: {dllPath}\n" +
                       $"File exists: {fileExists}\n" +
                       $"Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"DLL Load Error: {ex.GetType().Name}\n{ex.Message}";
            }
        }
    }
}
