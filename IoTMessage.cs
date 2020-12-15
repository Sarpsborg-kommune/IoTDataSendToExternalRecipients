/*
 * Every received message is translated to IoTMessage and sendt into the pipeline.
 *
 * Loriot: timeStamp is translated from ts (Linux Epoch)
 *         data is translated from original bytestring (data)
 *
*/

using System;

namespace Sarpsborgkommune.IoT.IoTMessage
{
    public class ElsysEnvData
    {
        public float temp { get; set; }
        public int rh { get; set; }
        public int light { get; set; }
        public int motion { get; set; }
        public int co2 { get; set; }
        public int vdd { get; set; }
    }


    public class ElsysIoTMessage
    {
        public string id { get; set; }              // From received message
        public DateTime timeStamp { get; set; }     // From received message
        public string deviceType { get; set; }      // From device twin
        public ElsysEnvData data { get; set; }
    }
}