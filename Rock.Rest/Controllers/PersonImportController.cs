using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    public class PersonImportController : ApiControllerBase
    {
        [System.Web.Http.Route( "api/PersonImport" )]
        [HttpPost]
       // [RequireHttps]
      //  [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage Post( [FromBody]List<Rock.BulkUpdate.PersonImport> personImports )
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
                    //family.CampusId = personImport.Campus.

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

            //using ( var ts = new System.Transactions.TransactionScope() )
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
                       // nbResults.Text = string.Empty;
                        foreach ( var entry in dex.Entries )
                        {
                            var errorRecord = ( entry.Entity as IEntity );
                         //   nbResults.NotificationBoxType = NotificationBoxType.Danger;
                            //nbResults.Text += string.Format( "Error on record with ForeignId: {0}, value: {1}, Error:{2}\n", errorRecord.ForeignId, errorRecord.ToString(), dex.GetBaseException().Message );
                        }

                       // return;
                    }
                }

                insertedPersonForeignIds = personsToInsert.Select( a => a.ForeignId.Value ).ToList();

                stopwatch.Stop();
                sbStats.AppendFormat( "[{1}ms] BulkInsert {0} Person records\n", personsToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
                stopwatch.Restart();

               // ts.Complete();
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
            sbStats.AppendFormat( "[{1}ms] Get groupMemberRecordsToInsertList {0}\n", groupMemberRecordsToInsertList.Count(), stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( groupMemberRecordsToInsertList, useSqlBulkCopy );
            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert groupMemberRecordsToInsertList {0}\n", groupMemberRecordsToInsertList.Count(), stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            List<Location> locationsToImport = new List<Location>();
            List<GroupLocation> groupLocationsToImport = new List<GroupLocation>();

            var locationCreatedDateTimeStart = RockDateTime.Now;


            //NOTE: TODO To test the "Foriegn Key Issue" , do the foreach this way: foreach ( var familyRecord in personsIdsForPersonImport.GroupBy( a => a.FamilyId ) )
            foreach ( var familyRecord in personsIdsForPersonImport.Where( a => insertedPersonForeignIds.Contains( a.PersonImport.PersonForeignId ) ).GroupBy( a => a.FamilyId ) )
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

            //nbResults.NotificationBoxType = NotificationBoxType.Success;
            //nbResults.Text = sbStats.ToString().ConvertCrLfToHtmlBr();


            // TODO: Rebuild all indexes on the effected tables to fix bogus "Foriegn Key violation" issue







            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
        }
    }
}
