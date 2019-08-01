namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using ApplicationInsights.DataContracts;
    using IstioMixerPlugin.Library;
    using Microsoft.IstioMixerPlugin.Common;
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Google.Protobuf.Collections;
    using Istio.Policy.V1Beta1;
    using Tracespan;
    using VisualStudio.TestTools.UnitTesting;
    using Exception = System.Exception;

    [TestClass]
    public class LibraryTests
    {
        private static readonly TimeSpan LongTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public void LibraryTests_LibraryStartsAndStopsWithCorrectConfig()
        {
            // ARRANGE
            int port = Common.GetPort();
            
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
    <Endpoints>
      <TelemetryChannelEndpoint></TelemetryChannelEndpoint>
    </Endpoints>
</Configuration>
";
            var lib = new Library(config);

            // ACT
            lib.Run();

            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void LibraryTests_LibraryThrowsOnMalformedConfig()
        {
            // ARRANGE

            // ACT
            new Library("Invalid XML here");

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidPort()
        {
            // ARRANGE
            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>NOT_A_NUMBER</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
</Configuration>
";

            // ACT
            new Library(config);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryThrowsOnInvalidHost()
        {
            // ARRANGE
            int port = Common.GetPort();
            

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>INVALID HOST NAME</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
</Configuration>
";

            // ACT
            new Library(config).Run();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStartTwice()
        {
            // ARRANGE
            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
                lib.Run();
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Run();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStopTwice()
        {
            // ARRANGE
            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
                lib.Run();
                lib.Stop();
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LibraryTests_LibraryDoesNotStopWithoutEverRunning()
        {
            // ARRANGE
            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Library lib = null;

            try
            {
                lib = new Library(config);
            }
            catch (Exception)
            {
                Assert.Fail();
            }

            // ACT
            lib.Stop();

            // ASSERT
        }

        [TestMethod]
        public void LibraryTests_LibraryTurnsOnInput()
        {
            // ARRANGE
            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <Watchlist>
        <Namespaces></Namespaces>
    </Watchlist>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            // ACT
            var lib = new Library(config);
            lib.Run();

            // ASSERT
            // check which ports are in use (listened on)
            var client = new TcpClient("localhost", port);
            Common.AssertIsTrueEventually(() => client.Connected, LongTimeout);

            client.Dispose();
        }

        [TestMethod]
        public void LibraryTests_LibraryCleansUpInputWhileStopping()
        {
            // ARRANGE
            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            var lib = new Library(config);
            lib.Run();

            var client = new TcpClient("localhost", port);
            
            Common.AssertIsTrueEventually(() => client.Connected, LongTimeout);

            // ACT
            lib.Stop();

            // ASSERT
            // check which ports are in use (listened on)
            client = null;
            
            try
            {
                client = new TcpClient("localhost", port);
            }
            catch (Exception)
            {
                // swallow
            }

            Assert.IsNull(client);
        }

        [TestMethod]
        public async Task LibraryTests_LibraryProcessesRequestsCorrectlyMoreNeeded()
        {
            Assert.Inconclusive("More regorous testing needed once requirements are defined - ingress/egress calls, complex namespace/label scenarios, etc. Model test: LibraryTests_LibraryProcessesRequestsCorrectly");
        }

        [TestMethod]
        public async Task LibraryTests_LibraryProcessesRequestsCorrectly()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>ikey1</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <Watchlist>
        <Namespaces>default</Namespaces>
    </Watchlist>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(Common.GetStandardInstanceMsg());

            var lib = new Library(config, telemetryClient);
            lib.Run();

            // ACT
            var writer = new GrpcWriter(port);
            await writer.Write(request).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => sentItems.Count == 2);

            lib.Stop();

            Assert.AreEqual("source-1", (sentItems.Skip(0).First() as DependencyTelemetry).Context.Cloud.RoleInstance);
            Assert.AreEqual("destination-1", (sentItems.Skip(1).First() as RequestTelemetry).Context.Cloud.RoleInstance);
        }

        [TestMethod]
        public async Task LibraryTests_LibraryLogsInputStatsCorrectly()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>ikey1</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <Watchlist>
        <Namespaces>default</Namespaces>
    </Watchlist>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(Common.GetStandardInstanceMsg());
            request.Instances.Add(Common.GetStandardInstanceMsg());


            // redirect loggging to a new file
            Diagnostics.Flush(TimeSpan.FromSeconds(5));
            string logFileName = Common.SwitchLoggerToDifferentFile();

            var lib = new Library(config, telemetryClient, TimeSpan.FromMilliseconds(10));
            lib.Run();

            // ACT
            var writer = new GrpcWriter(port);
            await writer.Write(request).ConfigureAwait(false);
            await writer.Write(request).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(() => sentItems.Count == 8);

            await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);

            lib.Stop();

            Diagnostics.Flush(TimeSpan.FromSeconds(5));

            // close the file
            Common.SwitchLoggerToDifferentFile();

            string logs = await File.ReadAllTextAsync(logFileName).ConfigureAwait(false);

            Assert.IsTrue(logs.Contains("|INFO|Istio's Mixer input: [ConnectionCount: 0, RequestsReceived: 0, InstancesSucceeded: 0, InstancesFailed: 0]"));
            Assert.IsTrue(logs.Contains("|INFO|Istio's Mixer input: [ConnectionCount: 0, RequestsReceived: 2, InstancesSucceeded: 4, InstancesFailed: 0]"));
        }
    }
}