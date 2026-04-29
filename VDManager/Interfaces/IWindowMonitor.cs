using System;

namespace VDManager.Services
{
    public interface IWindowMonitor : IDisposable
    {
        event EventHandler<RuleAppliedEventArgs>? RuleApplied;
        event EventHandler<string>? EnforcementSkipped;

        bool EnforcementEnabled { get; set; }
        int GracePeriodMs { get; set; }
        int CooldownMs { get; set; }
        int NewWindowRuleDelayMs { get; set; }
        bool SkipEnforcementWhenMinimized { get; set; }
        bool IsMonitoring { get; }

        void StartMonitoring(int intervalMs = 2000);
        void StopMonitoring();
        void ClearAllEnforcedWindows();
        void SuspendEnforcementForDisplayChange();
        void ResumeEnforcementAfterDisplayChange(bool monitorCountChanged);
        void OnVirtualDesktopCountChanged();
    }
}
