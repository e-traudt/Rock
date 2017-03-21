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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity Participant" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity participant" )]

    [CodeEditorField( "Profile Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, false,
    @"
{{ Group | Attribute:'OpportunityTitle' | AddMetaTagToHead:'property','twitter:title' }}
{{ Group | Attribute:'OpportunitySummary' | AddMetaTagToHead:'property','twitter:description' }}

{{ Group | Attribute:'OpportunityTitle' | AddMetaTagToHead:'property','og:title' }}
{{ Group | Attribute:'OpportunitySummary' | AddMetaTagToHead:'property','og:description' }}

{% assign opportunityPhotoUrl = item | Attribute:'OpportunityPhoto','Url' %}
{% if opportunityPhotoUrl != '' %}
    {{ opportunityPhotoUrl | AddMetaTagToHead:'property','og:image' }}
    {{ opportunityPhotoUrl | AddMetaTagToHead:'property','twitter:image' }}
    {{ 'summary_large_image' | AddMetaTagToHead:'property','twitter:card' }}
{% endif %}

<div class='row'>
    <img src='{{ GroupMember.Person.PhotoUrl }}' class='img-circle pull-left margin-r-md' width=100 class='pull-left margin-all-md' />
    <h2>{{ GroupMember.Person.FullName | Possessive }} {{ Group | Attribute:'OpportunityType' }}</h2>
    {% assign dateRangeParts = Group | Attribute:'OpportunityDateRange','RawValue' | Split:',' %}
    {% assign dateRangePartsSize = dateRangeParts | Size %}
    {% if dateRangePartsSize == 2 %}
      {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }} to {{ dateRangeParts[1] | Date:'MMMM dd, yyyy' }}<br/>
    {% elsif dateRangePartsSize == 1  %}      
      {{ dateRangeParts[0] | Date:'MMMM dd, yyyy' }}
    {% endif %}
    {{ Group | Attribute:'OpportunityLocation' }}
</div>

<p class='margin-v-lg'>
    {{ GroupMember | Attribute:'PersonalTripIntroduction' }}
</p>

<script>function fbs_click() { u = location.href; t = document.title; window.open('http://www.facebook.com/sharer.php?u=' + encodeURIComponent(u) + '&t=' + encodeURIComponent(t), 'sharer', 'toolbar=0,status=0,width=626,height=436'); return false; }</script>

<ul class='socialsharing margin-b-lg'>
	<li>
		<a href='http://www.facebook.com/share.php?u=<url>' onclick='return fbs_click()' target='_blank' class='socialicon socialicon-facebook' title='' data-original-title='Share via Facebook'>
			<i class='fa fa-fw fa-facebook'></i>
		</a>
	</li>
	<li>
		<a href='http://twitter.com/home?status=Help%20Fund%20Me {{ 'Global' | Page:'Url' | Escape }}' class='socialicon socialicon-twitter' title='' data-original-title='Share via Twitter'>
			<i class='fa fa-fw fa-twitter'></i>
		</a>
	</li>
	<li>
		<a href='mailto:?subject={{ Group | Attribute:'OpportunityTitle' | Escape }}&body=%0D%0A{{ 'Global' | Page:'Url' }}'  class='socialicon socialicon-email' title='' data-original-title='Share via Email'>
			<i class='fa fa-fw fa-envelope-o'></i>
		</a>
	</li>
</ul>", order: 3 )]
    [NoteTypeField( "Note Type", "Note Type to use for participant comments", false, "Rock.Model.GroupMember", defaultValue: "FFFC3644-60CD-4D14-A714-E8DCC202A0E1", order: 4 )]
    [LinkedPage( "Donation Page", "The page where a person can donate to the fundraising opportunity", required: false, order: 5 )]
    [LinkedPage( "Main Page", "The main page for the fundraising opportunity", required: false, order: 6 )]
    [BooleanField( "Show Clipboard Icon", "Show a clipboard icon which will copy the page url to the users clipboard", true, order:7)]
    [TextField( "Image CSS Class", "CSS class to apply to the image.", false, "img-thumbnail", key: "ImageCssClass", order: 8 )]
    public partial class FundraisingParticipant : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( this.GetAttributeValue( "ShowClipboardIcon" ).AsBoolean() )
            {
                // Setup for being able to copy text to clipboard
                RockPage.AddScriptLink( this.Page, "~/Scripts/ZeroClipboard/ZeroClipboard.js" );
                string script = string.Format( @"
    var client = new ZeroClipboard( $('#{0}'));
    $('#{0}').tooltip();
", btnCopyToClipboard.ClientID );
                ScriptManager.RegisterStartupScript( btnCopyToClipboard, btnCopyToClipboard.GetType(), "share-copy", script, true );
                btnCopyToClipboard.Attributes["data-clipboard-target"] = hfShareUrl.ClientID;

                Uri uri = new Uri( Request.Url.ToString() );
                hfShareUrl.Value = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + CurrentPageReference.BuildUrl();
                btnCopyToClipboard.Visible = true;
            }
            else
            {
                btnCopyToClipboard.Visible = false;
            }

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
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();

                if ( groupId.HasValue && groupMemberId.HasValue )
                {
                    ShowView( groupId.Value, groupMemberId.Value );
                }
                else
                {
                    pnlView.Visible = false;
                }

                imgOpportunityPhoto.CssClass = GetAttributeValue( "ImageCssClass" );
            }
            else
            {
                var groupMember = new GroupMemberService( new RockContext() ).Get( hfGroupMemberId.Value.AsInteger() );
                if ( groupMember != null )
                {
                    groupMember.LoadAttributes();
                    var person = groupMember.Person;
                    phGroupMemberAttributes.Controls.Clear();
                    Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, false, BlockValidationGroup );

                    // Person Attributes (the ones they picked in the Block Settings)
                    phPersonAttributes.Controls.Clear();

                    var personAttributes = this.GetAttributeValue( "PersonAttributes" ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Read( a ) );
                    if ( personAttributes.Any() )
                    {
                        person.LoadAttributes();
                        foreach ( var personAttribute in personAttributes.OrderBy( a => a.Order ) )
                        {
                            personAttribute.AddControl( phPersonAttributes.Controls, person.GetAttributeValue( personAttribute.Key ), "vgProfileEdit", false, true );
                        }
                    }
                }
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
            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnMainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMainPage_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            NavigateToLinkedPage( "MainPage", queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnMakeDonation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMakeDonation_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "GroupId", hfGroupId.Value );
            queryParams.Add( "GroupMemberId", hfGroupMemberId.Value );
            NavigateToLinkedPage( "DonationPage", queryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnUpdatesTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatesTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Updates" );
        }

        /// <summary>
        /// Handles the Click event of the btnContributionsTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnContributionsTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Contributions" );
        }

        #endregion

        #region Edit Preferences

        /// <summary>
        /// Handles the Click event of the btnEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditPreferences_Click( object sender, EventArgs e )
        {
            pnlMain.Visible = false;
            pnlEditPreferences.Visible = true;

            var rockContext = new RockContext();

            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            if ( groupMember != null )
            {
                var person = groupMember.Person;
                imgProfilePhoto.BinaryFileId = person.PhotoId;
                imgProfilePhoto.NoPictureUrl = Person.GetPersonPhotoUrl( person, 200, 200 );

                groupMember.LoadAttributes( rockContext );
                groupMember.Group.LoadAttributes( rockContext );
                var opportunityType = DefinedValueCache.Read( groupMember.Group.GetAttributeValue( "OpportunityType" ).AsGuid() );

                lProfileTitle.Text = string.Format(
                    "{0} Profile for the {1} {2}",
                    RockFilters.Possessive( groupMember.Person.FullName ),
                    groupMember.Group.GetAttributeValue( "OpportunityTitle" ),
                    opportunityType );

                var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( groupMember.Group.GetAttributeValue( "OpportunityDateRange" ) );

                lDateRange.Text = dateRange.ToString( "MMMM dd, yyyy" );

                // GroupMember Attributes (all of them)
                phGroupMemberAttributes.Controls.Clear();

                Rock.Attribute.Helper.AddEditControls( groupMember, phGroupMemberAttributes, true, "vgProfileEdit", true );

                // Person Attributes (the ones they picked in the Block Settings)
                phPersonAttributes.Controls.Clear();

                var personAttributes = this.GetAttributeValue( "PersonAttributes" ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Read( a ) );
                if ( personAttributes.Any() )
                {
                    person.LoadAttributes( rockContext );
                    foreach ( var personAttribute in personAttributes.OrderBy( a => a.Order ) )
                    {
                        personAttribute.AddControl( phPersonAttributes.Controls, person.GetAttributeValue( personAttribute.Key ), "vgProfileEdit", true, true );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSaveEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveEditPreferences_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var groupMember = new GroupMemberService( rockContext ).Get( hfGroupMemberId.Value.AsInteger() );
            var personService = new PersonService( rockContext );
            var person = personService.Get( groupMember.PersonId );

            var changes = new List<string>();

            int? orphanedPhotoId = null;
            if ( person.PhotoId != imgProfilePhoto.BinaryFileId )
            {
                orphanedPhotoId = person.PhotoId;
                person.PhotoId = imgProfilePhoto.BinaryFileId;

                if ( orphanedPhotoId.HasValue )
                {
                    if ( person.PhotoId.HasValue )
                    {
                        changes.Add( "Modified the photo." );
                    }
                    else
                    {
                        changes.Add( "Deleted the photo." );
                    }
                }
                else if ( person.PhotoId.HasValue )
                {
                    changes.Add( "Added a photo." );
                }

                // add or update the Photo Verify group to have this person as Pending since the photo was changed or deleted
                using ( var photoRequestRockContext = new RockContext() )
                {
                    GroupMemberService groupMemberService = new GroupMemberService( photoRequestRockContext );
                    Group photoRequestGroup = new GroupService( photoRequestRockContext ).Get( Rock.SystemGuid.Group.GROUP_PHOTO_REQUEST.AsGuid() );

                    var photoRequestGroupMember = groupMemberService.Queryable().Where( a => a.GroupId == photoRequestGroup.Id && a.PersonId == person.Id ).FirstOrDefault();
                    if ( photoRequestGroupMember == null )
                    {
                        photoRequestGroupMember = new GroupMember();
                        photoRequestGroupMember.GroupId = photoRequestGroup.Id;
                        photoRequestGroupMember.PersonId = person.Id;
                        photoRequestGroupMember.GroupRoleId = photoRequestGroup.GroupType.DefaultGroupRoleId ?? -1;
                        groupMemberService.Add( photoRequestGroupMember );
                    }

                    photoRequestGroupMember.GroupMemberStatus = GroupMemberStatus.Pending;

                    photoRequestRockContext.SaveChanges();
                }
            }

            // Save the GroupMember Attributes
            groupMember.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phGroupMemberAttributes, groupMember );

            // Save selected Person Attributes (The ones picked in Block Settings)
            var personAttributes = this.GetAttributeValue( "PersonAttributes" ).SplitDelimitedValues().AsGuidList().Select( a => AttributeCache.Read( a ) );
            if ( personAttributes.Any() )
            {
                person.LoadAttributes( rockContext );
                foreach ( var personAttribute in personAttributes )
                {
                    Control attributeControl = phPersonAttributes.FindControl( string.Format( "attribute_field_{0}", personAttribute.Id ) );
                    if ( attributeControl != null )
                    {
                        string originalValue = person.GetAttributeValue( personAttribute.Key );
                        string newValue = personAttribute.FieldType.Field.GetEditValue( attributeControl, personAttribute.QualifierValues );

                        // Save Attribute value to the database
                        Rock.Attribute.Helper.SaveAttributeValue( person, personAttribute, newValue, rockContext );

                        // Check for changes to write to history
                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                        {
                            string formattedOriginalValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( originalValue ) )
                            {
                                formattedOriginalValue = personAttribute.FieldType.Field.FormatValue( null, originalValue, personAttribute.QualifierValues, false );
                            }

                            string formattedNewValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( newValue ) )
                            {
                                formattedNewValue = personAttribute.FieldType.Field.FormatValue( null, newValue, personAttribute.QualifierValues, false );
                            }

                            History.EvaluateChange( changes, personAttribute.Name, formattedOriginalValue, formattedNewValue, personAttribute.FieldType.Field.IsSensitive() );
                        }
                    }
                }
            }

            // Save everything else to the database in a db transaction
            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();
                groupMember.SaveAttributeValues( rockContext );

                if ( changes.Any() )
                {
                    HistoryService.SaveChanges(
                        rockContext,
                        typeof( Person ),
                        Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(),
                        person.Id,
                        changes );
                }

                if ( orphanedPhotoId.HasValue )
                {
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( orphanedPhotoId.Value );
                    if ( binaryFile != null )
                    {
                        // marked the old images as IsTemporary so they will get cleaned up later
                        binaryFile.IsTemporary = true;
                        rockContext.SaveChanges();
                    }
                }

                // if they used the ImageEditor, and cropped it, the uncropped file is still in BinaryFile. So clean it up
                if ( imgProfilePhoto.CropBinaryFileId.HasValue )
                {
                    if ( imgProfilePhoto.CropBinaryFileId != person.PhotoId )
                    {
                        BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( imgProfilePhoto.CropBinaryFileId.Value );
                        if ( binaryFile != null && binaryFile.IsTemporary )
                        {
                            string errorMessage;
                            if ( binaryFileService.CanDelete( binaryFile, out errorMessage ) )
                            {
                                binaryFileService.Delete( binaryFile );
                                rockContext.SaveChanges();
                            }
                        }
                    }
                }
            } );

            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelEditPreferences_Click( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsInteger(), hfGroupMemberId.Value.AsInteger() );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        protected void ShowView( int groupId, int groupMemberId )
        {
            pnlView.Visible = true;
            pnlMain.Visible = true;
            pnlEditPreferences.Visible = false;
            hfGroupId.Value = groupId.ToString();
            hfGroupMemberId.Value = groupMemberId.ToString();
            var rockContext = new RockContext();

            var group = new GroupService( rockContext ).Get( groupId );
            if ( group == null )
            {
                pnlView.Visible = false;
                return;
            }

            var groupMember = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == groupId && a.Id == groupMemberId ).FirstOrDefault();
            if ( groupMember == null )
            {
                pnlView.Visible = false;
                return;
            }

            group.LoadAttributes( rockContext );

            // set page title to the trip name
            RockPage.Title = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.BrowserTitle = group.GetAttributeValue( "OpportunityTitle" );
            RockPage.Header.Title = group.GetAttributeValue( "OpportunityTitle" );

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Group", group );

            groupMember.LoadAttributes( rockContext );
            mergeFields.Add( "GroupMember", groupMember );

            // Left Top Sidebar
            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            imgOpportunityPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            SetActiveTab( "Updates" );

            // Top Main
            string profileLavaTemplate = this.GetAttributeValue( "ProfileLavaTemplate" );
            if ( groupMember.PersonId == this.CurrentPersonId )
            {
                // show a warning about missing Photo or Intro if the current person is viewing their own profile
                var warningItems = new List<string>();
                if ( !groupMember.Person.PhotoId.HasValue )
                {
                    warningItems.Add( "profile photo" );
                }
                if ( groupMember.GetAttributeValue( "PersonalTripIntroduction" ).IsNullOrWhiteSpace())
                {
                    warningItems.Add( "personal trip introduction" );
                }

                nbProfileWarning.Text = "<stong>Tip!</strong> A " + warningItems.AsDelimited( ", ", " and " ) + " is recommended. Click Edit Preferences.";
                nbProfileWarning.Visible = warningItems.Any();
            }
            else
            {
                nbProfileWarning.Visible = false;
            }

            btnEditPreferences.Visible = groupMember.PersonId == this.CurrentPersonId;

            lMainTopContentHtml.Text = profileLavaTemplate.ResolveMergeFields( mergeFields );

            bool disablePublicContributionRequests = groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean();

            // only show Contribution stuff for Participants that haven't disabled contributions
            pnlFundraising.Visible = !disablePublicContributionRequests;
            pnlContributions.Visible = !disablePublicContributionRequests;
            btnContributionsTab.Visible = !disablePublicContributionRequests;

            // Progress
            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

            var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                        .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                && d.EntityId == groupMemberId )
                        .Sum( a => (decimal?)a.Amount ) ?? 0.00M;

            var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
            if ( !individualFundraisingGoal.HasValue )
            {
                individualFundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
            }

            var amountLeft = individualFundraisingGoal - contributionTotal;
            var percentMet = individualFundraisingGoal > 0 ? contributionTotal * 100 / individualFundraisingGoal : 100;
            if ( amountLeft >= 0 )
            {
                lFundraisingAmountLeftText.Text = string.Format( "{0} left", amountLeft.FormatAsCurrency() );
            }
            else
            {
                // over 100% of the goal, so display percent
                lFundraisingAmountLeftText.Text = string.Format( "{0}%", Math.Round( percentMet ?? 0 ) );
            }

            lFundraisingProgressTitle.Text = "Fundraising Progress";
            lFundraisingProgressBar.Text = string.Format(
                @"<div class='progress'>
                    <div class='progress-bar' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {1}%;'>
                    <span class='sr-only'>{0}% Complete</span>
                    </div>
                 </div>",
                Math.Round( percentMet ?? 0, 2 ), percentMet > 100 ? 100 : percentMet );


            var opportunityType = DefinedValueCache.Read( group.GetAttributeValue( "OpportunityType" ).AsGuid() );

            // set text on the return button
            btnMainPage.Text = opportunityType.Value + " Page";

            // Tab:Updates
            btnUpdatesTab.Visible = false;
            var updatesContentChannelGuid = group.GetAttributeValue( "UpdateContentChannel" ).AsGuidOrNull();
            if ( updatesContentChannelGuid.HasValue )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( updatesContentChannelGuid.Value );
                if ( contentChannel != null )
                {
                    // only show the UpdatesTab if there is another Tab option
                    btnUpdatesTab.Visible = btnContributionsTab.Visible;

                    string updatesLavaTemplate = this.GetAttributeValue( "UpdatesLavaTemplate" );
                    var contentChannelItems = new ContentChannelItemService( rockContext ).Queryable().Where( a => a.ContentChannelId == contentChannel.Id ).AsNoTracking().ToList();

                    mergeFields.Add( "ContentChannelItems", contentChannelItems );
                    lUpdatesContentItemsHtml.Text = updatesLavaTemplate.ResolveMergeFields( mergeFields );
                    btnUpdatesTab.Text = string.Format( "{0} Updates ({1})", opportunityType, contentChannelItems.Count() );
                }
            }

            if ( groupMember.PersonId == this.CurrentPersonId )
            {
                btnMakeDonation.Text = "Make Payment";
            }
            else
            {
                btnMakeDonation.Text = string.Format( "Contribute to {0} {1}", RockFilters.Possessive( groupMember.Person.NickName ), opportunityType );
            }

            // Tab: Contributions
            BindContributionsGrid();

            // Tab:Comments
            var noteType = NoteTypeCache.Read( this.GetAttributeValue( "NoteType" ).AsGuid() );
            if ( noteType != null )
            {
                notesCommentsTimeline.NoteTypes = new List<NoteTypeCache> { noteType };
            }

            notesCommentsTimeline.EntityId = groupMember.Id;

            // show the Add button on comments if the current person is a member of the Fundraising Group
            notesCommentsTimeline.AddAllowed = group.Members.Any( a => a.PersonId == this.CurrentPersonId );

            notesCommentsTimeline.RebuildNotes( true );

            var enableCommenting = group.GetAttributeValue( "EnableCommenting" ).AsBoolean();
            notesCommentsTimeline.Visible = enableCommenting;
            pnlComments.Visible = enableCommenting;

            pnlUpdatesContributions.CssClass = enableCommenting ? "col-md-8" : "col-md-12";

            // if btnContributionsTab is the only visible tab, hide the tab since there is nothing else to tab to
            if ( !btnUpdatesTab.Visible )
            {
                btnContributionsTab.Visible = false;
            }
        }

        /// <summary>
        /// Binds the contributions grid.
        /// </summary>
        protected void BindContributionsGrid()
        {
            var rockContext = new RockContext();
            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();
            int groupMemberId = hfGroupMemberId.Value.AsInteger();

            var financialTransactionQry = new FinancialTransactionService( rockContext ).Queryable();

            financialTransactionQry = financialTransactionQry.Where( a =>
                a.TransactionDetails.Any( d => d.EntityTypeId == entityTypeIdGroupMember && d.EntityId == groupMemberId ) );

            financialTransactionQry = financialTransactionQry.OrderByDescending( a => a.TransactionDateTime );

            gContributions.SetLinqDataSource( financialTransactionQry );
            gContributions.DataBind();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gContributions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gContributions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            FinancialTransaction financialTransaction = e.Row.DataItem as FinancialTransaction;
            Literal lAddress = e.Row.FindControl( "lAddress" ) as Literal;
            if ( financialTransaction != null && lAddress != null && financialTransaction.AuthorizedPersonAliasId.HasValue )
            {
                var personAddress = financialTransaction.AuthorizedPersonAlias.Person.GetHomeLocation();
                lAddress.Text = personAddress.GetFullStreetAddress();
            }
        }

        /// <summary>
        /// Sets the active tab.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        protected void SetActiveTab( string tabName )
        {
            hfActiveTab.Value = tabName;
            pnlUpdates.Visible = tabName == "Updates";
            pnlContributions.Visible = tabName == "Contributions";
            btnUpdatesTab.CssClass = tabName == "Updates" ? "btn btn-primary" : "btn btn-default";
            btnContributionsTab.CssClass = tabName == "Contributions" ? "btn btn-primary" : "btn btn-default";
        }

        #endregion
    }
}