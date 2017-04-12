namespace Rock.BulkImport.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Rock.Data.RockClientInclude( "Model for Rock Bulk Insert APIs" )]
    [System.Diagnostics.DebuggerDisplay( "{Name}" )]
    public class ScheduleImport
    {
        /// <summary>
        /// Gets or sets the schedule foreign identifier.
        /// </summary>
        /// <value>
        /// The schedule foreign identifier.
        /// </value>
        public int ScheduleForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
    }
}
