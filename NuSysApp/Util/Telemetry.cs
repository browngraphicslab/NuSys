using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace NuSysApp
{
    public class Telemetry
    {

        private static TelemetryClient _client;

        public static void Init()
        {
            //var config = new TelemetryConfiguration();
            
            _client = new TelemetryClient();

        }

        public static void TrackEvent(string eventstring)
        {
            _client.TrackEvent(eventstring);
        }
    }
}
