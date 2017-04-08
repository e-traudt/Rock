﻿namespace Rock.BulkUpdate
{
    /// <summary>
    /// 
    /// </summary>
    public class GroupMemberImport
    {
        /// <summary>
        /// Gets or sets the group member foreign identifier.
        /// </summary>
        /// <value>
        /// The group member foreign identifier.
        /// </value>
        public int GroupMemberForeignId { get; set; }

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
