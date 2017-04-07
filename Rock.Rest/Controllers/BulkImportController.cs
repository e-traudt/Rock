using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Rock.Rest.Controllers
{
    public class BulkImportController : ApiControllerBase
    {
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
    }
}
