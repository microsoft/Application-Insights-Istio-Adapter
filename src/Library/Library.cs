namespace Microsoft.IstioMixerPlugin.Library
{
    using ApplicationInsights;
    using Common;
    using Inputs.GrpcInput;
    using Microsoft.ApplicationInsights.Extensibility;
    using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Exception = System.Exception;
    using Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel;
    using Tracespan;

    public class Library
    {
        private readonly TelemetryClient telemetryClient;

        private readonly GrpcInput istioMixerInput;
        
        private readonly Configuration config;

        private readonly TelemetryGenerator telemetryGenerator;

        private readonly string instrumentationKey;
        private readonly string liveMetricsStreamAuthenticationApiKey;
        
        private readonly TimeSpan statsTracingTimeout = TimeSpan.FromMinutes(1);
        
        /// <summary>
        /// For unit tests only.
        /// </summary>
        internal Library(string configuration, TelemetryClient telemetryClient, TimeSpan? statsTracingTimeout = null) : this(configuration)
        {
            this.telemetryClient = telemetryClient;
            this.statsTracingTimeout = statsTracingTimeout ?? this.statsTracingTimeout;
        }

        public bool IsRunning { get; private set; } = false;

        public Library(string configuration)
        {
            this.config = new Configuration(configuration);

            this.instrumentationKey = this.config.InstrumentationKey;
            this.liveMetricsStreamAuthenticationApiKey = this.config.LiveMetricsStreamAuthenticationApiKey;
            
            Diagnostics.LogInfo(
                FormattableString.Invariant($"Loaded configuration. {Environment.NewLine}{configuration}"));

            this.telemetryGenerator = new TelemetryGenerator(this.config.Watchlist_Namespaces, this.config.Watchlist_IgnoredNamespaces);

            try
            {
                var activeConfiguration = TelemetryConfiguration.Active;
                activeConfiguration.InstrumentationKey = this.instrumentationKey;

                var channel = new ServerTelemetryChannel();
                channel.Initialize(activeConfiguration);
                activeConfiguration.TelemetryChannel = channel;

                var builder = activeConfiguration.DefaultTelemetrySink.TelemetryProcessorChainBuilder;

                QuickPulseTelemetryProcessor processor = null;
                builder.Use((next) =>
                {
                    processor = new QuickPulseTelemetryProcessor(next);
                    return processor;
                });

                if (this.config.AdaptiveSampling_Enabled == true)
                {
                    builder.UseAdaptiveSampling(this.config.AdaptiveSampling_MaxOtherItemsPerSecond ?? 5, excludedTypes: "Event");
                    builder.UseAdaptiveSampling(this.config.AdaptiveSampling_MaxEventsPerSecond ?? 5, includedTypes: "Event");
                }

                builder.Build();

                var quickPulseModule = new QuickPulseTelemetryModule() { AuthenticationApiKey = this.liveMetricsStreamAuthenticationApiKey };
                quickPulseModule.Initialize(activeConfiguration);
                quickPulseModule.RegisterTelemetryProcessor(processor);

                this.telemetryClient = this.telemetryClient ?? new TelemetryClient(activeConfiguration);
            }
            catch (Exception e)
            {
                Diagnostics.LogError(
                    FormattableString.Invariant($"Could not initialize AI SDK. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not initialize AI SDK. {e.ToString()}"), e);
            }

            try
            {
                if (this.config.Port.HasValue)
                {
                    this.istioMixerInput = new GrpcInput(this.config.Host, this.config.Port.Value, this.OnDataReceived);

                    Diagnostics.LogInfo(
                        FormattableString.Invariant($"We will listen for Istio's Mixer data on {this.config.Host}:{this.config.Port}"));
                }
                else
                {
                    Diagnostics.LogInfo(
                        FormattableString.Invariant($"We will not listen for Istio's Mixer data, configuration is insufficient."));
                }
            }
            catch (Exception e)
            {
                Diagnostics.LogError(
                    FormattableString.Invariant($"Could not create the gRPC Istio Mixer channel. {e.ToString()}"));

                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not create the gRPC Istio Mixer channel. {e.ToString()}"), e);
            }
        }

        public void Run()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Run the library, it's already running"));
            }

            try
            {
                try
                {
                    this.istioMixerInput?.Start();
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(
                        FormattableString.Invariant($"Could not start the gRPC Istio's Mixer channel. {e.ToString()}"));

                    throw new InvalidOperationException(
                        FormattableString.Invariant($"Could not start the gRPC Istio's Mixer channel. {e.ToString()}"), e);
                }
            }
            catch (Exception)
            {
                // something went wrong, so stop both inputs to ensure consistent state
                this.EmergencyShutdownAllInputs();

                throw;
            }

            this.IsRunning = true;

            Task.Run(async () => await this.TraceStatsWorker().ConfigureAwait(false));
        }

        public void Stop()
        {
            if (!this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Stop the library, it's not currently running"));
            }

            try
            {
                try
                {
                    this.istioMixerInput?.Stop();
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(FormattableString.Invariant($"Could not stop the gRPC Istio's Mixer channel. {e.ToString()}"));

                    throw new InvalidOperationException(
                        FormattableString.Invariant($"Could not stop the gRPC Istio's Mixer channel. {e.ToString()}"), e);
                }
            }
            finally
            {
                this.IsRunning = false;
            }
        }


        private void OnDataReceived(InstanceMsg instance)
        {
            try
            {
                foreach (var telemetry in this.telemetryGenerator.Generate(instance))
                {
                    if (telemetry != null)
                    {
                        this.telemetryClient.Track(telemetry);
                    }
                }
            }
            catch (System.Exception e)
            {
                // unexpected exception occured
                Diagnostics.LogError(FormattableString.Invariant($"Unknown exception while processing an instance. {e.ToString()}"));
            }
        }

        private async Task TraceStatsWorker()
        {
            while (this.IsRunning)
            {
                try
                {
                    if (this.istioMixerInput?.IsRunning == true)
                    {
                        Common.Diagnostics.LogInfo($"Istio's Mixer input: [{this.istioMixerInput.GetStats()}]");
                    }
                }
                catch (Exception e)
                {
                    Common.Diagnostics.LogInfo($"Unexpected exception in the stats thread: {e.ToString()}");
                }

                await Task.Delay(this.statsTracingTimeout).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Shuts down all inputs in case at least one of them failed.
        /// </summary>
        /// <remarks>We don't care about exceptions here, this is the best effort to clean things up.</remarks>
        private void EmergencyShutdownAllInputs()
        {
            try
            {
                this.istioMixerInput?.Stop();
            }
            catch (Exception)
            {
                // swallow any further exceptions
            }
        }
    }
}
