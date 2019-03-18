namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using ApplicationInsights.DataContracts;
    using IstioMixerPlugin.Library;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Istio.Policy.V1Beta1;
    using Tracespan;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class HostTests
    {
        [TestMethod]
        public async Task HostTests_RunsLibrary()
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
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_NON_EVENTS_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            // ACT
            Host host = new Host(telemetryClient);
            host.Run(config, TimeSpan.FromSeconds(5));

            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(Common.GetStandardInstanceMsg());

            var writer = new GrpcWriter(port);
            await writer.Write(request).ConfigureAwait(false);
            
            Common.AssertIsTrueEventually(() => sentItems.Count == 2);

            // ASSERT
            Assert.AreEqual("source-1", (sentItems.Skip(0).First() as DependencyTelemetry).Context.Cloud.RoleInstance);
            Assert.AreEqual("destination-1", (sentItems.Skip(1).First() as RequestTelemetry).Context.Cloud.RoleInstance);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task HostTests_StopsLibrary()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

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
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_NON_EVENTS_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Host host = new Host(telemetryClient);
            host.Run(config, TimeSpan.FromSeconds(5));
            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            // ACT
            host.Stop();
            Thread.Sleep(TimeSpan.FromMilliseconds(250));

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg()
            {
                HttpStatusCode = 203,
                SpanTags = { { "source.workload.name", new Value { StringValue = "some-app" } }, { "destination.workload.name", new Value { StringValue = "some-other-app" } } }
            });

            // ASSERT
            var writer = new GrpcWriter(port);
            await writer.Write(request).ConfigureAwait(false);

            Assert.Fail();
        }

        [TestMethod]
        public async Task HostTests_RestartsLibraryIfStoppedUnexpectedly()
        {
            // ARRANGE
            var telemetryClient = Common.SetupStubTelemetryClient(out var sentItems);

            int port = Common.GetPort();

            var config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<Configuration>
    <Host>0.0.0.0</Host>
    <Port>{port}</Port>
    <InstrumentationKey>[SPECIFY INSTRUMENTATION KEY HERE]</InstrumentationKey>
    <LiveMetricsStreamAuthenticationApiKey></LiveMetricsStreamAuthenticationApiKey>
    <Watchlist>
        <Namespaces>default</Namespaces>
    </Watchlist>
    <AdaptiveSampling Enabled=""true"">
      <MaxEventsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_EVENTS_LIMIT%</MaxEventsPerSecond>
      <!--Telemetry items other than events are counted together-->
      <MaxOtherItemsPerSecond>%ISTIO_MIXER_PLUGIN_AI_ADAPTIVE_SAMPLING_NON_EVENTS_LIMIT%</MaxOtherItemsPerSecond>
    </AdaptiveSampling>
</Configuration>
";

            Host host = new Host(telemetryClient);
            host.Run(config, TimeSpan.FromSeconds(1));

            FieldInfo libraryFieldInfo = host.GetType().GetField("library", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            Common.AssertIsTrueEventually(() => libraryFieldInfo.GetValue(host) != null);

            // ACT
            // stop the existing library (as if something went wrong)
            var library = libraryFieldInfo.GetValue(host) as Library;
            library.Stop();

            Common.AssertIsTrueEventually(() => !library.IsRunning);

            // ASSERT
            // wait for a new library
            Common.AssertIsTrueEventually(() => libraryFieldInfo.GetValue(host) != null);
            Common.AssertIsTrueEventually(() => (libraryFieldInfo.GetValue(host) as Library).IsRunning);

            // verify the new library works
            var request = new HandleTraceSpanRequest();
            request.Instances.Add(Common.GetStandardInstanceMsg());

            var writer = new GrpcWriter(port);
            await writer.Write(request).ConfigureAwait(false);

            Common.AssertIsTrueEventually(() => sentItems.Count == 2);

            Assert.AreEqual("source-1", (sentItems.Skip(0).First() as DependencyTelemetry).Context.Cloud.RoleInstance);
            Assert.AreEqual("destination-1", (sentItems.Skip(1).First() as RequestTelemetry).Context.Cloud.RoleInstance);
        }
    }
}