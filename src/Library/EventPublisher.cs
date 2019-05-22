namespace Microsoft.IstioMixerPlugin.Library
{
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using Microsoft.IstioMixerPlugin.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class EventPublisher
    {
        private IHeartbeatPropertyManager heartbeatModule;
        private bool firstTime = true;
        public EventPublisher()
        {
            var telemetryModules = TelemetryModules.Instance;
            this.heartbeatModule = telemetryModules.Modules.OfType<IHeartbeatPropertyManager>().FirstOrDefault();
            Diagnostics.LogInfo("EventPubliesher initialized");
        }

        public bool UpdateClusterId(string clusterId)
        {
            bool sent = false;
            // we don't want to throw here, just log. thus in case of evel message we don't explode
            try
            {
                if (String.IsNullOrEmpty(clusterId))
                {
                    throw new ArgumentNullException("clusterId");
                }

                if (heartbeatModule != null)
                {
                    if (firstTime)
                    {
                        heartbeatModule.AddHeartbeatProperty("clusterID", clusterId, true);
                        firstTime = false;
                    }
                    else
                    {
                        heartbeatModule.SetHeartbeatProperty("clusterID", clusterId);
                    }
                    Diagnostics.LogInfo(FormattableString.Invariant($"sent update with cluesterid: {clusterId}"));
                    sent = true;
                }
                else
                {
                    Diagnostics.LogInfo(FormattableString.Invariant($"unable to send clusterId, no telemetry module detected"));
                }
            }
            catch (Exception e)
            {
                // unexpected exception occured
                Diagnostics.LogError(FormattableString.Invariant($"Unknown exception while pushing event . {e.ToString()}"));
            }
            return sent;
        }
    }
}
