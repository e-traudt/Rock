﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        public System.Net.Http.HttpResponseMessage AttendanceImport( [FromBody]List<Rock.BulkImport.Model.AttendanceImport> attendanceImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkAttendanceImport( attendanceImports );
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
        public System.Net.Http.HttpResponseMessage GroupImport( [FromBody]List<Rock.BulkImport.Model.GroupImport> groupImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkGroupImport( groupImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialAccountImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialAccountImport( [FromBody]List<Rock.BulkImport.Model.FinancialAccountImport> financialAccountImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkFinancialAccountImport( financialAccountImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialBatchImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialBatchImport( [FromBody]List<Rock.BulkImport.Model.FinancialBatchImport> financialBatchImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkFinancialBatchImport( financialBatchImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        [System.Web.Http.Route( "api/BulkImport/FinancialTransactionImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage FinancialTransactionImport( [FromBody]List<Rock.BulkImport.Model.FinancialTransactionImport> financialTransactionImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkFinancialTransactionImport( financialTransactionImports );
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
        public System.Net.Http.HttpResponseMessage LocationImport( [FromBody]List<Rock.BulkImport.Model.LocationImport> locationImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkLocationImport( locationImports );
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
        public System.Net.Http.HttpResponseMessage PersonImport( [FromBody]List<Rock.BulkImport.Model.PersonImport> personImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkPersonImport( personImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }

        /// <summary>
        /// Bulk Import of Family or Person Photo records
        /// </summary>
        /// <param name="personImports">The person imports.</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/BulkImport/PhotoImport" )]
        [HttpPost]
        [Authenticate, Secured]
        public System.Net.Http.HttpResponseMessage PhotoImport( [FromBody]List<Rock.BulkImport.Model.PhotoImport> photoImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkPhotoImport( photoImports );
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
        public System.Net.Http.HttpResponseMessage ScheduleImport( [FromBody]List<Rock.BulkImport.Model.ScheduleImport> scheduleImports )
        {
            var responseText = BulkImport.BulkImportHelper.BulkScheduleImport( scheduleImports );
            return ControllerContext.Request.CreateResponse<string>( HttpStatusCode.Created, responseText );
        }
    }
}
