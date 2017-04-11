using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class BulkImportController : ApiControllerBase
    {
        [System.Web.Http.Route( "api/BulkImport/AttendanceImport" )]
        [HttpPost]
        // [RequireHttps]
        //  [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage GroupImport( [FromBody]List<Rock.BulkUpdate.AttendanceImport> attendanceImports )
        {
            try
            {
                var responseText = BulkUpdate.BulkInsertHelper.BulkAttendanceImport( attendanceImports );
                return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }


        [System.Web.Http.Route( "api/BulkImport/GroupImport" )]
        [HttpPost]
        // [RequireHttps]
        //  [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage GroupImport( [FromBody]List<Rock.BulkUpdate.GroupImport> groupImports )
        {
            try
            {
                var responseText = BulkUpdate.BulkInsertHelper.BulkGroupImport( groupImports );
                return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }


        /// <summary>
        /// Persons the import.
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route("api/BulkImport/PersonImport")]
        [HttpPost]
        // [RequireHttps]
        //  [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage PersonImport( [FromBody]List<Rock.BulkUpdate.PersonImport> personImports )
        {
            try
            {
                var responseText = BulkUpdate.BulkInsertHelper.BulkPersonImport( personImports );
                return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Locations the import.
        /// </summary>
        /// <param name="locationImports">The location imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/LocationImport" )]
        [HttpPost]
        // [RequireHttps]
        //  [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage LocationImport( [FromBody]List<Rock.BulkUpdate.LocationImport> locationImports )
        {
            try
            {
                var responseText = BulkUpdate.BulkInsertHelper.BulkLocationImport( locationImports );
                return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        [System.Web.Http.Route( "api/BulkImport/ScheduleImport" )]
        [HttpPost]
        //[RequireHttps]
        //[Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage ScheduleImport( [FromBody]List<Rock.BulkUpdate.ScheduleImport> scheduleImports )
        {
            try
            {
                var responseText = BulkUpdate.BulkInsertHelper.BulkScheduleImport( scheduleImports );
                return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }
    }
}
