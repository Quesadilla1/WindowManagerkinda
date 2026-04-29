using System;
using System.Collections.Generic;

namespace VDManager.Models
{
    /// <summary>
    /// Represents a saved workspace configuration
    /// </summary>
    public class WorkspaceProfile
    {
        /// <summary>
        /// Unique identifier for the profile
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Name of the profile
        /// </summary>
        public string Name { get; set; } = "Untitled Profile";

        /// <summary>
        /// Description of the profile
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Rules included in this profile
        /// </summary>
        public List<WindowRule> Rules { get; set; } = new List<WindowRule>();

        /// <summary>
        /// Number of desktops required for this profile
        /// </summary>
        public int RequiredDesktops { get; set; } = 1;

        /// <summary>
        /// Creation timestamp
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Last modified timestamp
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Whether to auto-apply this profile on startup
        /// </summary>
        public bool AutoApply { get; set; } = false;

        public override string ToString()
        {
            return $"{Name} ({Rules.Count} rules)";
        }
    }
}
