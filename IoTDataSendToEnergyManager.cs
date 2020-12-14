using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;


namespace Sarpsborgkommune.IoT.DataSendToEnergyManager
{
    public static class IoTDataSendToEnergyManager
    {
        [FunctionName("IoTDataSendToEnergyManager")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req, ILogger log)
        {
            log.LogInformation($"IoTDataSendToExternalRecipients triggered with URI: {req.RequestUri}");

            string content = await req.Content.ReadAsStringAsync();
            log.LogInformation($"Content is: {content}");
            /*dynamic data = JsonSerializer.Deserialize(content);

            log.LogInformation($"Data Count is: {data?.Count}");

            if (data?.ToString()?.Length > 262144)
            {
                return new HttpResponseMessage(HttpStatusCode.RequestEntityTooLarge);
            }*/
            return req.CreateResponse(System.Net.HttpStatusCode.OK, "Success");
        }
    }
}