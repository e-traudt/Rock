using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.BulkUpdate.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Rock.Data.RockClientIncludeAttribute( "Model for Rock Bulk Insert APIs" )]
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
