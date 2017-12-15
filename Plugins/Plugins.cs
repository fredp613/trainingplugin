using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;

// Microsoft Dynamics CRM namespace(s)
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;

namespace Microsoft.Crm.Sdk.Samples
{
    /// <summary>
    /// A sandboxed plug-in that can access network (Web) resources.
    /// </summary>
    /// <remarks>Register this plug-in in the sandbox. You can provide an unsecure string
    /// during registration that specifies the Web address (URI) to access from the plug-in.
    /// </remarks>
    public sealed class CreateRecordAPI : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {


            ITracingService tracingService =
               (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var _service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext ctx = new OrganizationServiceContext(_service);
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

                // Verify that the target entity represents an inquiry record.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "new_inquiry")
                    return;

                try
                {
                    // Download the target URI using a Web client. Any .NET class that uses the
                    // HTTP or HTTPS protocols and a DNS lookup should work.
                    using (WebClient client = new WebClient())
                    {

                        var data = String.Format("{{\"id\": \"{0}\", \"Response\": \"{1}\"}}", entity.Id, "frederick");
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        var response = client.UploadString(new Uri("http://rest.learncode.academy/api/myapi/inquiries/"), "POST", data);
                        var apiID = response.Substring(response.IndexOf("id\":\"") + 5, 24);                        
                        var currentRecord = _service.Retrieve("new_inquiry", entity.Id, new Xrm.Sdk.Query.ColumnSet(true));
                        currentRecord["new_apiid"] = apiID;
                        _service.Update(currentRecord);
                        tracingService.Trace("web client executed successfully, new record created:" + apiID);
                    }
                }

                catch (WebException exception)
                {
                    string str = string.Empty;
                    if (exception.Response != null)
                    {
                        using (StreamReader reader =
                            new StreamReader(exception.Response.GetResponseStream()))
                        {
                            str = reader.ReadToEnd();
                        }
                        exception.Response.Close();
                    }
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        throw new InvalidPluginExecutionException(
                            "The timeout elapsed while attempting to issue the request.", exception);
                    }
                    throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                        "A Web exception occurred while attempting to issue the request. {0}: {1}",
                        exception.Message, str), exception);
                }

            }

        }
    }

    public sealed class UpdateRecordAPI : IPlugin
    {

        public void Execute(IServiceProvider serviceProvider)
        {


            ITracingService tracingService =
               (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var _service = serviceFactory.CreateOrganizationService(context.UserId);
            OrganizationServiceContext ctx = new OrganizationServiceContext(_service);
            // The InputParameters collection contains all the data passed in the message request.
            if (context.InputParameters.Contains("Target") &&
                context.InputParameters["Target"] is Entity)
            {
                // Obtain the target entity from the input parameters.
                Entity entity = (Entity)context.InputParameters["Target"];

               // Entity apiId = (Entity)context.PreEntityImages["Image"];

                Entity postEntity = (Entity)context.PostEntityImages["postInquiry"];

                // Verify that the target entity represents an inquiry record.
                // If not, this plug-in was not registered correctly.
                if (entity.LogicalName != "new_inquiry")
                    return;

                try
                {

                    using (WebClient client = new WebClient())
                    {
                        var apiId = postEntity.GetAttributeValue<string>("new_apiid");
                        tracingService.Trace("1 web client plugin executed successfully, the record id is: " + apiId);
                        var data = String.Format("{{\"id\": \"{0}\", \"Response\": \"{1}\"}}", apiId, entity.GetAttributeValue<string>("new_response"));
                        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                        var response = client.UploadString(new Uri("http://rest.learncode.academy/api/myapi/inquiries/"+apiId), "PUT", data);
                      
                        tracingService.Trace("web client plugin executed successfully, the record id is: " + apiId);
                        
                    }
                }

                catch (WebException exception)
                {
                    string str = string.Empty;
                    if (exception.Response != null)
                    {
                        using (StreamReader reader =
                            new StreamReader(exception.Response.GetResponseStream()))
                        {
                            str = reader.ReadToEnd();
                        }
                        exception.Response.Close();
                    }
                    if (exception.Status == WebExceptionStatus.Timeout)
                    {
                        throw new InvalidPluginExecutionException(
                            "The timeout elapsed while attempting to issue the request.", exception);
                    }
                    throw new InvalidPluginExecutionException(String.Format(CultureInfo.InvariantCulture,
                        "A Web exception occurred while attempting to issue the request. {0}: {1}",
                        exception.Message, str), exception);
                }

            }

        }
    }

}
