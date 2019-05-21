namespace Microsoft.IstioMixerPlugin.Library
{
    using Microsoft.ApplicationInsights;
    using Microsoft.IstioMixerPlugin.Common;
    using System;
    using System.Collections.Generic;
    internal class EventPublisher
    {
        private TelemetryClient telemetryClient;
        public EventPublisher(TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient ?? throw new ArgumentNullException("telemetryClient");
            Diagnostics.LogInfo("EventPubliesher initialized");
        }

        public void UpdateClusterId(string clusterId)
        {
            try
            {
                Dictionary<string, string> properties = AddToDictionary("cluesterId", clusterId);
                this.telemetryClient.TrackEvent("CluesterId", properties);
                Diagnostics.LogInfo(FormattableString.Invariant($"sent update with cluesterid: {clusterId}"));
            }
            catch (Exception e)
            {
                // unexpected exception occured
                Diagnostics.LogError(FormattableString.Invariant($"Unknown exception while pushing event . {e.ToString()}"));
            }
        }

        private Dictionary<string, T> AddToDictionary<T>(string key, T value, Dictionary<string, T> input = null)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            Dictionary<string, T> output = input ?? new Dictionary<string, T>();
            output[key] = value;
            return output;
        }
    }
}
