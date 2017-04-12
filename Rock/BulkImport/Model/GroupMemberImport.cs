namespace Rock.BulkImport.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Rock.Data.RockClientInclude( "Model for Rock Bulk Insert APIs" )]
    [System.Diagnostics.DebuggerDisplay( "{RoleName}" )]
    public class GroupMemberImport
    {
        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>
        /// The person identifier.
        /// </value>
        public int PersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the role.
        /// </summary>
        /// <value>
        /// The role.
        /// </value>
        public string RoleName { get; set; }
    }
}
