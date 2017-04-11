using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.BulkUpdate
{
    public static class BulkInsertHelper
    {
        public static string BulkAttendanceImport( List<BulkUpdate.AttendanceImport> attendanceImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            var groupIdLookup = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var locationIdLookup = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            var scheduleIdLookup = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, a.ForeignId } ).ToDictionary( k => k.ForeignId.Value, v => v.Id );

            // Get the primary alias id lookup for each person foreign id
            var personAliasIdLookup = new PersonAliasService( rockContext ).Queryable().Where( a => a.Person.ForeignId.HasValue && a.PersonId == a.AliasPersonId )
                .Select( a => new { PersonAliasId = a.Id, PersonForeignId = a.Person.ForeignId } ).ToDictionary( k => k.PersonForeignId.Value, v => v.PersonAliasId );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Lookups for Attendance Insert" );
            stopwatch.Restart();

            var attendancesToInsert = new List<Attendance>( attendanceImports.Count );
            foreach ( var attendanceImport in attendanceImports )
            {
                var attendance = new Attendance();

                // NOTE: attendanceImport doesn't have to have an AttendanceForeignId and probably won't have one
                if ( attendanceImport.AttendanceForeignId.HasValue )
                {
                    attendance.ForeignId = attendanceImport.AttendanceForeignId;
                }

                attendance.CampusId = attendanceImport.CampusId;
                attendance.StartDateTime = attendanceImport.StartDateTime;
                attendance.EndDateTime = attendanceImport.EndDateTime;
                
                if ( attendanceImport.GroupForeignId.HasValue )
                {
                    attendance.GroupId = groupIdLookup.GetValueOrNull( attendanceImport.GroupForeignId.Value );
                }

                if ( attendanceImport.LocationForeignId.HasValue )
                {
                    attendance.LocationId = locationIdLookup.GetValueOrNull( attendanceImport.LocationForeignId.Value );
                }

                if ( attendanceImport.ScheduleForeignId.HasValue )
                {
                    attendance.ScheduleId = scheduleIdLookup.GetValueOrNull( attendanceImport.ScheduleForeignId.Value );
                }
                
                attendance.PersonAliasId = personAliasIdLookup.GetValueOrNull( attendanceImport.PersonForeignId );
                
                attendance.Note = attendanceImport.Note;


                attendancesToInsert.Add( attendance );
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Attendance Insert List for {attendanceImports.Count} Attendance Imports" );
            stopwatch.Restart();

            var groupIds = attendancesToInsert.Select( a => a.GroupId ).Distinct().ToList();
            var allGroupIds = new GroupService( rockContext ).Queryable().Select( a => a.Id ).ToList();
            var missing = groupIds.Where( a => !a.HasValue || !allGroupIds.Contains( a.Value ) );

            rockContext.BulkInsert( attendancesToInsert );

            sbStats.AppendLine( $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Import {attendanceImports.Count} AttendanceImports" );
            var responseText = sbStats.ToString();

            return responseText;
        }


        /// <summary>
        /// Bulks the group import.
        /// </summary>
        /// <param name="groupImports">The group imports.</param>
        /// <returns></returns>
        public static string BulkGroupImport( List<BulkUpdate.GroupImport> groupImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();
            Stopwatch stopwatch = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();
            StringBuilder sbStats = new StringBuilder();

            var qryGroupsWithForeignIds = new GroupService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var groupsAlreadyExistForeignIdHash = new HashSet<int>( qryGroupsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            var newGroupImports = groupImports.Where( a => !groupsAlreadyExistForeignIdHash.Contains( a.GroupForeignId ) ).ToList();

            var importedGroupTypeRoleNames = groupImports.GroupBy( a => a.GroupTypeId ).Select( a => new
            {
                GroupTypeId = a.Key,
                RoleNames = a.SelectMany( x => x.GroupMemberImports ).Select( x => x.RoleName ).Distinct().ToList()
            } );

            // Create any missing roles on the GroupType
            var groupTypeRolesToInsert = new List<GroupTypeRole>();

            foreach ( var importedGroupTypeRoleName in importedGroupTypeRoleNames )
            {
                var groupTypeCache = GroupTypeCache.Read( importedGroupTypeRoleName.GroupTypeId, rockContext );
                foreach ( var roleName in importedGroupTypeRoleName.RoleNames )
                {
                    if ( !groupTypeCache.Roles.Any( a => a.Name.Equals( roleName, StringComparison.OrdinalIgnoreCase ) ) )
                    {
                        var groupTypeRole = new GroupTypeRole();
                        groupTypeRole.GroupTypeId = groupTypeCache.Id;
                        groupTypeRole.Name = roleName.Truncate( 100 );
                        groupTypeRolesToInsert.Add( groupTypeRole );
                    }
                }
            }

            var updatedGroupTypes = groupTypeRolesToInsert.Select( a => a.GroupTypeId.Value ).Distinct().ToList();
            updatedGroupTypes.ForEach( id => GroupTypeCache.Flush( id ) );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Updated {groupTypeRolesToInsert.Count} GroupType Roles" );
            stopwatch.Restart();

            if ( groupTypeRolesToInsert.Any() )
            {
                rockContext.BulkInsert( groupTypeRolesToInsert );
            }

            List<Group> groupsToInsert = new List<Group>( newGroupImports.Count );

            foreach ( var groupImport in newGroupImports )
            {
                var group = new Group();
                group.ForeignId = groupImport.GroupForeignId;
                group.GroupTypeId = groupImport.GroupTypeId;
                if ( groupImport.Name.Length > 100 )
                {
                    group.Name = groupImport.Name.Truncate( 100 );
                    group.Description = groupImport.Name;
                }
                else
                {
                    group.Name = groupImport.Name;
                }

                group.Order = groupImport.Order;
                group.CampusId = groupImport.CampusId;


                groupsToInsert.Add( group );
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare {groupsToInsert.Count} Groups" );
            stopwatch.Restart();

            rockContext.BulkInsert( groupsToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupsToInsert.Count} Groups" );
            stopwatch.Restart();

            // Get lookups for Group and Person so that we can populate the ParentGroups and GroupMembers
            var groupIDLookup = qryGroupsWithForeignIds.Select( a => new { a.Id, a.ForeignId } ).ToList().ToDictionary( k => k.ForeignId.Value, v => v );
            var personIdLookup = new PersonService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue )
                .Select( a => new { a.Id, ForeignId = a.ForeignId.Value } ).ToDictionary( k => k.ForeignId, v => v.Id );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Prepare Lookups for Group Members " );
            stopwatch.Restart();

            // populate GroupMembers from the new groups that we added
            List<GroupMember> groupMembersToInsert = new List<GroupMember>();
            var groupMemberImports = newGroupImports.SelectMany( a => a.GroupMemberImports ).ToList();
            foreach ( var groupWithMembers in newGroupImports.Where( a => a.GroupMemberImports.Any() ) )
            {
                var groupTypeRoleLookup = GroupTypeCache.Read( groupWithMembers.GroupTypeId ).Roles.ToDictionary( k => k.Name, v => v.Id );
                foreach ( var groupMemberImport in groupWithMembers.GroupMemberImports )
                {
                    var groupMember = new GroupMember();
                    groupMember.GroupId = groupIDLookup[groupWithMembers.GroupForeignId].Id;
                    groupMember.GroupRoleId = groupTypeRoleLookup[groupMemberImport.RoleName];
                    groupMember.PersonId = personIdLookup[groupMemberImport.PersonForeignId];
                    groupMembersToInsert.Add( groupMember );
                }
            }

            rockContext.BulkInsert( groupMembersToInsert );

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Insert {groupMembersToInsert.Count} Group Members " );
            stopwatch.Restart();

            var groupsUpdated = false;
            var groupImportsWithParentGroup = newGroupImports.Where( a => a.ParentGroupForeignId.HasValue ).ToList();
            var groupLookup = qryGroupsWithForeignIds.ToDictionary( k => k.ForeignId.Value, v => v );
            foreach ( var groupImport in groupImportsWithParentGroup )
            {
                var group = groupLookup.GetValueOrNull( groupImport.GroupForeignId );
                if ( group != null )
                {
                    var parentGroup = groupLookup.GetValueOrNull( groupImport.ParentGroupForeignId.Value );
                    if ( parentGroup != null && group.ParentGroupId != parentGroup.Id )
                    {
                        group.ParentGroupId = parentGroup.Id;
                        groupsUpdated = true;
                    }
                    else
                    {
                        sbStats.AppendLine( $"ERROR: Unable to lookup ParentGroup {groupImport.ParentGroupForeignId} for Group {groupImport.Name}:{groupImport.GroupForeignId} " );
                    }
                }
                else
                {
                    throw new Exception( "Unable to lookup Group with ParentGroup" );
                }
            }

            if ( groupsUpdated )
            {
                rockContext.SaveChanges( true );
            }

            stopwatch.Stop();
            sbStats.AppendLine( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] Update {groupImportsWithParentGroup.Count} Group's Parent Group " );
            stopwatch.Restart();

            stopwatchTotal.Stop();

            sbStats.AppendLine( $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Import {newGroupImports.Count} GroupImports and {groupMembersToInsert.Count} GroupMemberImports" );
            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Bulks the location import.
        /// </summary>
        /// <param name="locationImports">The location imports.</param>
        /// <returns></returns>
        public static string BulkLocationImport( List<BulkUpdate.LocationImport> locationImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qryLocationsWithForeignIds = new LocationService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var locationsAlreadyExistForeignIdHash = new HashSet<int>( qryLocationsWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<Location> locationsToImport = new List<Location>();
            var newLocationImports = locationImports.Where( a => !locationsAlreadyExistForeignIdHash.Contains( a.LocationForeignId ) ).ToList();

            foreach ( var locationImport in newLocationImports )
            {
                var location = new Location();
                location.ForeignId = locationImport.LocationForeignId;
                location.LocationTypeValueId = locationImport.LocationTypeValueId;

                location.Street1 = locationImport.Street1.Truncate( 50 );
                location.Street2 = locationImport.Street2.Truncate( 50 );
                location.City = locationImport.City;
                location.County = locationImport.County;
                location.State = locationImport.State;
                location.Country = locationImport.Country;
                location.PostalCode = locationImport.PostalCode;

                location.Name = locationImport.Name.Truncate( 100 );
                location.IsActive = locationImport.IsActive;
                locationsToImport.Add( location );
            }

            rockContext.BulkInsert( locationsToImport );

            // Get the Location records for the locations that we imported so that we can populate the ParentLocations
            var locationLookup = qryLocationsWithForeignIds.ToList().ToDictionary( k => k.ForeignId.Value, v => v );
            var locationsUpdated = false;
            foreach ( var locationImport in newLocationImports.Where( a => a.ParentLocationForeignId.HasValue ) )
            {
                var location = locationLookup.GetValueOrNull( locationImport.LocationForeignId );
                if ( location != null )
                {
                    var parentLocation = locationLookup.GetValueOrNull( locationImport.ParentLocationForeignId.Value );
                    if ( parentLocation != null && location.ParentLocationId != parentLocation.Id )
                    {
                        location.ParentLocationId = parentLocation.Id;
                        locationsUpdated = true;
                    }
                }
            }

            if ( locationsUpdated )
            {
                rockContext.SaveChanges();
            }

            stopwatchTotal.Stop();
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Import {newLocationImports.Count} LocationImports";

            return responseText;
        }

        /// <summary>
        /// Bulks the import.
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        public static string BulkPersonImport( List<BulkUpdate.PersonImport> personImports )
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
                    person.Gender = ( Gender ) personImport.Gender;
                    person.MaritalStatusValueId = personImport.MaritalStatusValueId;
                    person.AnniversaryDate = personImport.AnniversaryDate;
                    person.GraduationYear = personImport.GraduationYear;
                    person.Email = personImport.Email;
                    person.IsEmailActive = personImport.IsEmailActive;
                    person.EmailNote = personImport.EmailNote;
                    person.EmailPreference = ( EmailPreference ) personImport.EmailPreference;
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
                locationRockContext.BulkInsert( locationsToImportWithGeoSpatial );
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

            // TODO: Person Photo
            /*
            var personsWithPhoto = personImports.Where( a => !string.IsNullOrEmpty( a.PersonPhotoUrl ) ).ToList();
            int photoExceptionCount = 0;
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
                    photoExceptionCount++;
                }
            }

            stopwatch.Stop();
            sbStats.Append( $"[{stopwatch.Elapsed.TotalMilliseconds}ms] UpdatePersonPhotos {personsWithPhoto.Count} personsWithPhoto records, {photoExceptionCount} exceptions \n");
            stopwatch.Restart();
            */

            stopwatchTotal.Stop();
            sbStats.AppendFormat( "\n\nTotal: [{0}ms] \n", stopwatchTotal.Elapsed.TotalMilliseconds );

            // TODO: Rebuild all indexes on the effected tables to fix bogus "Foriegn Key violation" issue
            var responseText = sbStats.ToString();

            return responseText;
        }

        /// <summary>
        /// Bulks the schedule import.
        /// </summary>
        /// <param name="scheduleImports">The schedule imports.</param>
        /// <returns></returns>
        public static string BulkScheduleImport( List<BulkUpdate.ScheduleImport> scheduleImports )
        {
            Stopwatch stopwatchTotal = Stopwatch.StartNew();

            RockContext rockContext = new RockContext();

            var qrySchedulesWithForeignIds = new ScheduleService( rockContext ).Queryable().Where( a => a.ForeignId.HasValue );

            var scheduleAlreadyExistForeignIdHash = new HashSet<int>( qrySchedulesWithForeignIds.Select( a => a.ForeignId.Value ).ToList() );

            List<Schedule> schedulesToImport = new List<Schedule>();
            var newScheduleImports = scheduleImports.Where( a => !scheduleAlreadyExistForeignIdHash.Contains( a.ScheduleForeignId ) ).ToList();

            foreach ( var scheduleImport in newScheduleImports )
            {
                var schedule = new Schedule();
                schedule.ForeignId = scheduleImport.ScheduleForeignId;
                if ( scheduleImport.Name.Length > 50 )
                {
                    schedule.Name = scheduleImport.Name.Truncate( 50 );
                    schedule.Description = scheduleImport.Name;
                }
                else
                {
                    schedule.Name = scheduleImport.Name;
                }

                schedulesToImport.Add( schedule );
            }

            rockContext.BulkInsert( schedulesToImport );

            stopwatchTotal.Stop();
            var responseText = $"[{stopwatchTotal.Elapsed.TotalMilliseconds}ms] Import {schedulesToImport.Count} ScheduleImports";

            return responseText;
        }
    }
}
