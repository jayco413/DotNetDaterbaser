namespace DotNetDaterbaser.TestPrepApplication
{
    /// <summary>
    /// Tracks executed scripts for the prep application.
    /// </summary>
    public class TrackingEntry
    {
        /// <summary>
        /// Gets or sets a value indicating whether the full database script has run.
        /// </summary>
        public bool FullRun { get; set; }

        /// <summary>
        /// Gets or sets the set of individual script names that have executed.
        /// </summary>
        public HashSet<string> Scripts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
