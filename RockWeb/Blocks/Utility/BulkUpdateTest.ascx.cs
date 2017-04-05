// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Web.UI;
using Rock.Data;
using Rock.Model;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "BulkUpdateTest" )]
    [Category( "Utility" )]
    [Description( "" )]
    public partial class BulkUpdateTest : Rock.Web.UI.RockBlock
    {
        #region

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                // added for your convenience

                // to show the created/modified by date time details in the PanelDrawer do something like this:
                // pdAuditDetails.SetEntity( <YOUROBJECT>, ResolveRockUrl( "~" ) );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            //
        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion

        /// <summary>
        /// Handles the Click event of the btnCleanup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCleanup_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            rockContext.Database.ExecuteSqlCommand( "DELETE FROM [PersonViewed] where TargetPersonAliasId in (SELECT ID FROM [PersonAlias] where [PersonId] in (SELECT ID FROM [Person] where [ForeignId] is not null))" );
            rockContext.Database.ExecuteSqlCommand( "DELETE FROM [PersonAlias] where [PersonId] in (SELECT ID FROM [Person] where [ForeignId] is not null)" );
            rockContext.Database.ExecuteSqlCommand( "DELETE FROM [GroupMember] where [PersonId] in (SELECT ID FROM [Person] where [ForeignId] is not null)" );
            rockContext.Database.ExecuteSqlCommand( "DELETE FROM [Person] where [ForeignId] is not null" );
            

            // Delete Location (and cascade delete GroupLocation) records
            rockContext.Database.ExecuteSqlCommand( @"
DELETE
FROM [Location]
WHERE Id IN (
		SELECT LocationId
		FROM GroupLocation
		WHERE GroupId IN (
				SELECT Id
				FROM [Group]
				WHERE [ForeignId] IS NOT NULL
				)
		)" );


            rockContext.Database.ExecuteSqlCommand( "DELETE FROM [Group] where [ForeignId] is not null" );

            rockContext.Database.ExecuteSqlCommand( "DELETE from [AttributeValue] where AttributeId in (select Id from Attribute where EntityTypeId = 15) and EntityId not in (select Id from Person)" );

            nbResults.Text = "Cleanup complete";
        }
    }
}