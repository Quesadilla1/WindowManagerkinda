using System;
using System.Collections.Generic;
using VDManager.Models;

namespace VDManager.Services
{
    public interface IRulesManager
    {
        event Action<WindowInfo, WindowRule>? RuleAppliedSuccessfully;

        List<WindowRule> GetAllRules();
        void AddRule(WindowRule rule);
        bool UpdateRule(string ruleId, WindowRule updatedRule);
        bool DeleteRule(string ruleId);
        bool SetRuleEnabled(string ruleId, bool enabled);

        void LoadRules();
        bool SaveRules();

        WindowRule? FindMatchingRule(WindowInfo window);
        bool ApplyRule(WindowRule rule, WindowInfo window);
        int ApplyRulesToAllWindows();
        bool ApplyRuleToWindow(WindowInfo window);

        List<WindowRule> GetRulesForProcess(string processName);
        bool HasRules();
        int GetEnabledRulesCount();
    }
}
