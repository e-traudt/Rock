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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using CsvHelper;
using CsvHelper.Configuration;
using Rock.BulkUpdate;
using System.Diagnostics;
using System.Text;
using System.IO.Compression;

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

        protected void btnGo_Click( object sender, EventArgs e )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();
            RockContext rockContext = new RockContext();
            var qryAllPersons = new PersonService( rockContext ).Queryable( true, true );
            var groupService = new GroupService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var locationService = new LocationService( rockContext );

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int familyGroupTypeId = familyGroupType.Id;

            // int familyGroupTypeId, personRecordTypeValueId;
            Dictionary<int, Group> familiesLookup;
            Dictionary<int, Person> personLookup;

            StringBuilder sbStats = new StringBuilder();

            // dictionary of Families. KEY is FamilyForeignId
            familiesLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );
            personLookup = qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{2}ms] Get {0} family and {1} person lookups\n", familiesLookup.Count, personLookup.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            List<PersonImport> personImports = GetPersonImportsFromSlingshotFile();

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Get {0} PersonImport records from CSV File\n", personImports.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            foreach ( var personImport in personImports )
            {
                Group family = null;

                if ( familiesLookup.ContainsKey( personImport.FamilyForeignId ) )
                {
                    family = familiesLookup[personImport.FamilyForeignId];
                }

                if ( family == null )
                {
                    family = new Group();
                    family.GroupTypeId = familyGroupTypeId;
                    family.Name = personImport.LastName;
                    family.CampusId = personImport.CampusId;

                    family.ForeignId = personImport.FamilyForeignId;
                    familiesLookup.Add( personImport.FamilyForeignId, family );
                }

                Person person = null;
                if ( personLookup.ContainsKey( personImport.PersonForeignId ) )
                {
                    person = personLookup[personImport.PersonForeignId];
                }

                if ( person == null )
                {
                    person = new Person();
                    person.RecordTypeValueId = personImport.RecordTypeValueId;
                    person.RecordStatusValueId = personImport.RecordStatusValueId;
                    person.RecordStatusLastModifiedDateTime = personImport.RecordStatusLastModifiedDateTime;
                    person.RecordStatusReasonValueId = personImport.RecordStatusReasonValueId;
                    person.ConnectionStatusValueId = personImport.ConnectionStatusValueId;
                    person.ReviewReasonValueId = personImport.ReviewReasonValueId;
                    person.IsDeceased = personImport.IsDeceased;
                    person.TitleValueId = personImport.TitleValueId;
                    person.FirstName = personImport.FirstName.FixCase();
                    person.NickName = personImport.NickName.FixCase();

                    if ( string.IsNullOrWhiteSpace( person.NickName ) )
                    {
                        person.NickName = person.FirstName;
                    }

                    if ( string.IsNullOrWhiteSpace( person.FirstName ) )
                    {
                        person.FirstName = person.NickName;
                    }

                    person.LastName = personImport.LastName.FixCase();
                    person.SuffixValueId = personImport.SuffixValueId;
                    person.BirthDay = personImport.BirthDay;
                    person.BirthMonth = personImport.BirthMonth;
                    person.BirthYear = personImport.BirthYear;
                    person.Gender = (Gender)personImport.Gender;
                    person.MaritalStatusValueId = personImport.MaritalStatusValueId;
                    person.AnniversaryDate = personImport.AnniversaryDate;
                    person.GraduationYear = personImport.GraduationYear;
                    person.Email = personImport.Email;
                    person.IsEmailActive = personImport.IsEmailActive;
                    person.EmailNote = personImport.EmailNote;
                    person.EmailPreference = (EmailPreference)personImport.EmailPreference;
                    person.InactiveReasonNote = personImport.InactiveReasonNote;
                    person.ConnectionStatusValueId = personImport.ConnectionStatusValueId;
                    person.ForeignId = personImport.PersonForeignId;
                    personLookup.Add( personImport.PersonForeignId, person );
                }
            }

            stopwatch.Stop();
            sbStats.AppendFormat( "[{0}ms] BuildImportLists\n", stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            double buildImportListsMS = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            bool useSqlBulkCopy = true;
            List<int> insertedPersonForeignIds = new List<int>();

            using ( var ts = new System.Transactions.TransactionScope() )
            {
                // insert all the [Group] records
                var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

                // insert all the [Person] records.
                // NOTE: we are only inserting the [Person] record, not the PersonAlias or GroupMember records yet
                var personsToInsert = personLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

                rockContext.BulkInsert( familiesToInsert, useSqlBulkCopy );

                stopwatch.Stop();
                sbStats.AppendFormat( "[{1}ms] BulkInsert {0} family(Group) records\n", familiesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
                stopwatch.Restart();

                try
                {
                    rockContext.BulkInsert( personsToInsert, useSqlBulkCopy );

                    // TODO: Figure out a good way to handle database errors since SqlBulkCopy doesn't tell you which record failed.  Maybe do it like this where we catch the exception, rollback to a EF AddRange, and then report which record(s) had the problem
                }
                catch
                {
                    try
                    {
                        // do it the EF AddRange which is slower, but it will help us determine which record fails
                        rockContext.BulkInsert( personsToInsert, false );
                    }
                    catch ( System.Data.Entity.Infrastructure.DbUpdateException dex )
                    {
                        nbResults.Text = string.Empty;
                        foreach ( var entry in dex.Entries )
                        {
                            var errorRecord = ( entry.Entity as IEntity );
                            nbResults.NotificationBoxType = NotificationBoxType.Danger;
                            nbResults.Text += string.Format( "Error on record with ForeignId: {0}, value: {1}, Error:{2}\n", errorRecord.ForeignId, errorRecord.ToString(), dex.GetBaseException().Message );
                        }

                        return;
                    }
                }

                insertedPersonForeignIds = personsToInsert.Select( a => a.ForeignId.Value ).ToList();

                stopwatch.Stop();
                sbStats.AppendFormat( "[{1}ms] BulkInsert {0} Person records\n", personsToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
                stopwatch.Restart();

                ts.Complete();
            };

            // Make sure everybody has a PersonAlias
            PersonAliasService personAliasService = new PersonAliasService( rockContext );
            var personAliasServiceQry = personAliasService.Queryable();
            List<PersonAlias> personAliasesToInsert = qryAllPersons.Where( p => p.ForeignId.HasValue && !p.Aliases.Any() && !personAliasServiceQry.Any( pa => pa.AliasPersonId == p.Id ) )
                .Select( x => new { x.Id, x.Guid } )
                .ToList()
                .Select( person => new PersonAlias { AliasPersonId = person.Id, AliasPersonGuid = person.Guid, PersonId = person.Id } ).ToList();

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Get {0} PersonAliases to insert\n", personAliasesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( personAliasesToInsert, useSqlBulkCopy );
            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert{0} PersonAliases\n", personAliasesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            // get the person Ids along with the PersonImport and GroupMember record
            var personsIdsForPersonImport = from p in qryAllPersons.AsNoTracking().Where( a => a.ForeignId.HasValue ).Select( a => new { a.Id, a.ForeignId } ).ToList()
                                            join pi in personImports on p.ForeignId equals pi.PersonForeignId
                                            join f in groupService.Queryable().Where( a => a.ForeignId.HasValue ).Select( a => new { a.Id, a.ForeignId } ).ToList() on pi.FamilyForeignId equals f.ForeignId
                                            join gm in groupMemberService.Queryable( true ).Select( a => new { a.Id, a.PersonId } ) on p.Id equals gm.PersonId into gmj
                                            from gm in gmj.DefaultIfEmpty()
                                            select new
                                            {
                                                PersonId = p.Id,
                                                PersonImport = pi,
                                                FamilyId = f.Id,
                                                HasGroupMemberRecord = gm != null
                                            };

            // Make the GroupMember records for all the imported person (unless they are already have a groupmember record for the family)
            var groupMemberRecordsToInsertQry = from ppi in personsIdsForPersonImport
                                                where !ppi.HasGroupMemberRecord
                                                select new GroupMember
                                                {
                                                    PersonId = ppi.PersonId,
                                                    GroupRoleId = ppi.PersonImport.GroupRoleId,
                                                    GroupId = ppi.FamilyId,
                                                    GroupMemberStatus = GroupMemberStatus.Active
                                                };

            var groupMemberRecordsToInsertList = groupMemberRecordsToInsertQry.ToList();
            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Get groupMemberRecordsToInsertList {0}\n", groupMemberRecordsToInsertList.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( groupMemberRecordsToInsertList, useSqlBulkCopy );
            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert groupMemberRecordsToInsertList {0}\n", groupMemberRecordsToInsertList.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            // TODO: Addresses
            List<Location> locationsToImport = new List<Location>();
            List<GroupLocation> groupLocationsToImport = new List<GroupLocation>();

            var locationCreatedDateTimeStart = RockDateTime.Now;


            //NOTE: TODO To test the "Foriegn Key Issue" , do the foreach this way: foreach ( var familyRecord in personsIdsForPersonImport.GroupBy( a => a.FamilyId ) )
            foreach ( var familyRecord in personsIdsForPersonImport.Where(a => insertedPersonForeignIds.Contains(a.PersonImport.PersonForeignId)).GroupBy( a => a.FamilyId ) )
            {
                // get the distinct addresses for each family in our import
                var familyAddresses = familyRecord.SelectMany( a => a.PersonImport.Addresses ).DistinctBy( a => new { a.Street1, a.Street2, a.City, a.County, a.State, a.Country, a.PostalCode } );

                foreach ( var address in familyAddresses )
                {
                    GroupLocation groupLocation = new GroupLocation();
                    groupLocation.GroupLocationTypeValueId = address.GroupLocationTypeValueId;
                    groupLocation.GroupId = familyRecord.Key;
                    groupLocation.IsMailingLocation = address.IsMailingLocation;
                    groupLocation.IsMappedLocation = address.IsMappedLocation;
                    
                    Location location = new Location();
                    
                    location.Street1 = address.Street1;
                    location.Street2 = address.Street2;
                    location.City = address.City;
                    location.County = address.County;
                    location.State = address.State;
                    location.Country = address.Country;
                    location.PostalCode = address.PostalCode;
                    location.CreatedDateTime = locationCreatedDateTimeStart;

                    // give the Location a Guid, and store a reference to which Location is associated with the GroupLocation record. Then we'll match them up later and do the bulk insert
                    location.Guid = Guid.NewGuid();
                    groupLocation.Location = location;

                    groupLocationsToImport.Add( groupLocation );
                    locationsToImport.Add( location );
                }
            }

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Prepare {0} Location/GroupLocation records for BulkInsert\n", locationsToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( locationsToImport, useSqlBulkCopy );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} Location records\n", locationsToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            var locationIdLookup = locationService.Queryable().Select( a => new { a.Id, a.Guid } ).ToList().ToDictionary( k => k.Guid, v => v.Id );
            foreach ( var groupLocation in groupLocationsToImport )
            {
                groupLocation.LocationId = locationIdLookup[groupLocation.Location.Guid];
            }

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Prepare {0} GroupLocation records with LocationId lookup\n", groupLocationsToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( groupLocationsToImport );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} GroupLocation records\n", groupLocationsToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();


            // TODO: PhoneNumbers
            // TODO: Attributes

            stopwatchTotal.Stop();
            sbStats.AppendFormat( "\n\nTotal: [{0}ms] \n", stopwatchTotal.Elapsed.TotalMilliseconds );

            nbResults.NotificationBoxType = NotificationBoxType.Success;
            nbResults.Text = sbStats.ToString().ConvertCrLfToHtmlBr();


            // TODO: Rebuild all indexes on the effected tables to fix bogus "Foriegn Key violation" issue
        }

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

            nbResults.Text = "Cleanup complete";
        }

        /// <summary>
        /// Gets the person imports from slingshot file.
        /// </summary>
        /// <returns></returns>
        private List<PersonImport> GetPersonImportsFromSlingshotFile()
        {
            RockContext rockContext = new RockContext();
            int personRecordTypeValueId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int adultRoleId = familyGroupType.Roles.Where( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).First().Id;

            int recordStatusValueActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            int recordStatusValueInActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() ).Id;

            int connectionStatusValueAttendeeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_ATTENDEE.AsGuid() ).Id;

            int maritalStatusMarriedId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() ).Id;
            int maritalStatusSingleId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid() ).Id;

            int phoneNumberTypeHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() ).Id;
            int phoneNumberTypeMobileId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
            int phoneNumberTypeWorkId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() ).Id;

            int groupLocationTypeHomeId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() ).Id;
            int groupLocationTypeWorkId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK.AsGuid() ).Id;

            var attributeService = new AttributeService( rockContext );
            var attributeAllergy = attributeService.GetByEntityTypeId( EntityTypeCache.Read<Person>().Id ).FirstOrDefault( a => a.Key == "Allergy" );
            var attributeBaptismDate = attributeService.GetByEntityTypeId( EntityTypeCache.Read<Person>().Id ).FirstOrDefault( a => a.Key == "BaptismDate" );

            var gradeOffsetLookup = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.SCHOOL_GRADES.AsGuid() ).DefinedValues.ToDictionary( k => k.Description.ToLower(), v => v.Value.AsInteger() );

            var personImports = new List<PersonImport>();
            var campusLookup = CampusCache.All().ToDictionary( k => k.Name, v => v.Id );


            var slingshotFileName = Server.MapPath( "~/App_Data/ccb-export.slingshot" );
            var slingshotDirectoryName = Path.Combine( Server.MapPath( "~/App_Data/slingshots" ), Path.GetFileNameWithoutExtension( slingshotFileName ) );

            var slingshotFilesDirectory = new DirectoryInfo( slingshotDirectoryName );
            if ( slingshotFilesDirectory.Exists )
            {
                slingshotFilesDirectory.Delete( true );
            }
            slingshotFilesDirectory.Create();
            ZipFile.ExtractToDirectory( slingshotFileName, slingshotFilesDirectory.FullName );

            using ( var personFileStream = File.OpenText( Path.Combine( slingshotDirectoryName, "person.csv" ) ) )
            {
                CsvReader csv = new CsvReader( personFileStream );
                csv.Configuration.HasHeaderRecord = true;
            }

                /*
                using ( var fileStream = File.OpenText( fileName ) )
                {
                    CsvReader csv = new CsvReader( fileStream );
                    csv.Configuration.HasHeaderRecord = true;
                    csv.Configuration.PrepareHeaderForMatch = ( h ) =>
                    {
                        return h.RemoveSpaces();
                    };

                    //csv.Configuration.IgnoreHeaderWhiteSpace = true;
                    //csv.Configuration.IsHeaderCaseSensitive = false;
                    //csv.Configuration.BufferSize = 999999;
                    csv.Configuration.RegisterClassMap<ImportUpdateRowMap>();

                    csv.Configuration.ThrowOnBadData = true;
                    csv.Configuration.BadDataCallback = ( s ) =>
                    {
                        System.Diagnostics.Debug.WriteLine( s );
                    };

                    foreach ( var flatRecord in csv.GetRecords<ImportUpdateRow>() )
                    {
                        var personImport = new PersonImport
                        {
                            PersonForeignId = flatRecord.IndividualID.AsInteger(),
                            FamilyForeignId = flatRecord.FamilyID.AsInteger(),
                            GroupRoleId = adultRoleId,
                            GivingIndividually = false,
                            RecordTypeValueId = personRecordTypeValueId,
                        };

                        if ( personImport.PersonForeignId == 0 || personImport.FamilyForeignId == 0 )
                        {
                            throw new Exception( "personImport.PersonForeignId == 0 || personImport.FamilyForeignId == 0" );
                        }

                        if ( !string.IsNullOrEmpty( flatRecord.Campus ) && campusLookup.ContainsKey( flatRecord.Campus ) )
                        {
                            personImport.CampusId = campusLookup[flatRecord.Campus];
                        }

                        if ( flatRecord.Inactive.Equals( "Inactive", StringComparison.OrdinalIgnoreCase ) )
                        {
                            personImport.RecordStatusValueId = recordStatusValueInActiveId;
                        }
                        else
                        {
                            personImport.RecordStatusValueId = recordStatusValueActiveId;
                        }

                        personImport.ConnectionStatusValueId = connectionStatusValueAttendeeId;

                        personImport.IsDeceased = flatRecord.Deceased_Date.AsDateTime().HasValue;

                        personImport.FirstName = flatRecord.LegalFirst;
                        personImport.NickName = flatRecord.First;
                        personImport.MiddleName = flatRecord.Middle;
                        personImport.LastName = flatRecord.Last;

                        var birthDate = flatRecord.Birthday.AsDateTime();
                        if ( birthDate != null )
                        {
                            personImport.BirthDay = birthDate.Value.Day;
                            personImport.BirthMonth = birthDate.Value.Month;
                            personImport.BirthYear = birthDate.Value.Year;
                        }

                        if ( flatRecord.Gender.Equals( "Male", StringComparison.OrdinalIgnoreCase ) || flatRecord.Gender.Equals( "M", StringComparison.OrdinalIgnoreCase ) )
                        {
                            personImport.Gender = Gender.Male;
                        }
                        else if ( flatRecord.Gender.Equals( "Female", StringComparison.OrdinalIgnoreCase ) || flatRecord.Gender.Equals( "F", StringComparison.OrdinalIgnoreCase ) )
                        {
                            personImport.Gender = Gender.Female;
                        }
                        else
                        {
                            personImport.Gender = Gender.Unknown;
                        }

                        if ( flatRecord.MaritalStatus.Equals( "Married", StringComparison.OrdinalIgnoreCase ) )
                        {
                            personImport.MaritalStatusValueId = maritalStatusMarriedId;
                        }
                        else if ( flatRecord.MaritalStatus.Equals( "Single", StringComparison.OrdinalIgnoreCase ) )
                        {
                            personImport.MaritalStatusValueId = maritalStatusSingleId;
                        }

                        personImport.AnniversaryDate = flatRecord.Anniversary.AsDateTime();

                        // determine GraduationYear from "School Grade"
                        if ( !string.IsNullOrEmpty( flatRecord.SchoolGrade ) )
                        {
                            var schoolGradeKey = flatRecord.SchoolGrade.ToLower();
                            if ( gradeOffsetLookup.ContainsKey( schoolGradeKey ) )
                            {
                                personImport.GraduationYear = Person.GraduationYearFromGradeOffset( gradeOffsetLookup[schoolGradeKey] );
                            }
                        }

                        personImport.Email = flatRecord.Email;
                        if ( flatRecord.EmailPrivacyLevel == "1" )
                        {
                            personImport.EmailPreference = EmailPreference.EmailAllowed;
                        }
                        else
                        {
                            personImport.EmailPreference = EmailPreference.DoNotEmail;
                        }

                        personImport.CreatedDateTime = flatRecord.Date_Created.AsDateTime();
                        personImport.ModifiedDateTime = flatRecord.Date_Modified.AsDateTime();

                        // Phone Numbers
                        personImport.PhoneNumbers = new List<PhoneNumberImport>();

                        if ( !string.IsNullOrEmpty( flatRecord.HomePhone.AsNumeric() ) )
                        {
                            personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.HomePhone.AsNumeric(), phoneNumberTypeHomeId ) );
                        }

                        if ( !string.IsNullOrEmpty( flatRecord.MobilePhone.AsNumeric() ) )
                        {
                            personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.MobilePhone.AsNumeric(), phoneNumberTypeMobileId ) );
                        }

                        if ( !string.IsNullOrEmpty( flatRecord.WorkPhone.AsNumeric() ) )
                        {
                            personImport.PhoneNumbers.Add( new PhoneNumberImport( flatRecord.WorkPhone.AsNumeric(), phoneNumberTypeWorkId ) );
                        }

                        // Addresses
                        personImport.Addresses = new List<AddressImport>();
                        if ( !string.IsNullOrEmpty( flatRecord.HomeStreet ) )
                        {
                            var homeAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.HomeStreet, flatRecord.HomeCity, flatRecord.HomeState, flatRecord.HomeZip )
                            {
                                IsMailingLocation = false,
                                IsMappedLocation = true
                            };

                            personImport.Addresses.Add( homeAddress );
                        }

                        if ( !string.IsNullOrEmpty( flatRecord.MailingStreet ) )
                        {
                            var mailingAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.MailingStreet, flatRecord.MailingCity, flatRecord.MailingState, flatRecord.MailingZip )
                            {
                                IsMailingLocation = true,
                                IsMappedLocation = false
                            };

                            personImport.Addresses.Add( mailingAddress );
                        }

                        if ( !string.IsNullOrEmpty( flatRecord.WorkStreet ) )
                        {
                            var workAddress = new AddressImport( groupLocationTypeHomeId, flatRecord.WorkStreet, flatRecord.WorkCity, flatRecord.WorkState, flatRecord.WorkZip )
                            {
                                IsMailingLocation = false,
                                IsMappedLocation = false
                            };

                            personImport.Addresses.Add( workAddress );
                        }

                        // Attributes
                        personImport.AttributeValues = new List<AttributeValueImport>();

                        if ( flatRecord.Allergies.IsNotNullOrWhitespace() && attributeAllergy != null )
                        {
                            personImport.AttributeValues.Add( new AttributeValueImport( attributeAllergy.Id, flatRecord.Allergies ) );
                        }

                        if ( flatRecord.Baptized.AsDateTime() != null && attributeBaptismDate != null )
                        {
                            personImport.AttributeValues.Add( new AttributeValueImport( attributeBaptismDate.Id, flatRecord.Baptized.AsDateTime() ) );
                        }

                        personImports.Add( personImport );
                    }
                }*/

                return personImports;
        }
    }
}