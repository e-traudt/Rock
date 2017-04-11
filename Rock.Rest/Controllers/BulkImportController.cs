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
        /// <summary>
        /// Bulk Import of Attendance
        /// </summary>
        /// <param name="attendanceImports">The attendance imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/AttendanceImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage AttendanceImport( [FromBody]List<Rock.BulkUpdate.AttendanceImport> attendanceImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkAttendanceImport( attendanceImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        /// <summary>
        /// Bulk Import of Groups
        /// </summary>
        /// <param name="groupImports">The group imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/GroupImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage GroupImport( [FromBody]List<Rock.BulkUpdate.GroupImport> groupImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkGroupImport( groupImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialAccountImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialAccountImport( [FromBody]List<Rock.BulkUpdate.FinancialAccountImport> financialAccountImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkFinancialAccountImport( financialAccountImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialBatchImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialBatchImport( [FromBody]List<Rock.BulkUpdate.FinancialBatchImport> financialBatchImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkFinancialBatchImport( financialBatchImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialTransactionImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialTransactionImport( [FromBody]List<Rock.BulkUpdate.FinancialTransactionImport> financialTransactionImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkFinancialTransactionImport( financialTransactionImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        /// <summary>
        /// Bulk Import of Locations
        /// </summary>
        /// <param name="locationImports">The location imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/LocationImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage LocationImport( [FromBody]List<Rock.BulkUpdate.LocationImport> locationImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkLocationImport( locationImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        /// <summary>
        /// Bulk Import of Person records
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/PersonImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage PersonImport( [FromBody]List<Rock.BulkUpdate.PersonImport> personImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkPersonImport( personImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        /// <summary>
        /// Bulk Import of Schedules
        /// </summary>
        /// <param name="scheduleImports">The schedule imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/ScheduleImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage ScheduleImport( [FromBody]List<Rock.BulkUpdate.ScheduleImport> scheduleImports )
        {
            var responseText = BulkUpdate.BulkInsertHelper.BulkScheduleImport( scheduleImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }
    }
}
