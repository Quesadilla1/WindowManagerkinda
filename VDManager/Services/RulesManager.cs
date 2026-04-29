using System;
using System.Collections.Generic;
using System.Linq;
using VDManager.Models;

namespace VDManager.Services
{
    /// <summary>
    /// Service for managing and applying window rules
    /// </summary>
    public class RulesManager : IRulesManager
    {
        private readonly IWindowManager windowManager;
        private readonly IPersistenceService persistenceService;
        private readonly IWindowInstanceTracker instanceTracker;
        private List<WindowRule> rules;

        /// <summary>
        /// Fired after a rule is successfully applied to a window.
        /// WindowMonitor subscribes to this to register windows for position enforcement.
        /// </summary>
        public event Action<WindowInfo, WindowRule>? RuleAppliedSuccessfully;

        public RulesManager(IWindowManager windowManager, IPersistenceService persistenceService, IWindowInstanceTracker instanceTracker)
        {
            this.windowManager = windowManager;
            this.persistenceService = persistenceService;
            this.instanceTracker = instanceTracker;
            this.rules = new List<WindowRule>();
        }

        /// <summary>
        /// Get all rules
        /// </summary>
        public List<WindowRule> GetAllRules() => rules.ToList();

        /// <summary>
        /// Add a new rule
        /// </summary>
        public void AddRule(WindowRule rule)
        {
            rules.Add(rule);
            SaveRules();
        }

