﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.BulkUpdate
{
    public class FinancialTransactionDetailImport
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int FinancialTransactionDetailForeignId { get; set; }

        /// <summary>
        /// Gets or sets the account identifier.
        /// </summary>
        /// <value>
        /// The account identifier.
        /// </value>
        public int? FinancialAccountForeignId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is non cash.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is non cash; otherwise, <c>false</c>.
        /// </value>
        public bool IsNonCash { get; set; }

        /// <summary>
        /// Gets or sets the created by person identifier.
        /// </summary>
        /// <value>
        /// The created by person identifier.
        /// </value>
        public int? CreatedByPersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the modified by person identifier.
        /// </summary>
        /// <value>
        /// The modified by person identifier.
        /// </value>
        public int? ModifiedByPersonForeignId { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }
    }
}
