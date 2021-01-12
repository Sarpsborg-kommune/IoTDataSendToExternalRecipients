using System;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Sarpsborgkommune.IoT.IoTMessage;
using Sarpsborgkommune.IoT.EnergyManager;


namespace Sarpsborgkommune.IoT.DataSendToEnergyManager
{
    public static class IoTDataSendToEnergyManager
    {
        [FunctionName("IoTDataSendToEnergyManager")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestMessage req,
            [EventHub("dest", Connection = "EventHubEndpointEM")] IAsyncCollector<byte[]> outputEvents, ILogger log)
        {
            List<ElsysIoTMessage> iotData = new List<ElsysIoTMessage>();
            Stopwatch timerTotal = new Stopwatch();
            Stopwatch timerSend = new Stopwatch();

            timerTotal.Start();

            string content = await req.Content.ReadAsStringAsync();
            log.LogInformation($"Recieved data: {content}");

            try
            {
                iotData = JsonSerializer.Deserialize<List<ElsysIoTMessage>>(content);
            }
            catch (Exception ex)
            {
                // OK is returned because there is no internal error only that the received data
                // is invalid JSON. This should lead to data being discarded.
                log.LogError($"Input is not valid JSON. Data not sendt. {ex.Message}");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Input is not valid JSON.", Encoding.UTF8, "text/plain"),
                };
            }

            log.LogInformation($"Data is valid and deserialized. Elements: {iotData.Count}");

            foreach (var iotDataElement in iotData)
            {
                string emSendData;

                // This IF should validate that the iotDataElement contains data
                if (!string.IsNullOrEmpty(iotDataElement.id) &&
                     !(iotDataElement.timeStamp == null) &&
                     !string.IsNullOrEmpty(iotDataElement.deviceType) &&
                     !(iotDataElement.data == null))
                {
                    // Transform data to EnergyManager format.
                    try
                    {
                        // Dictionary, List, Sensor, Measurements is created to form the necessary
                        // EnergyManage datastructure
                        EnergyManagerMultiSensorData emData = new EnergyManagerMultiSensorData();
                        emData.data = new Dictionary<string, List<Sensor>>();
                        List<Sensor> sItemList = new List<Sensor>();
                        Sensor sItem = new Sensor();
                        sItem.v = new Measurements();

                        sItem.ts = iotDataElement.timeStamp;
                        if (iotDataElement.data.temp != null) sItem.v.temp = (Double)iotDataElement.data.temp;
                        if (iotDataElement.data.rh != null) sItem.v.humidity = (Double)iotDataElement.data.rh;
                        if (iotDataElement.data.light != null) sItem.v.light = (Double)iotDataElement.data.light;
                        if (iotDataElement.data.motion != null) sItem.v.motion = Math.Min((Double)iotDataElement.data.motion / 255.0, 1.0); // Normalized
                        if (iotDataElement.data.co2 != null) sItem.v.co2 = (Double)iotDataElement.data.co2;
                        if (iotDataElement.data.vdd != null) sItem.v.bat = Math.Min((Double)iotDataElement.data.vdd / 3600.0, 1.0);      // Normalized

                        if ((sItem.v.temp != null) ||
                             (sItem.v.humidity != null) ||
                             (sItem.v.light != null) ||
                             (sItem.v.motion != null) ||
                             (sItem.v.co2 != null) ||
                             (sItem.v.bat != null))
                        {
                            sItemList.Add(sItem);
                            emData.data.Add(iotDataElement.id, sItemList);

                            emSendData = JsonSerializer.Serialize(emData.data);

                            log.LogInformation("Data transformed to EnergyManager format:");
                            log.LogInformation($"{emSendData}");

                            // Data is discarded if send fails.
                            try
                            {
                                // var emSendDataMessage = new Message(Encoding.ASCII.GetBytes(emSendData));

                                timerSend.Start();
                                //await outputEvents.AddAsync(Encoding.ASCII.GetBytes(emSendData));
                                await outputEvents.AddAsync(Encoding.ASCII.GetBytes(emSendData));
                                timerSend.Stop();
                                log.LogInformation($"Data sendt to EnergyManager. Send time {timerSend.ElapsedMilliseconds} ms.");
                            }
                            catch (Exception ex)
                            {
                                log.LogError($"Unable to connect or send message to external IoTHub. {ex.Message}");
                            }
                        }
                        else
                        {
                            log.LogError($"JSON does not contain any measurements to send, data discarded.");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Unexpected Data transformation error. {ex.Message}");
                    }
                }
                else
                {
                    log.LogError($"JSON does not contain necessary data for the external receiver, data discarded.");
                }
            }

            timerTotal.Stop();
            log.LogInformation($"Total function time: {timerTotal.ElapsedMilliseconds} ms.");

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Success", Encoding.UTF8, "text/plain"),
            };
        }
    }
}