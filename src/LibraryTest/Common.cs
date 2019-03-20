namespace Microsoft.IstioMixerPlugin.LibraryTest
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility;
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using Google.Protobuf.Collections;
    using Istio.Policy.V1Beta1;
    using Tracespan;
    using VisualStudio.TestTools.UnitTesting;

    public static class Common
    {
        private static readonly Random rand = new Random();

        public static byte[] EncodeLengthPrefix(int length)
        {
            // little endian
            //value = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            // xx xx xx xx
            return new byte[] {(byte)(length & 0x000000ff), (byte)((length & 0x0000ff00) >> 8), (byte)((length & 0x00ff0000) >> 16), (byte)((length & 0xff000000) >> 24) };
        }

        public static void AssertIsTrueEventually(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(10);
            Assert.IsTrue(SpinWait.SpinUntil(condition, timeout.Value));
        }

        public static void AssertIsFalseEventually(Func<bool> condition, TimeSpan? timeout = null)
        {
            timeout = timeout ?? TimeSpan.FromSeconds(10);
            Assert.IsTrue(SpinWait.SpinUntil(() => !condition(), timeout.Value));
        }

        public static int GetPort()
        {
            // dynamic port range
            return rand.Next(49152, 65535);
        }

        public static TelemetryClient SetupStubTelemetryClient(out ConcurrentQueue<ITelemetry> sentItems)
        {
            var configuration = new TelemetryConfiguration();
            var queue = new ConcurrentQueue<ITelemetry>();

            var channel = new StubTelemetryChannel
            {
                OnSend = delegate (ITelemetry t)
                {
                    queue.Enqueue(t);
                }
            };

            sentItems = queue;

            configuration.TelemetryChannel = channel;

            return new TelemetryClient(configuration);
        }

        public static string SwitchLoggerToDifferentFile()
        {
            NLog.LogManager.Flush();

            string newLogFileName = $"IstioMixerPlugin_{Guid.NewGuid()}.log";
            NLog.LogManager.Configuration.FindTargetByName<NLog.Targets.FileTarget>("LogFile").FileName = newLogFileName;
            NLog.LogManager.ReconfigExistingLoggers();

            return newLogFileName;
        }

        public static InstanceMsg GetStandardInstanceMsg()
        {
            return new InstanceMsg()
            {
                SpanTags =
                {
                    {"context.reporter.uid", new Value() {StringValue = "kubernetes://destination-deployment-1"}},
                    {"context.reporter.kind", new Value() {StringValue = "inbound"}},
                    {"context.protocol", new Value() {StringValue = "http"}},

                    {"connection.event", new Value() {StringValue = ""}},

                    {"source.uid", new Value() {StringValue = "kubernetes://source-deployment-1"}},
                    {"source.workload.namespace", new Value() {StringValue = "default"}},
                    {"source.workload.name", new Value() {StringValue = "source-deployment"}},
                    {"source.labels.appinsights.monitoring.enabled", new Value() {StringValue = ""}},
                    {"source.labels.istio.isingressgateway", new Value() {BoolValue = false}},
                    {"source.role.name", new Value() {StringValue = "source"}},
                    {"source.role.instance", new Value() {StringValue = "source-1"}},
                    {"source.ip", new Value() {IpAddressValue= new IPAddress()}},

                    {"destination.uid", new Value() {StringValue = "kubernetes://destination-deployment-1"}},
                    {"destination.workload.namespace", new Value() {StringValue = "default"}},
                    {"destination.workload.name", new Value() {StringValue = "destination-deployment"}},
                    {"destination.labels.appinsights.monitoring.enabled", new Value() {StringValue = ""}},
                    {"destination.role.name", new Value() {StringValue = "destination"}},
                    {"destination.role.instance", new Value() {StringValue = "destination-1"}},
                    {"destination.port", new Value() {StringValue = "80"}},
                    {"destination.ip", new Value() {IpAddressValue= new IPAddress()}},
                    {"destination.service.host", new Value() {StringValue = ""}},

                    {"http.useragent", new Value() {StringValue = "Mozilla"}},
                    {"http.status_code", new Value() {StringValue = "203"}},
                    {"http.path", new Value() {StringValue = "/some/path"}},
                    {"http.method", new Value() {StringValue = "GET"}},

                    {"host", new Value() {StringValue = "destination-1:80"}},

                    {"request.headers.request.id", new Value() {StringValue = "request-id-1"}},
                    {"request.scheme", new Value() {StringValue = "http"}},
                    {"request.path", new Value() {StringValue = "/some/path"}},
                    {"request.size", new Value() {Int64Value= 0}},
                    {"request.headers.synthetictest.runid", new Value() {StringValue = ""}},
                    {"request.headers.synthetictest.location", new Value() {StringValue = ""}},
                    {"request.headers.request.context", new Value() {StringValue = ""}},

                    {"response.headers.request.context", new Value() {StringValue = ""}},
                    {"response.size", new Value() {Int64Value= 0}},

                    {"api.service", new Value() {StringValue= ""}},
                    {"api.protocol", new Value() {StringValue= ""}},

                }
            };
        }
    }
}
