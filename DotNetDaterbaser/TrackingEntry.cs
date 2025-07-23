using System;
using System.Collections.Generic;

namespace ExampleCorp.DotNetDaterbaser
{
    /// <summary>
    /// Tracks which database scripts have been executed for a given server and database.
    /// </summary>
    public class TrackingEntry
    {
        /// <summary>
        /// Gets or sets a value indicating whether the full database script has run.
        /// </summary>
        public bool FullRun { get; set; }

        /// <summary>
        /// Gets the set of individual script names that have executed.
        /// </summary>
        public HashSet<string> Scripts { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