        /// <summary>
        /// Update an existing rule
        /// </summary>
        public bool UpdateRule(string ruleId, WindowRule updatedRule)
        {
            var index = rules.FindIndex(r => r.Id == ruleId);
            if (index >= 0)
            {
                updatedRule.Id = ruleId; // Preserve ID
                rules[index] = updatedRule;
                SaveRules();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delete a rule
        /// </summary>
        public bool DeleteRule(string ruleId)
        {
            var rule = rules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null)
            {
                rules.Remove(rule);
                SaveRules();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Load rules from disk
        /// </summary>
        public void LoadRules()
        {
            rules = persistenceService.LoadRules();
        }

        /// <summary>
        /// Save rules to disk
        /// </summary>
        public bool SaveRules()
        {
            return persistenceService.SaveRules(rules);
        }

        /// <summary>
        /// Find matching rule for a window
        /// </summary>
        public WindowRule? FindMatchingRule(WindowInfo window)
        {
            // Get all windows once to ensure consistent ordering
            var allWindows = windowManager.GetAllWindows();
            return FindMatchingRule(window, allWindows);
        }

        /// <summary>
        /// Calculate a specificity score for a rule so that more constrained rules
        /// win over catch-all rules when their Priority is equal.
        /// </summary>
        private static int GetSpecificityScore(WindowRule rule)
        {
            int score = 0;

            // Having a title pattern is more specific than having none
            if (!string.IsNullOrEmpty(rule.WindowTitlePattern) && rule.WindowTitlePattern != "*")
                score += 2;

            // Targeting a specific instance is more specific than matching any instance
            if (rule.InstanceNumber > 0)
                score += 2;

            // Regex is considered more intentional/specific than a plain contains match
            if (rule.UseRegex)
                score += 1;

            return score;
        }

        /// <summary>
        /// Find matching rule for a window using two-phase matching.
        ///
        /// Phase 1 — Title-filter rules (exclusive reservations):
        ///   Rules with a WindowTitlePattern act as one-at-a-time seats. The first window
        ///   that matches and claims the seat holds it until the window closes. Subsequent
        ///   windows with the same title are treated as generic and fall through to Phase 2.
        ///
        /// Phase 2 — Instance / catch-all rules:
        ///   For windows not claimed in Phase 1, the "generic instance number" is their
        ///   arrival-order position among unclaimed windows of the same process. This means
        ///   title-claimed windows are invisible to the instance counter.
        /// </summary>
        private WindowRule? FindMatchingRule(WindowInfo window, List<WindowInfo> allWindows)
        {
            // Sort once: priority DESC, then specificity DESC as tiebreaker
            var sortedRules = rules
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => GetSpecificityScore(r))
                .ToList();

            // Separate into title-filter rules and generic (no title / wildcard) rules
            var titleRules = sortedRules
                .Where(r => !string.IsNullOrEmpty(r.WindowTitlePattern) && r.WindowTitlePattern != "*")
                .ToList();
            var genericRules = sortedRules
                .Where(r => string.IsNullOrEmpty(r.WindowTitlePattern) || r.WindowTitlePattern == "*")
                .ToList();

            // ── Phase 1: title-filter rules ──────────────────────────────────────
            foreach (var rule in titleRules)
            {
                if (!rule.Matches(window))
                    continue;

                // Attempt to claim this rule's seat for this window.
                // Succeeds if the seat is free, or already held by this window (re-apply).
                // Fails if another window is already sitting in this seat.
                if (instanceTracker.TryClaimRule(rule.Id, window.Handle))
                    return rule;

                // Seat is taken — keep trying lower-priority title rules
            }

            // ── Phase 2: instance / catch-all rules ──────────────────────────────
            // Determine the IDs of all title rules so we can exclude claimed windows
            // from the generic instance count.
            var titleRuleIds = titleRules.Select(r => r.Id).ToHashSet();

            // "Generic" windows = same process, NOT currently holding a title-rule claim
            var genericWindows = allWindows
                .Where(w => w.ProcessName.Equals(window.ProcessName, StringComparison.OrdinalIgnoreCase))
                .Where(w => !titleRuleIds.Any(rid => instanceTracker.IsRuleClaimedBy(rid, w.Handle)))
                .OrderBy(w => instanceTracker.GetInstance(w.Handle) ?? int.MaxValue)
                .ToList();

            // Find this window's position in the generic list (1-based)
            int genericInstance = genericWindows.FindIndex(w => w.Handle == window.Handle) + 1;

            if (genericInstance == 0)
            {
                // Window not in the generic list — it must have been claimed as a title-rule
                // window but all those rules were taken. No match.
                return null;
            }

            // Check if any generic rule is instance-specific
            bool hasInstanceRules = genericRules.Any(r => r.Matches(window) && r.InstanceNumber > 0);

            if (!hasInstanceRules)
            {
                // Simple path: return first matching generic rule
                return genericRules.FirstOrDefault(r => r.Matches(window));
            }

            // Instance path: match rule to this window's generic slot
            foreach (var rule in genericRules)
            {
                if (!rule.Matches(window))
                    continue;

                if (rule.InstanceNumber == 0 || rule.InstanceNumber == genericInstance)
                    return rule;
            }

            return null;
        }

        /// <summary>
        /// Apply a rule to a window
        /// </summary>
        public bool ApplyRule(WindowRule rule, WindowInfo window)
        {
            if (!rule.Enabled)
                return false;

            // If the rule targets a specific monitor that is currently disconnected,
            // skip placement rather than silently relocating the window to the primary monitor.
            if (rule.Quadrant != Quadrant.None && rule.MonitorIndex >= QuadrantLayout.GetMonitorCount())
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[RulesManager] Skipping rule '{rule.ProcessName}' — MonitorIndex={rule.MonitorIndex} " +
                    $"is unavailable ({QuadrantLayout.GetMonitorCount()} monitor(s) connected).");
                return false;
            }

            try
            {
                bool result;
                if (rule.Quadrant == Quadrant.None)
                {
                    // Just move to desktop
                    result = windowManager.MoveWindowToDesktop(window, rule.DesktopIndex);
                }
                else
                {
                    // Move and position
                    result = windowManager.MoveAndPositionWindow(
                        window,
                        rule.DesktopIndex,
                        rule.Quadrant,
                        rule.MonitorIndex
                    );
                }

                if (result)
                {
                    RuleAppliedSuccessfully?.Invoke(window, rule);
                }

                return result;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Apply rules to all matching windows
        /// </summary>
        public int ApplyRulesToAllWindows()
        {
            int appliedCount = 0;
            var windows = windowManager.GetAllWindows();

            // Ensure all currently open windows have stable instance numbers before matching.
            instanceTracker.SeedFromWindows(windows);

            // Pre-pass: Establish title-rule claims for all windows before computing
            // generic instance numbers. Without this pre-pass, the order windows are
            // iterated below could affect which window claims a title rule, causing
            // incorrect generic instance numbers for the remaining windows.
            var titleRulesForPrepass = rules
                .Where(r => !string.IsNullOrEmpty(r.WindowTitlePattern) && r.WindowTitlePattern != "*")
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => GetSpecificityScore(r))
                .ToList();

            foreach (var window in windows)
            {
                foreach (var rule in titleRulesForPrepass)
                {
                    if (!rule.Matches(window))
                        continue;
                    // TryClaimRule is idempotent — calling it here and again in FindMatchingRule is safe
                    if (instanceTracker.TryClaimRule(rule.Id, window.Handle))
                        break; // window claimed a title rule, stop checking for this window
                }
            }

            // Main pass: Apply rules to all windows (title claims are now fully established)
            foreach (var window in windows)
            {
                var rule = FindMatchingRule(window, windows);
                if (rule != null)
                {
                    if (ApplyRule(rule, window))
                        appliedCount++;
                }
            }

            return appliedCount;
        }

        /// <summary>
        /// Apply rules to a specific window
        /// </summary>
        public bool ApplyRuleToWindow(WindowInfo window)
        {
            var rule = FindMatchingRule(window);
            if (rule != null)
            {
                return ApplyRule(rule, window);
            }
            return false;
        }

        /// <summary>
        /// Enable or disable a rule
        /// </summary>
        public bool SetRuleEnabled(string ruleId, bool enabled)
        {
            var rule = rules.FirstOrDefault(r => r.Id == ruleId);
            if (rule != null)
            {
                rule.Enabled = enabled;
                SaveRules();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get rules for a specific process
        /// </summary>
        public List<WindowRule> GetRulesForProcess(string processName)
        {
            return rules.Where(r => r.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                       .ToList();
        }

        /// <summary>
        /// Check if any rules exist
        /// </summary>
        public bool HasRules() => rules.Any();

        /// <summary>
        /// Get count of enabled rules
        /// </summary>
        public int GetEnabledRulesCount() => rules.Count(r => r.Enabled);
    }
}
