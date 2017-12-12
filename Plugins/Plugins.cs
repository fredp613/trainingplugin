
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Net;

// Microsoft Dynamics CRM namespace(s)
using Microsoft.Xrm.Sdk;
using System.ServiceModel;

namespace Plugins
{
    public class UpdateAPI : IPlugin
    {
		public void Execute(IServiceProvider serviceProvider)
		{
			//Extract the tracing service for use in debugging sandboxed plug-ins.
			ITracingService tracingService =
				(ITracingService)serviceProvider.GetService(typeof(ITracingService));

			// Obtain the execution context from the service provider.
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			// The InputParameters collection contains all the data passed in the message request.
			if (context.InputParameters.Contains("Target") &&
				context.InputParameters["Target"] is Entity)
			{
				// Obtain the target entity from the input parameters.
				Entity entity = (Entity)context.InputParameters["Target"];

				// Verify that the target entity represents an account.
				// If not, this plug-in was not registered correctly.
				if (entity.LogicalName != "new_inquiry")
					return;

				try
				{
					FaultException ex = new FaultException();
					var response = entity.GetAttributeValue<string>("new_response");
					tracingService.Trace("Plugin is working");
					// throw new InvalidPluginExecutionException("Plugin is working", ex);
                    ApiActions.UpdateAPI(entity.Id, response);



				}
				catch (FaultException<OrganizationServiceFault> ex)
				{
					throw new InvalidPluginExecutionException("An error occurred in the FollowupPlugin plug-in.", ex);
				}

				catch (Exception ex)
				{
					tracingService.Trace("FollowupPlugin: {0}", ex.ToString());
					throw;
				}


			}
		}
        
    }

	public class InsertAPI : IPlugin
	{
		public void Execute(IServiceProvider serviceProvider)
		{
			//Extract the tracing service for use in debugging sandboxed plug-ins.
			ITracingService tracingService =
				(ITracingService)serviceProvider.GetService(typeof(ITracingService));

			// Obtain the execution context from the service provider.
			IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

			// The InputParameters collection contains all the data passed in the message request.
			if (context.InputParameters.Contains("Target") &&
				context.InputParameters["Target"] is Entity)
			{
				// Obtain the target entity from the input parameters.
				Entity entity = (Entity)context.InputParameters["Target"];

				// Verify that the target entity represents an account.
				// If not, this plug-in was not registered correctly.
				if (entity.LogicalName != "new_inquiry")
					return;

				try
				{
					FaultException ex = new FaultException();
					var response = entity.GetAttributeValue<string>("new_response");
                    var question = entity.GetAttributeValue<string>("new_question");
					tracingService.Trace("Plugin is working");
					// throw new InvalidPluginExecutionException("Plugin is working", ex);
					ApiActions.InsertAPI(entity.Id, question, response);



				}
				catch (FaultException<OrganizationServiceFault> ex)
				{
					throw new InvalidPluginExecutionException("An error occurred in the FollowupPlugin plug-in.", ex);
				}

				catch (Exception ex)
				{
					tracingService.Trace("FollowupPlugin: {0}", ex.ToString());
					throw;
				}


			}
		}

	}

    public class ApiActions {
        public static void UpdateAPI(Guid id, string response)
        {
                      
			var data = String.Format("{{\"InquiryId\": \"{0}\", \"Response\": \"{1}\"}}", id, response);
			var client = new WebClient();
			client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			client.UploadStringAsync(new Uri("http://rest.learncode.academy/api/fredp/inquiries/" + id.ToString()), "PUT", data);
		}
		public static void InsertAPI(Guid id, string question, string response)
		{
			var data = String.Format("{{\"InquiryId\": \"{0}\", \"Question\": \"{1}\", \"Response\": \"{2}\"}}", id, question, response);
			var client = new WebClient();
			client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
			client.UploadStringAsync(new Uri("http://rest.learncode.academy/api/fredp/inquiries/" + id.ToString()), "POST", data);
		}
    }
        
}
