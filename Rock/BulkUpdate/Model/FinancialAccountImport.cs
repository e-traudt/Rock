using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.BulkUpdate.Model
{
    [Rock.Data.RockClientIncludeAttribute( "Model for Rock Bulk Insert APIs" )]
    public class FinancialAccountImport
    {
        /// <summary>
        /// Gets or sets the financial account foreign identifier.
        /// </summary>
        /// <value>
        /// The financial account foreign identifier.
        /// </value>
        public int FinancialAccountForeignId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tax deductible.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is tax deductible; otherwise, <c>false</c>.
        /// </value>
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the parent financial account foreign identifier.
        /// </summary>
        /// <value>
        /// The parent financial account foreign identifier.
        /// </value>
        public int? ParentFinancialAccountForeignId { get; set; }
    }
}
