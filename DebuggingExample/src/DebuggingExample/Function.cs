using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json;
using Amazon.XRay.Recorder.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(
typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DebuggingExample
{
    public class Functions
    {
        ITimeProcessor processor = new TimeProcessor();

        public APIGatewayProxyResponse Get(APIGatewayProxyRequest request, ILambdaContext context)
        {
            LogMessage(context, "Processing request started");

            APIGatewayProxyResponse response;
            try
            {   
                //var result = processor.CurrentTimeUTC();
                var result = TraceFunction(processor.CurrentTimeUTC, "GetTime");
                response = CreateResponse(result);

                LogMessage(context, "Processing request succeeded.");
            }
            catch (Exception ex)
            {
                LogMessage(context, string.Format("Processing request failed - {0}", ex.Message));
                response = CreateResponse(null);
            }

            return response;
        }

        APIGatewayProxyResponse CreateResponse(DateTime? result)
        {
            int statusCode = (result != null) ?
                (int)HttpStatusCode.OK :
                (int)HttpStatusCode.InternalServerError;

            string body = (result != null) ?
                JsonConvert.SerializeObject(result) : string.Empty;

            var response = new APIGatewayProxyResponse
            {
                StatusCode = statusCode,
                Body = body,
                Headers = new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Access-Control-Allow-Origin", "*" }
        }
            };

            return response;
        }

        private void LogMessage(ILambdaContext context, string message)
        {
            context.Logger.LogLine(string.Format("{0}:{1} - {2}", context.AwsRequestId, context.FunctionName, message));
        }

        private T TraceFunction<T>(Func<T> func, string subSegmentName)
        {
            AWSXRayRecorder.Instance.BeginSubsegment(subSegmentName);
            T result = func();
            AWSXRayRecorder.Instance.EndSubsegment();

            return result;
        } 
    }
}