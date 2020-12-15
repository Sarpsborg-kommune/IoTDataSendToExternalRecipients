using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Sarpsborgkommune.IoT.EnergyManager
{
    public class Measurements
    {
        public Double temp { get; set; }
        public Double humidity { get; set; }
        public Double co2 { get; set; }
        public Double light { get; set; }
        public Double motion { get; set; }
        public Double bat { get; set; }
    }

    public class Sensor
    {
        public DateTime ts { get; set; }
        public Measurements v { get; set; }
    }


    public class EnergyManagerMultiSensorData
    {
        public Dictionary<string, List<Sensor>> data { get; set; }

        public string toJson() => JsonSerializer.Serialize(data);
    }
}