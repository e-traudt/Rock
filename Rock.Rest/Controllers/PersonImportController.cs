﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using Rock;
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
            try
            {
                var content = this.Request.Content;
                return BulkImport( personImports );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Bulks the import.
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        private HttpResponseMessage BulkImport( List<BulkUpdate.PersonImport> personImports )
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
            string defaultPhoneCountryCode = PhoneNumber.DefaultCountryCode();

            foreach ( var personImport in personImports )
            {
                Group family = null;

                if ( personImport.FamilyForeignId.HasValue )
                {
                    if ( personImport.FamilyForeignId.HasValue )
                    {
                        if ( familiesLookup.ContainsKey( personImport.FamilyForeignId.Value ) )
                        {
                            family = familiesLookup[personImport.FamilyForeignId.Value];
                        }
                    }
                }
                else
                {
                    // TODO: If personImport.FamilyForeignId is null, that means we need to create a new family
                }

                if ( family == null )
                {
                    family = new Group();
                    family.GroupTypeId = familyGroupTypeId;
                    family.Name = string.IsNullOrEmpty( personImport.FamilyName ) ? personImport.LastName : personImport.FamilyName;

                    if ( family.Name.IsNullOrWhiteSpace() )
                    {
                        family.Name = "Family";
                    }

                    family.CampusId = personImport.CampusId;

                    family.ForeignId = personImport.FamilyForeignId;
                    familiesLookup.Add( personImport.FamilyForeignId.Value, family );
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

            // insert all the [Group] records
            var familiesToInsert = familiesLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            // insert all the [Person] records.
            // NOTE: we are only inserting the [Person] record, not the PersonAlias or GroupMember records yet
            var personsToInsert = personLookup.Where( a => a.Value.Id == 0 ).Select( a => a.Value ).ToList();

            rockContext.BulkInsert( familiesToInsert, useSqlBulkCopy );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} family(Group) records\n", familiesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            // lookup GroupId from Group.ForeignId
            var familyIdLookup = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == familyGroupTypeId && a.ForeignId.HasValue )
                .ToList().ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var personToInsertLookup = personsToInsert.ToDictionary( k => k.ForeignId.Value, v => v );
            // now that we have GroupId for each family, set the GivingGroupId for personImport's that don't give individually
            foreach ( var personImport in personImports )
            {
                if ( !personImport.GivingIndividually && personImport.FamilyForeignId.HasValue )
                {
                    var personToInsert = personToInsertLookup.GetValueOrNull( personImport.PersonForeignId );
                    if ( personToInsert != null )
                    {
                        personToInsert.GivingGroupId = familyIdLookup[personImport.FamilyForeignId.Value];
                    }
                }
            }

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
                        var errorRecord = entry.Entity as IEntity;
                    }
                }
            }

            insertedPersonForeignIds = personsToInsert.Select( a => a.ForeignId.Value ).ToList();

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} Person records\n", personsToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

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
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} PersonAliases\n", personAliasesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
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

            // narrow it down to just person records that we inserted
            personsIdsForPersonImport = personsIdsForPersonImport.Where( a => insertedPersonForeignIds.Contains( a.PersonImport.PersonForeignId ) );

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

            //NOTE: TODO To test the "Foriegn Key Issue" , don't narrow it down to just person records that we inserted
            foreach ( var familyRecord in personsIdsForPersonImport.GroupBy( a => a.FamilyId ) )
            {
                // get the distinct addresses for each family in our import
                var familyAddresses = familyRecord.Where( a => a.PersonImport?.Addresses != null ).SelectMany( a => a.PersonImport.Addresses ).DistinctBy( a => new { a.Street1, a.Street2, a.City, a.County, a.State, a.Country, a.PostalCode } );

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
                    if ( address.Latitude.HasValue && address.Longitude.HasValue )
                    {
                        location.SetLocationPointFromLatLong( address.Latitude.Value, address.Longitude.Value );
                    }

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
            var locationsToImportWithGeoSpatial = locationsToImport.Where( a => a.GeoPoint != null ).ToList();
            using ( var locationRockContext = new RockContext() )
            {
                // rockContext.BulkInsert doesn't support GeoSpatial, so we have to insert these the regular way
                locationRockContext.BulkInsert( locationsToImportWithGeoSpatial, false );
            }

            var locationsToBulkInsert = locationsToImport.Where( a => a.GeoPoint == null ).ToList();
            rockContext.BulkInsert( locationsToBulkInsert );

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

            // PhoneNumbers
            List<PhoneNumber> phoneNumbersToImport = new List<PhoneNumber>();

            foreach ( var personsIds in personsIdsForPersonImport )
            {
                foreach ( var phoneNumberImport in personsIds.PersonImport.PhoneNumbers )
                {
                    var phoneNumberToImport = new PhoneNumber();

                    phoneNumberToImport.PersonId = personsIds.PersonId;
                    phoneNumberToImport.NumberTypeValueId = phoneNumberImport.NumberTypeValueId;
                    phoneNumberToImport.CountryCode = defaultPhoneCountryCode;
                    phoneNumberToImport.Number = PhoneNumber.CleanNumber( phoneNumberImport.Number );
                    phoneNumberToImport.NumberFormatted = PhoneNumber.FormattedNumber( phoneNumberToImport.CountryCode, phoneNumberToImport.Number );
                    phoneNumberToImport.Extension = phoneNumberImport.Extension;
                    phoneNumberToImport.IsMessagingEnabled = phoneNumberImport.IsMessagingEnabled;
                    phoneNumberToImport.IsUnlisted = phoneNumberImport.IsUnlisted;

                    phoneNumbersToImport.Add( phoneNumberToImport );
                }
            }

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Prepare {0} PhoneNumber records\n", phoneNumbersToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( phoneNumbersToImport );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} PhoneNumber records\n", phoneNumbersToImport.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            // Attribute Values
            var attributeValuesToInsert = new List<AttributeValue>();
            foreach ( var personsIds in personsIdsForPersonImport )
            {
                foreach ( var attributeValueImport in personsIds.PersonImport.AttributeValues )
                {
                    var attributeValue = new AttributeValue();

                    attributeValue.EntityId = personsIds.PersonId;
                    attributeValue.AttributeId = attributeValueImport.AttributeId;
                    attributeValue.Value = attributeValueImport.Value;

                    attributeValuesToInsert.Add( attributeValue );
                }
            }

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] Prepare {0} AttributeValue records\n", attributeValuesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            rockContext.BulkInsert( attributeValuesToInsert );

            stopwatch.Stop();
            sbStats.AppendFormat( "[{1}ms] BulkInsert {0} AttributeValue records\n", attributeValuesToInsert.Count, stopwatch.Elapsed.TotalMilliseconds );
            stopwatch.Restart();

            stopwatchTotal.Stop();
            sbStats.AppendFormat( "\n\nTotal: [{0}ms] \n", stopwatchTotal.Elapsed.TotalMilliseconds );

            // TODO: Person Photo
            var personsWithPhoto = personImports.Where( a => !string.IsNullOrEmpty( a.PersonPhotoUrl ) ).ToList();
            foreach ( var personImport in personsWithPhoto )
            {
                try
                {
                    HttpWebRequest imageRequest = (HttpWebRequest)HttpWebRequest.Create( personImport.PersonPhotoUrl );
                    HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
                    var imageStream = imageResponse.GetResponseStream();
                }
                catch ( Exception ex )
                {
                    Debug.WriteLine( ex.Message );
                }
            }

            // TODO: Rebuild all indexes on the effected tables to fix bogus "Foriegn Key violation" issue
            var responseText = sbStats.ToString();
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }
    }
}
