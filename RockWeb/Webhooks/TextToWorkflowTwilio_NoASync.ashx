﻿<%@ WebHandler Language="C#" Class="TextToWorkflowTwilio_NoASync" %>

using System;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Newtonsoft.Json;
using Rock;
using Rock.Model;
using Rock.Web.Cache;
using com.minecartstudio.TextToWorkflow;

public class TextToWorkflowTwilio_NoASync : IHttpHandler
{
    private HttpRequest request;
    private HttpResponse response;

    private static int requestItem = 0;

    public void ProcessRequest( HttpContext context )
    {
        request = context.Request;
        response = context.Response;
        requestItem++;

        response.ContentType = "text/plain";

        if ( request.HttpMethod != "POST" )
        {
            response.Write( "Invalid request type." );
            return;
        }


        if ( request.Form["SmsStatus"] != null )
        {
            switch ( request.Form["SmsStatus"] )
            {
                case "received":
                    string fromPhone = string.Empty;
                    string toPhone = string.Empty;
                    string message = string.Empty;


                    if ( !string.IsNullOrEmpty( request.Form["To"] ) )
                    {
                        toPhone = request.Form["To"];
                    }

                    if ( !string.IsNullOrEmpty( request.Form["From"] ) )
                    {
                        fromPhone = request.Form["From"];
                    }

                    if ( !string.IsNullOrEmpty( request.Form["Body"] ) )
                    {
                        message = request.Form["Body"];
                    }

                    string processResponse = string.Empty;

                    TextToWorkflowUtility.MessageRecieved( toPhone, fromPhone, message, out processResponse );
                    System.Threading.Thread.Sleep( 5000 );

                    if ( processResponse != string.Empty )
                    {
                        response.Write( processResponse );
                    }

                    break;
            }

            response.StatusCode = 200;
        }
        else
        {
            response.StatusCode = 500;
        }

    }

    public bool IsReusable
    {
        get
        {
            return false;
        }
    }
}