﻿namespace Rock.BulkUpdate.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Rock.Data.RockClientIncludeAttribute( "Model for Rock Bulk Insert APIs" )]
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
