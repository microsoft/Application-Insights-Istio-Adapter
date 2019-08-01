namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using IstioMixerPlugin.Library;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ConfigurationTests
    {
        [TestMethod]
        public void ConfigurationTests_DefaultConfigurationIsCorrect()
        {
            // ARRANGE
            string defaultConfig;
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Microsoft.IstioMixerPlugin.LibraryTest.IstioMixerPlugin.config";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    defaultConfig = reader.ReadToEnd();
                }
            }

            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT", "25", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES_IGNORED", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_TELEMETRY_CHANNEL_ENDPOINT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_QUICKPULSE_SERVICE_ENDPOINT", null, EnvironmentVariableTarget.Process);

            // ACT
            var config = new Configuration(defaultConfig);

            // ASSERT
            Assert.AreEqual("0.0.0.0", config.Host);
            Assert.AreEqual(6789, config.Port);

            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY%", config.InstrumentationKey);
            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_AI_LIVE_METRICS_STREAM_AUTH_KEY%", config.LiveMetricsStreamAuthenticationApiKey);
            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES%", config.Watchlist_Namespaces.Single());
            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES_IGNORED%", config.Watchlist_IgnoredNamespaces.Single());
            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_TELEMETRY_CHANNEL_ENDPOINT%", config.Endpoints_TelemetryChannelEndpoint);
            Assert.AreEqual("%ISTIO_MIXER_PLUGIN_QUICKPULSE_SERVICE_ENDPOINT%", config.Endpoints_QuickPulseServiceEndpoint);

            Assert.AreEqual(true, config.AdaptiveSampling_Enabled);
            Assert.AreEqual(10, config.AdaptiveSampling_MaxEventsPerSecond);
            Assert.AreEqual(25, config.AdaptiveSampling_MaxOtherItemsPerSecond);

            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_AI_INSTRUMENTATIONKEY", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_WATCHLIST_NAMESPACES_IGNORED", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_TELEMETRY_CHANNEL_ENDPOINT", null, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("ISTIO_MIXER_PLUGIN_QUICKPULSE_SERVICE_ENDPOINT", null, EnvironmentVariableTarget.Process);
        }

        [TestMethod]
        public void ConfigurationTests_EnvironmentVariablesAreResolved()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>%Input_Host%</Host>
    <Port>%Input_Port%</Port>
    <InstrumentationKey>%ConfigTestInstrumentationKey%</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
    <Endpoints>
        <TelemetryChannelEndpoint>%TelemetryChannelEndpoint%</TelemetryChannelEndpoint>
        <QuickPulseServiceEndpoint>%QuickPulseEndpoint%</QuickPulseServiceEndpoint>
    </Endpoints>
</Configuration>
";

            var rand = new Random();
            string host = Guid.NewGuid().ToString();
            string port = rand.Next().ToString();
            string ikey = Guid.NewGuid().ToString();
            string telemetryChannelEndpoint = Guid.NewGuid().ToString();
            string quickPulseEndpoint = Guid.NewGuid().ToString();

            Environment.SetEnvironmentVariable("Input_Host", host);
            Environment.SetEnvironmentVariable("Input_Port", port);
            Environment.SetEnvironmentVariable("ConfigTestInstrumentationKey", ikey);
            Environment.SetEnvironmentVariable("TelemetryChannelEndpoint", telemetryChannelEndpoint);
            Environment.SetEnvironmentVariable("QuickPulseEndpoint", quickPulseEndpoint);

            // ACT
            var configuration = new Configuration(config);

            // ASSERT
            Assert.AreEqual(host, configuration.Host);
            Assert.AreEqual(port, configuration.Port.ToString());
            Assert.AreEqual(ikey, configuration.InstrumentationKey);
            Assert.AreEqual(telemetryChannelEndpoint, configuration.Endpoints_TelemetryChannelEndpoint);
            Assert.AreEqual(quickPulseEndpoint, configuration.Endpoints_QuickPulseServiceEndpoint);
        }

        [TestMethod]
        public void ConfigurationTests_EnvironmentVariablesAreNotResolvedIfNonExistent()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>%Input_Host%</Host>
    <Port>0</Port>
    <InstrumentationKey>%ConfigTestInstrumentationKey%</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Environment.SetEnvironmentVariable("Input_Host", null);
            Environment.SetEnvironmentVariable("ConfigTestInstrumentationKey", null);
            
            // ACT
            var configuration = new Configuration(config);

            // ASSERT
            Assert.AreEqual("%Input_Host%", configuration.Host);
            Assert.AreEqual("%ConfigTestInstrumentationKey%", configuration.InstrumentationKey);
        }

        [TestMethod]
        public void ConfigurationTests_WatchlistNamespacesAreParsedCorrectly()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>%Input_Host%</Host>
    <Port>%Input_Port%</Port>
    <InstrumentationKey>%ConfigTestInstrumentationKey%</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <Watchlist>
        <Namespaces>default,some-other-namespace, yet-another-namespace </Namespaces>
    </Watchlist>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";
            // ACT
            var configuration = new Configuration(config);

            // ASSERT
            Assert.AreEqual(3, configuration.Watchlist_Namespaces.Length);
            Assert.AreEqual("default", configuration.Watchlist_Namespaces[0]);
            Assert.AreEqual("some-other-namespace", configuration.Watchlist_Namespaces[1]);
            Assert.AreEqual("yet-another-namespace", configuration.Watchlist_Namespaces[2]);
        }
    }
}