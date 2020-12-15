using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Sarpsborgkommune.IoT.IoTMessage;
using Sarpsborgkommune.IoT.EnergyManager;


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

            var iotData = JsonSerializer.Deserialize<List<ElsysIoTMessage>>(content);

            EnergyManagerMultiSensorData emData = new EnergyManagerMultiSensorData();
            emData.data = new Dictionary<string, List<Sensor>>();
            Sensor sItem = new Sensor();
            List<Sensor> sItemList = new List<Sensor>();

            sItem.ts = iotData[0].timeStamp;
            sItem.v = new Measurements();
            sItem.v.temp = iotData[0].data.temp;
            sItem.v.humidity = iotData[0].data.rh;
            sItem.v.light = iotData[0].data.light;
            sItem.v.motion = iotData[0].data.motion;
            sItem.v.co2 = iotData[0].data.co2;
            sItem.v.bat = iotData[0].data.vdd;

            sItemList.Add(sItem);
            emData.data.Add(iotData[0].id, sItemList);
            log.LogInformation($"Data: {JsonSerializer.Serialize(emData.data)}");
            try
            {
                String connection = Environment.GetEnvironmentVariable("IoTHubEndpointEM");
                DeviceClient deviceClient = DeviceClient.CreateFromConnectionString(connection, TransportType.Mqtt);
                await deviceClient.SendEventAsync(new Message(Encoding.ASCII.GetBytes(JsonSerializer.Serialize(emData.data))));
            }
            catch
            {
                req.CreateResponse(System.Net.HttpStatusCode.InternalServerError, "Unable to connect or send send message to external IoTHub");
                log.LogError("Unable to connect or send message to external IoTHub.");
            }
            log.LogInformation("Data sendt.");
            return req.CreateResponse(System.Net.HttpStatusCode.OK, "Success");

        }
    }
}