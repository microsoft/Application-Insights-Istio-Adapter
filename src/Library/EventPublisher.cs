namespace Microsoft.IstioMixerPlugin.Library
{
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
    using Microsoft.IstioMixerPlugin.Common;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    internal class HeartbeatCustomizer
    {
        private IHeartbeatPropertyManager heartbeatModule;
        private bool firstTimeUpdate = true;
        public HeartbeatCustomizer()
        {
            var telemetryModules = TelemetryModules.Instance;
            this.heartbeatModule = telemetryModules.Modules.OfType<IHeartbeatPropertyManager>().FirstOrDefault();
            Diagnostics.LogInfo("HeartbeatCustomizer initialized");
        }

        public bool UpdateClusterId(string clusterId)
        {
            bool sent = false;
            // we don't want to throw here, just log. thus in case of empty cluster id in the message message we don't want to throw, but we will log it
            try
            {
                if (String.IsNullOrEmpty(clusterId))
                {
                    throw new ArgumentNullException("clusterId");
                }

                if (heartbeatModule != null)
                {
                    if (firstTimeUpdate)
                    {
                        heartbeatModule.AddHeartbeatProperty("clusterID", clusterId, true);
                        firstTimeUpdate = false;
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
