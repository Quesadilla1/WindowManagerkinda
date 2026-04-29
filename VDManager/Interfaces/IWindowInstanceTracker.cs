using System;
using System.Collections.Generic;
using VDManager.Models;

namespace VDManager.Services
{
    public interface IWindowInstanceTracker
    {
        int AssignInstance(WindowInfo window);
        int? GetInstance(IntPtr handle);
        void RemoveWindow(IntPtr handle);
        int GetProcessWindowCount(string processName);
        Dictionary<IntPtr, int> GetAllTrackedWindows();
        void SeedFromWindows(IEnumerable<WindowInfo> windows);
        void Clear();

        bool TryClaimRule(string ruleId, IntPtr handle);
        bool IsRuleClaimedBy(string ruleId, IntPtr handle);
        bool IsRuleAvailable(string ruleId);
        IntPtr GetClaimedHandle(string ruleId);
    }
}
