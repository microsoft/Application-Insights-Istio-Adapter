namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using ApplicationInsights;
    using ApplicationInsights.Channel;
    using ApplicationInsights.DataContracts;
    using ApplicationInsights.Extensibility.Implementation;
    using Google.Protobuf;
    using Google.Protobuf.WellKnownTypes;
    using Istio.Policy.V1Beta1;
    using IstioMixerPlugin.Library;
    using Tracespan;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class TelemetryGeneratorTests
    {
        [TestMethod]
        public void TelemetryGeneratorTests_HandlesTargetNamespacesCorrectly()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"ns1", "ns2"}, new string[]{});

            // ACT
            var instance0 = Common.GetStandardInstanceMsg();
            instance0.SpanTags["source.workload.namespace"].StringValue = "unknown";
            instance0.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance00 = Common.GetStandardInstanceMsg();
            instance00.SpanTags["source.workload.namespace"].StringValue = "";
            instance00.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "ns20";

            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "ns10";
            instance2.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance2.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "ns2";

            var instance5 = Common.GetStandardInstanceMsg();
            instance5.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance5.SpanTags["destination.workload.namespace"].StringValue = "unknown";
            instance5.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance5.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance55 = Common.GetStandardInstanceMsg();
            instance55.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance55.SpanTags["destination.workload.namespace"].StringValue = "";
            instance55.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance55.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var telemetry = telemetryGenerator.Generate(instance0, instance00, instance1, instance2, instance3, instance4, instance5, instance55).ToList();

            // ASSERT
            Assert.AreEqual(1 + 1 + 0 + 1 + 1 + 2 + 1 + 1, telemetry.Count);

            Assert.IsTrue(telemetry[0] is RequestTelemetry);

            Assert.IsTrue(telemetry[1] is RequestTelemetry);

            Assert.IsTrue(telemetry[2] is DependencyTelemetry);

            Assert.IsTrue(telemetry[3] is RequestTelemetry);

            Assert.IsTrue(telemetry[4] is DependencyTelemetry);
            Assert.IsTrue(telemetry[5] is RequestTelemetry);

            Assert.IsTrue(telemetry[6] is DependencyTelemetry);

            Assert.IsTrue(telemetry[7] is DependencyTelemetry);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_HandlesEmptyTargetNamespacesCorrectly()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new string[] { }, new string[] { });

            // ACT
            var instance0 = Common.GetStandardInstanceMsg();
            instance0.SpanTags["source.workload.namespace"].StringValue = "unknown";
            instance0.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance00 = Common.GetStandardInstanceMsg();
            instance00.SpanTags["source.workload.namespace"].StringValue = "";
            instance00.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "ns20";

            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "ns10";
            instance2.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance2.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "ns2";

            var instance5 = Common.GetStandardInstanceMsg();
            instance5.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance5.SpanTags["destination.workload.namespace"].StringValue = "unknown";
            instance5.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance5.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance55 = Common.GetStandardInstanceMsg();
            instance55.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance55.SpanTags["destination.workload.namespace"].StringValue = "";
            instance55.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-deployment-1";
            instance55.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var telemetry = telemetryGenerator.Generate(instance0, instance00, instance1, instance2, instance3, instance4, instance5, instance55).ToList();

            // ASSERT
            Assert.AreEqual(1 + 1 + 2 + 0 + 2 + 2 + 1 + 1, telemetry.Count);

            Assert.IsTrue(telemetry[0] is RequestTelemetry);

            Assert.IsTrue(telemetry[1] is RequestTelemetry);

            Assert.IsTrue(telemetry[2] is DependencyTelemetry);
            Assert.IsTrue(telemetry[3] is RequestTelemetry);
            
            Assert.IsTrue(telemetry[4] is DependencyTelemetry);
            Assert.IsTrue(telemetry[5] is RequestTelemetry);

            Assert.IsTrue(telemetry[6] is DependencyTelemetry);
            Assert.IsTrue(telemetry[7] is RequestTelemetry);

            Assert.IsTrue(telemetry[8] is DependencyTelemetry);

            Assert.IsTrue(telemetry[9] is DependencyTelemetry);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_HandlesIgnoredNamespacesCorrectly()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new[] { "ns1", "ns2" }, new [] { "ns1" });

            // ACT
            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "ns20";

            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "ns10";
            instance2.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "ns1";

            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "ns2";
            instance4.SpanTags["destination.role.instance"].StringValue = "cool instance";

            var telemetry = telemetryGenerator.Generate(instance1, instance2, instance3, instance4).ToList();

            // ASSERT
            Assert.AreEqual(0 + 0 + 0 + 1, telemetry.Count);

            Assert.IsTrue(telemetry[0] is RequestTelemetry);
            Assert.AreEqual("cool instance", (telemetry[0] as RequestTelemetry).Context.Cloud.RoleInstance);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_HandlesPodLevelConfigurationCorrectly()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new[] { "ns1", "ns2" }, new string[] { "ns10", "ns20" });

            // ACT
            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance1.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "true";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "ns20";
            instance1.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "true";

            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance2.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "false";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "ns10";
            instance2.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "true";
            
            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["source.workload.namespace"].StringValue = "ns10";
            instance3.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "true";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "ns1";
            instance3.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "false";
            instance3.SpanTags["context.reporter.kind"].StringValue = "outbound";

            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["source.workload.namespace"].StringValue = "ns1";
            instance4.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "false";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "ns2";
            instance4.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "false";

            var telemetry = telemetryGenerator.Generate(instance1, instance2, instance3, instance4).ToList();

            // ASSERT
            Assert.AreEqual(2 + 1 + 1 + 0, telemetry.Count);

            Assert.IsTrue(telemetry[0] is DependencyTelemetry);
            Assert.IsTrue(telemetry[1] is RequestTelemetry);

            Assert.IsTrue(telemetry[2] is RequestTelemetry);

            Assert.IsTrue(telemetry[3] is DependencyTelemetry);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_PutsReponseHeaderRequestContextIntoDependencyTarget()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"default"}, new string[0]);

            // ACT
            var instanceMsg = Common.GetStandardInstanceMsg();
            instanceMsg.SpanTags["response.headers.request.context"].StringValue = "appId=cid-v1:dff954f7-ed4b-4ac6-9987-ed659ba2bd47";

            DependencyTelemetry dependency = telemetryGenerator.Generate(instanceMsg).OfType<DependencyTelemetry>().Single();
            
            // ASSERT
            Assert.AreEqual("destination-1 | cid-v1:dff954f7-ed4b-4ac6-9987-ed659ba2bd47", dependency.Target);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_RequestIdIsCorrectlyPropagatedWhenPassedFromOutsideThroughIngressGateway()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"default"}, new string[]{});

            // incoming request, reported by outbound proxy of the gateway
            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["context.reporter.uid"].StringValue = "kubernetes://istio-ingressgateway";
            instance1.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance1.SpanTags["source.uid"].StringValue = "kubernetes://istio-ingressgateway.istio-system";
            instance1.SpanTags["source.workload.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance1.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance1.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance1.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance1.SpanTags["destination.uid"].StringValue = "kubernetes://istio-ingressgateway.istio-system";
            instance1.SpanTags["destination.workload.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance1.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance1.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance1.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance1.SpanTags["request.scheme"].StringValue = "http";
            instance1.SpanTags["request.path"].StringValue = "/some/path";
            instance1.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance1.SpanTags["host"].StringValue = "destination-deployment-1";
            instance1.SpanTags["http.status_code"].StringValue = "200";
            instance1.SpanTags["http.path"].StringValue = "/some/path";
            instance1.SpanTags["http.method"].StringValue = "GET";
            instance1.SpanTags["destination.port"].StringValue = "8888";
            instance1.SpanTags["request.headers.request.id"].StringValue = "|original-guid.";
            instance1.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance1.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance1.SpanTags["request.headers.request.context"].StringValue = "";
            instance1.SpanTags["response.headers.request.context"].StringValue = "";

            // same incoming request, reported by inbound proxy of the destination-deployment-1 pod
            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["context.reporter.uid"].StringValue = "kubernetes://destination-deployment-1";
            instance2.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance2.SpanTags["source.uid"].StringValue = "kubernetes://istio-ingressgateway.istio-system";
            instance2.SpanTags["source.workload.name"].StringValue = "istio-ingressgateway";
            instance2.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance2.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance2.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance2.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance2.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance2.SpanTags["destination.uid"].StringValue = "kubernetes://destination-deployment-1.default";
            instance2.SpanTags["destination.workload.name"].StringValue = "destination-deployment";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance2.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance2.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance2.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance2.SpanTags["request.scheme"].StringValue = "http";
            instance2.SpanTags["request.path"].StringValue = "/some/path";
            instance2.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance2.SpanTags["host"].StringValue = "destination-deployment-1";
            instance2.SpanTags["http.status_code"].StringValue = "200";
            instance2.SpanTags["http.path"].StringValue = "/some/path";
            instance2.SpanTags["http.method"].StringValue = "GET";
            instance2.SpanTags["destination.port"].StringValue = "8888";
            instance2.SpanTags["request.headers.request.id"].StringValue = "|original-guid.";
            instance2.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance2.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance2.SpanTags["request.headers.request.context"].StringValue = "";
            instance2.SpanTags["response.headers.request.context"].StringValue = "";

            // request destination-deployment-1 --> another-destination-deployment-1, reported by outbound proxy of the destination-deployment-1 pod
            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["context.reporter.uid"].StringValue = "kubernetes://destination-deployment-1";
            instance3.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance3.SpanTags["source.uid"].StringValue = "kubernetes://destination-deployment-1.default";
            instance3.SpanTags["source.workload.name"].StringValue = "destination-deployment";
            instance3.SpanTags["source.workload.namespace"].StringValue = "default";
            instance3.SpanTags["source.name"].StringValue = "destination-deployment-1";
            instance3.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance3.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance3.SpanTags["source.role.name"].StringValue = "destination-deployment";
            instance3.SpanTags["source.role.instance"].StringValue = "destination-deployment-1";
            instance3.SpanTags["destination.uid"].StringValue = "kubernetes://another-destination-deployment-1.default";
            instance3.SpanTags["destination.workload.name"].StringValue = "another-destination-deployment";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance3.SpanTags["destination.name"].StringValue = "another-destination-deployment-1";
            instance3.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance3.SpanTags["destination.role.name"].StringValue = "another-destination-deployment";
            instance3.SpanTags["destination.role.instance"].StringValue = "another-destination-deployment-1";
            instance3.SpanTags["request.scheme"].StringValue = "http";
            instance3.SpanTags["request.path"].StringValue = "/some/path";
            instance3.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance3.SpanTags["host"].StringValue = "destination-deployment-1";
            instance3.SpanTags["http.status_code"].StringValue = "200";
            instance3.SpanTags["http.path"].StringValue = "/some/path";
            instance3.SpanTags["http.method"].StringValue = "GET";
            instance3.SpanTags["destination.port"].StringValue = "8888";
            instance3.SpanTags["request.headers.request.id"].StringValue = "";
            instance3.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance3.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance3.SpanTags["request.headers.request.context"].StringValue = "";
            instance3.SpanTags["response.headers.request.context"].StringValue = "";

            // request destination-deployment-1 --> another-destination-deployment-1, reported by inbound proxy of the another-destination-deployment-1 pod
            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["context.reporter.uid"].StringValue = "kubernetes://another-destination-deployment-1";
            instance4.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance4.SpanTags["source.uid"].StringValue = "kubernetes://destination-deployment-1.default";
            instance4.SpanTags["source.workload.name"].StringValue = "destination-deployment";
            instance4.SpanTags["source.workload.namespace"].StringValue = "default";
            instance4.SpanTags["source.name"].StringValue = "destination-deployment-1";
            instance4.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance4.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance4.SpanTags["source.role.name"].StringValue = "destination-deployment";
            instance4.SpanTags["source.role.instance"].StringValue = "destination-deployment-1";
            instance4.SpanTags["destination.uid"].StringValue = "kubernetes://another-destination-deployment-1.default";
            instance4.SpanTags["destination.workload.name"].StringValue = "another-destination-deployment";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance4.SpanTags["destination.name"].StringValue = "another-destination-deployment-1";
            instance4.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance4.SpanTags["destination.role.name"].StringValue = "another-destination-deployment";
            instance4.SpanTags["destination.role.instance"].StringValue = "another-destination-deployment-1";
            instance4.SpanTags["request.scheme"].StringValue = "http";
            instance4.SpanTags["request.path"].StringValue = "/some/path";
            instance4.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance4.SpanTags["host"].StringValue = "destination-deployment-1";
            instance4.SpanTags["http.status_code"].StringValue = "200";
            instance4.SpanTags["http.path"].StringValue = "/some/path";
            instance4.SpanTags["http.method"].StringValue = "GET";
            instance4.SpanTags["destination.port"].StringValue = "8888";
            instance4.SpanTags["request.headers.request.id"].StringValue = "";
            instance4.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance4.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance4.SpanTags["request.headers.request.context"].StringValue = "";
            instance4.SpanTags["response.headers.request.context"].StringValue = "";

            // ACT
            ITelemetry[] telemetryItems = telemetryGenerator.Generate(instance1, instance2, instance3, instance4).ToArray();

            // ASSERT
            // 2 items for gateway, 2 items for destination-deployment, 1 items for destination-deployment
            Assert.AreEqual(5, telemetryItems.Length);

            var gatewayRequest = telemetryItems[0] as RequestTelemetry;
            var gatewayDependency = telemetryItems[1] as DependencyTelemetry;
            var destinationRequest = telemetryItems[2] as RequestTelemetry;
            var destinationDependency = telemetryItems[3] as DependencyTelemetry;
            var anotherDestinationRequest = telemetryItems[4] as RequestTelemetry;

            Assert.IsNotNull(gatewayRequest);
            Assert.IsNotNull(gatewayDependency);
            Assert.IsNotNull(destinationDependency);
            Assert.IsNotNull(destinationRequest);
            Assert.IsNotNull(anotherDestinationRequest);

            ValidateTelemetrySource(gatewayRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(gatewayDependency, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(destinationRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(destinationDependency, "destination-deployment-1", "destination-deployment", "default", "another-destination-deployment-1", "another-destination-deployment", "default", "kubernetes://another-destination-deployment-1", "inbound");
            ValidateTelemetrySource(anotherDestinationRequest, "destination-deployment-1", "destination-deployment", "default", "another-destination-deployment-1", "another-destination-deployment", "default", "kubernetes://another-destination-deployment-1", "inbound");

            ValidateTelemetryOperationData(gatewayRequest,               @"^\|original-guid\.[A-Za-z0-9]{8}_$",                                                              @"^\|original-guid\.$",                                                       @"^original-guid$");
            ValidateTelemetryOperationData(gatewayDependency,            @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$",                                             @"^\|original-guid\.[A-Za-z0-9]{8}_$",                                       @"^original-guid$");
            ValidateTelemetryOperationData(destinationRequest,           @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_$",                             @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$",                      @"^original-guid$");
            // destination-deployment-1 does not propagate Request-Id header, so the original trace is lost and regenerated here
            ValidateTelemetryOperationData(destinationDependency,        @"^\|[A-Za-z0-9]{32}\.[A-Za-z0-9]{8}.$",                                                            @"^\|[A-Za-z0-9]{32}\.$",                                                     @"^[A-Za-z0-9]{32}$");
            ValidateTelemetryOperationData(anotherDestinationRequest,    @"^\|[A-Za-z0-9]{32}\.[A-Za-z0-9]{8}.[A-Za-z0-9]{8}_$",                                            @"^\|[A-Za-z0-9]{32}\.[A-Za-z0-9]{8}.$",                                     @"^[A-Za-z0-9]{32}$");

            Assert.AreEqual(gatewayRequest.Id, gatewayDependency.Context.Operation.ParentId);
            Assert.AreEqual(gatewayDependency.Id, destinationRequest.Context.Operation.ParentId);
            // headers are not propogated, so the trace context is lost here
            Assert.AreNotEqual(destinationRequest.Id, destinationDependency.Context.Operation.ParentId);
            Assert.AreEqual(destinationDependency.Id, anotherDestinationRequest.Context.Operation.ParentId);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_RequestIdIsCorrectlyPropagatedWhenApplicationsSupportPropagation()
        {
            //!!!
            Assert.Inconclusive("TODO");

            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"default"}, new string[]{});

            // incoming request, reported by outbound proxy of the gateway
            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["context.reporter.uid"].StringValue = "kubernetes://istio-ingressgateway";
            instance1.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance1.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance1.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance1.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance1.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance1.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance1.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance1.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance1.SpanTags["request.scheme"].StringValue = "http";
            instance1.SpanTags["request.path"].StringValue = "/some/path";
            instance1.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance1.SpanTags["host"].StringValue = "destination-deployment-1";
            instance1.SpanTags["http.status_code"].StringValue = "200";
            instance1.SpanTags["http.path"].StringValue = "/some/path";
            instance1.SpanTags["http.method"].StringValue = "GET";
            instance1.SpanTags["destination.port"].StringValue = "8888";
            instance1.SpanTags["request.headers.request.id"].StringValue = "|original-guid.";
            instance1.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance1.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance1.SpanTags["request.headers.request.context"].StringValue = "";
            instance1.SpanTags["response.headers.request.context"].StringValue = "";

            // same incoming request, reported by inbound proxy of the destination-deployment-1 pod
            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["context.reporter.uid"].StringValue = "kubernetes://destination-deployment-1";
            instance2.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance2.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance2.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance2.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance2.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance2.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance2.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance2.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance2.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance2.SpanTags["request.scheme"].StringValue = "http";
            instance2.SpanTags["request.path"].StringValue = "/some/path";
            instance2.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance2.SpanTags["host"].StringValue = "destination-deployment-1";
            instance2.SpanTags["http.status_code"].StringValue = "200";
            instance2.SpanTags["http.path"].StringValue = "/some/path";
            instance2.SpanTags["http.method"].StringValue = "GET";
            instance2.SpanTags["destination.port"].StringValue = "8888";
            instance2.SpanTags["request.headers.request.id"].StringValue = "|original-guid.";
            instance2.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance2.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance2.SpanTags["request.headers.request.context"].StringValue = "";
            instance2.SpanTags["response.headers.request.context"].StringValue = "";

            // request destination-deployment-1 --> another-destination-deployment-1, reported by outbound proxy of the destination-deployment-1 pod
            var instance3 = Common.GetStandardInstanceMsg();
            instance3.SpanTags["context.reporter.uid"].StringValue = "kubernetes://destination-deployment-1";
            instance3.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance3.SpanTags["source.workload.namespace"].StringValue = "default";
            instance3.SpanTags["source.name"].StringValue = "destination-deployment-1";
            instance3.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance3.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance3.SpanTags["source.role.name"].StringValue = "destination-deployment";
            instance3.SpanTags["source.role.instance"].StringValue = "destination-deployment-1";
            instance3.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance3.SpanTags["destination.name"].StringValue = "another-destination-deployment";
            instance3.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance3.SpanTags["destination.role.name"].StringValue = "another-destination-deployment-1";
            instance3.SpanTags["destination.role.instance"].StringValue = "another-destination-deployment-1";
            instance3.SpanTags["request.scheme"].StringValue = "http";
            instance3.SpanTags["request.path"].StringValue = "/some/path";
            instance3.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance3.SpanTags["host"].StringValue = "destination-deployment-1";
            instance3.SpanTags["http.status_code"].StringValue = "200";
            instance3.SpanTags["http.path"].StringValue = "/some/path";
            instance3.SpanTags["http.method"].StringValue = "GET";
            instance3.SpanTags["destination.port"].StringValue = "8888";
            instance3.SpanTags["request.headers.request.id"].StringValue = "|original-guid.1.";  // Request-Id header is propagated by the app
            instance3.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance3.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance3.SpanTags["request.headers.request.context"].StringValue = "";
            instance3.SpanTags["response.headers.request.context"].StringValue = "";

            // request destination-deployment-1 --> another-destination-deployment-1, reported by inbound proxy of the another-destination-deployment-1 pod
            var instance4 = Common.GetStandardInstanceMsg();
            instance4.SpanTags["context.reporter.uid"].StringValue = "kubernetes://another-destination-deployment-1";
            instance4.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance4.SpanTags["source.workload.namespace"].StringValue = "default";
            instance4.SpanTags["source.name"].StringValue = "destination-deployment-1";
            instance4.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance4.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance4.SpanTags["source.role.name"].StringValue = "destination-deployment";
            instance4.SpanTags["source.role.instance"].StringValue = "destination-deployment-1";
            instance4.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance4.SpanTags["destination.name"].StringValue = "another-destination-deployment-1";
            instance4.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance4.SpanTags["destination.role.name"].StringValue = "another-destination-deployment";
            instance4.SpanTags["destination.role.instance"].StringValue = "another-destination-deployment-1";
            instance4.SpanTags["request.scheme"].StringValue = "http";
            instance4.SpanTags["request.path"].StringValue = "/some/path";
            instance4.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance4.SpanTags["host"].StringValue = "destination-deployment-1";
            instance4.SpanTags["http.status_code"].StringValue = "200";
            instance4.SpanTags["http.path"].StringValue = "/some/path";
            instance4.SpanTags["http.method"].StringValue = "GET";
            instance4.SpanTags["destination.port"].StringValue = "8888";
            instance4.SpanTags["request.headers.request.id"].StringValue = "|original-guid.1.";  // Request-Id header is propagated by the app
            instance4.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance4.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance4.SpanTags["request.headers.request.context"].StringValue = "";
            instance4.SpanTags["response.headers.request.context"].StringValue = "";

            // request another-destination-deployment-1 --> yet-another-destination-deployment-1, reported by outbound proxy of the another-destination-deployment-1 pod
            var instance5 = Common.GetStandardInstanceMsg();
            instance5.SpanTags["context.reporter.uid"].StringValue = "kubernetes://another-destination-deployment-1";
            instance5.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance5.SpanTags["source.workload.namespace"].StringValue = "default";
            instance5.SpanTags["source.name"].StringValue = "another-destination-deployment";
            instance5.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance5.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance5.SpanTags["source.role.name"].StringValue = "another-destination-deployment";
            instance5.SpanTags["source.role.instance"].StringValue = "another-destination-deployment-1";
            instance5.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance5.SpanTags["destination.name"].StringValue = "yet-another-destination-deployment-1";
            instance5.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance5.SpanTags["destination.role.name"].StringValue = "yet-another-destination-deployment";
            instance5.SpanTags["destination.role.instance"].StringValue = "yet-another-destination-deployment-1";
            instance5.SpanTags["request.scheme"].StringValue = "http";
            instance5.SpanTags["request.path"].StringValue = "/some/path";
            instance5.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance5.SpanTags["host"].StringValue = "yet-destination-deployment-1";
            instance5.SpanTags["http.status_code"].StringValue = "200";
            instance5.SpanTags["http.path"].StringValue = "/some/path";
            instance5.SpanTags["http.method"].StringValue = "GET";
            instance5.SpanTags["destination.port"].StringValue = "8888";
            instance5.SpanTags["request.headers.request.id"].StringValue = "|original-guid.1.dep1kj4_";  // Request-Id header is propagated by the app
            instance5.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance5.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance5.SpanTags["request.headers.request.context"].StringValue = "";
            instance5.SpanTags["response.headers.request.context"].StringValue = "";

            // request another-destination-deployment-1 --> yet-another-destination-deployment-1, reported by inbound proxy of the yet-another-destination-deployment-1 pod
            var instance6 = Common.GetStandardInstanceMsg();
            instance6.SpanTags["context.reporter.uid"].StringValue = "kubernetes://yet-another-destination-deployment-1";
            instance6.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance6.SpanTags["source.workload.namespace"].StringValue = "default";
            instance6.SpanTags["source.name"].StringValue = "another-destination-deployment-1";
            instance6.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance6.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance6.SpanTags["source.role.name"].StringValue = "another-destination-deployment";
            instance6.SpanTags["source.role.instance"].StringValue = "another-destination-deployment-1";
            instance6.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance6.SpanTags["destination.name"].StringValue = "yet-another-destination-deployment-1";
            instance6.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance6.SpanTags["destination.role.name"].StringValue = "yet-another-destination-deployment";
            instance6.SpanTags["destination.role.instance"].StringValue = "yet-another-destination-deployment-1";
            instance6.SpanTags["request.scheme"].StringValue = "http";
            instance6.SpanTags["request.path"].StringValue = "/some/path";
            instance6.SpanTags["http.useragent"].StringValue = "Mozilla";
            instance6.SpanTags["host"].StringValue = "yet-destination-deployment-1";
            instance6.SpanTags["http.status_code"].StringValue = "200";
            instance6.SpanTags["http.path"].StringValue = "/some/path";
            instance6.SpanTags["http.method"].StringValue = "GET";
            instance6.SpanTags["destination.port"].StringValue = "8888";
            instance6.SpanTags["request.headers.request.id"].StringValue = "|original-guid.1.dep1kj4_";  // Request-Id header is propagated by the app
            instance6.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance6.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance6.SpanTags["request.headers.request.context"].StringValue = "";
            instance6.SpanTags["response.headers.request.context"].StringValue = "";

            // ACT
            ITelemetry[] telemetryItems = telemetryGenerator.Generate(instance1, instance2, instance3, instance4, instance5, instance6).ToArray();

            // ASSERT
            // 2 items for gateway, 2 items for destination-deployment, 2 items for another-destination-deployment, 1 item for yet-another-destination-deployment
            Assert.AreEqual(7, telemetryItems.Length);

            var gatewayRequest = telemetryItems[0] as RequestTelemetry;
            var gatewayDependency = telemetryItems[1] as DependencyTelemetry;
            var destinationRequest = telemetryItems[2] as RequestTelemetry;
            var destinationDependency = telemetryItems[3] as DependencyTelemetry;
            var anotherDestinationRequest = telemetryItems[4] as RequestTelemetry;
            var anotherDestinationDependency = telemetryItems[5] as DependencyTelemetry;
            var yetAnotherDestinationRequest = telemetryItems[6] as RequestTelemetry;

            Assert.IsNotNull(gatewayRequest);
            Assert.IsNotNull(gatewayDependency);
            Assert.IsNotNull(destinationDependency);
            Assert.IsNotNull(destinationRequest);
            Assert.IsNotNull(anotherDestinationRequest);
            Assert.IsNotNull(anotherDestinationDependency);
            Assert.IsNotNull(yetAnotherDestinationRequest);

            ValidateTelemetrySource(gatewayRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(gatewayDependency, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(destinationRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(destinationDependency, "destination-deployment-1", "destination-deployment", "default", "another-destination-deployment-1", "another-destination-deployment", "default", "kubernetes://another-destination-deployment-1", "inbound");
            ValidateTelemetrySource(anotherDestinationRequest, "destination-deployment-1", "destination-deployment", "default", "another-destination-deployment-1", "default", "another-destination-deployment", "kubernetes://another-destination-deployment-1", "inbound");
            ValidateTelemetrySource(anotherDestinationDependency, "another-destination-deployment-1", "another-destination-deployment", "default", "yet-another-destination-deployment-1", "yet-another-destination-deployment", "default", "kubernetes://yet-another-destination-deployment-1", "inbound");
            ValidateTelemetrySource(yetAnotherDestinationRequest, "another-destination-deployment-1", "another-destination-deployment", "default", "yet-another-destination-deployment-1", "yet-another-destination-deployment", "default", "kubernetes://yet-another-destination-deployment-1", "inbound");

            // Request-Id is propagated by applications, so it is expected to be maintained all the way through the trace
            ValidateTelemetryOperationData(gatewayRequest,    @"^\|original-guid\.[A-Za-z0-9]{8}_$", @"^\|original-guid\.$", @"^original-guid$");
            ValidateTelemetryOperationData(gatewayDependency, @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^\|original-guid\.[A-Za-z0-9]{8}_$", @"^original-guid$");
            ValidateTelemetryOperationData(destinationRequest, @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_$", @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^original-guid$");
            ValidateTelemetryOperationData(destinationDependency, @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_$", @"^original-guid$");
            ValidateTelemetryOperationData(anotherDestinationRequest, @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_$", @"^\|original-guid\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^original-guid$");

            Assert.AreEqual(gatewayRequest.Id, gatewayDependency.Context.Operation.ParentId);
            Assert.AreEqual(gatewayDependency.Id, destinationRequest.Context.Operation.ParentId);
            // headers are not propogated, so the trace context is lost here
            Assert.AreNotEqual(destinationRequest.Id, destinationDependency.Context.Operation.ParentId);
            Assert.AreEqual(destinationDependency.Id, anotherDestinationRequest.Context.Operation.ParentId);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_TcpExternalDependencyIsHandledCorrectly()
        {
            //!!!
            Assert.Inconclusive("TODO: TCP support");

            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new[]{"default"}, new string[]{});

            // incoming request, reported by outbound proxy of the gateway
            var now = DateTimeOffset.UtcNow;
            var instance1 = Common.GetStandardInstanceMsg();

            instance1.SpanName = " ://";
            instance1.StartTime = new TimeStamp(){ Value = Timestamp.FromDateTimeOffset(now)};
            instance1.EndTime = new TimeStamp() { Value = Timestamp.FromDateTimeOffset(now) };
            instance1.Name = "i1.instance.istio-system";

            instance1.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance1.SpanTags["context.reporter.uid"].StringValue = "kubernetes://source-1.default";
            instance1.SpanTags["context.protocol"].StringValue = "TCP";

            instance1.SpanTags["connection.event"].StringValue = "OPEN";

            instance1.SpanTags["source.name"].StringValue = "source-1";
            instance1.SpanTags["source.ip"].IpAddressValue = new IPAddress(){Value = ByteString.FromBase64("CvQSCA==")};
            instance1.SpanTags["source.labels.istio.isingressgateway"].BoolValue = false;
            instance1.SpanTags["source.role.instance"].StringValue = "kubernetes://source-1.default";
            instance1.SpanTags["source.workload.namespace"].StringValue = "default";
            instance1.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["source.role.name"].StringValue = "source.default";

            instance1.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["destination.role.instance"].StringValue = "unknown";
            instance1.SpanTags["destination.role.name"].StringValue = "unknown";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "";
            instance1.SpanTags["destination.name"].StringValue = "";
            instance1.SpanTags["destination.ip"].IpAddressValue = new IPAddress() { Value = ByteString.FromBase64("DVuGLQ==") };
            instance1.SpanTags["destination.port"].Int64Value = 6379;

            instance1.SpanTags["host"].StringValue = "destination.redis.cache.windows.net";

            instance1.SpanTags["request.headers.request.context"].StringValue = "";
            instance1.SpanTags["response.size"].Int64Value = 0;
            instance1.SpanTags["http.useragent"].StringValue = "";
            instance1.SpanTags["request.headers.synthetictest.location"].StringValue = "";
            instance1.SpanTags["request.path"].StringValue = "";
            instance1.SpanTags["request.size"].Int64Value = 0;
            instance1.SpanTags["http.method"].StringValue = "";
            instance1.SpanTags["http.path"].StringValue = "";
            instance1.SpanTags["http.status_code"].Int64Value = 0;
            instance1.SpanTags["request.headers.request.id"].StringValue = "";
            instance1.SpanTags["request.scheme"].StringValue = "";
            instance1.SpanTags["request.headers.synthetictest.runid"].StringValue = "";
            instance1.SpanTags["response.headers.request.context"].StringValue = "";
            
            // ACT
            ITelemetry[] telemetryItems = telemetryGenerator.Generate(instance1).ToArray();

            // ASSERT
            Assert.AreEqual(1, telemetryItems.Length);

            var tcpDependency = telemetryItems[0] as DependencyTelemetry;
            
            Assert.IsNotNull(tcpDependency);
            
            ValidateTelemetrySource(tcpDependency, "source-1.default", "source", "default", "", "", "", "kubernetes://source-1.default", "outbound");
            
            Assert.AreEqual("source-1.default", tcpDependency.Context.Cloud.RoleInstance);
            Assert.AreEqual("source.default", tcpDependency.Context.Cloud.RoleName);
            Assert.AreEqual("destination.redis.cache.windows.net", tcpDependency.Target);
            Assert.AreEqual("tcp://destination.redis.cache.windows.net:6379", tcpDependency.Data);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_AvailabilityProbeIsHandledCorrectlyThroughIngressGateway()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"default"}, new string[]{});

            // incoming request, reported by outbound proxy of the gateway
            var instance1 = Common.GetStandardInstanceMsg();
            instance1.SpanTags["context.reporter.uid"].StringValue = "kubernetes://istio-ingressgateway";
            instance1.SpanTags["context.reporter.kind"].StringValue = "outbound";
            instance1.SpanTags["source.uid"].StringValue = "kubernetes://istio-ingressgateway.istio-system";
            instance1.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance1.SpanTags["source.workload.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance1.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance1.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance1.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance1.SpanTags["destination.uid"].StringValue = "kubernetes://destination-deployment-1.default";
            instance1.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance1.SpanTags["destination.workload.name"].StringValue = "destination-deployment";
            instance1.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance1.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance1.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance1.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance1.SpanTags["request.scheme"].StringValue = "http";
            instance1.SpanTags["request.path"].StringValue = "/some/path";
            instance1.SpanTags["http.useragent"].StringValue = "GSM";
            instance1.SpanTags["host"].StringValue = "destination-deployment-1";
            instance1.SpanTags["http.status_code"].StringValue = "200";
            instance1.SpanTags["http.path"].StringValue = "/some/path";
            instance1.SpanTags["http.method"].StringValue = "GET";
            instance1.SpanTags["destination.port"].StringValue = "8888";
            instance1.SpanTags["request.headers.request.id"].StringValue = "|29b9f7da25a34eb183dfa589c30ee9e9.0";
            instance1.SpanTags["request.headers.synthetictest.runid"].StringValue = "29b9f7da25a34eb183dfa589c30ee9e9";
            instance1.SpanTags["request.headers.synthetictest.location"].StringValue = "us-va-ash-azr";
            instance1.SpanTags["request.headers.request.context"].StringValue = "appId=cid-v1:b146db49-7bee-425a-9dc5-a302329eb7b2";
            instance1.SpanTags["response.headers.request.context"].StringValue = "";

            // same incoming request, reported by inbound proxy of the destination-deployment-1 pod
            var instance2 = Common.GetStandardInstanceMsg();
            instance2.SpanTags["context.reporter.uid"].StringValue = "kubernetes://destination-deployment-1";
            instance2.SpanTags["context.reporter.kind"].StringValue = "inbound";
            instance2.SpanTags["source.uid"].StringValue = "kubernetes://istio-ingressgateway.istio-system";
            instance2.SpanTags["source.workload.namespace"].StringValue = "istio-system";
            instance2.SpanTags["source.workload.name"].StringValue = "istio-ingressgateway";
            instance2.SpanTags["source.name"].StringValue = "istio-ingressgateway";
            instance2.SpanTags["source.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["source.labels.istio.isingressgateway"].BoolValue = true;
            instance2.SpanTags["source.role.name"].StringValue = "istio-ingressway";
            instance2.SpanTags["source.role.instance"].StringValue = "istio-ingressway-1";
            instance2.SpanTags["destination.uid"].StringValue = "kubernetes://destination-deployment-1.default";
            instance2.SpanTags["destination.workload.namespace"].StringValue = "default";
            instance2.SpanTags["destination.workload.name"].StringValue = "destination-deployment";
            instance2.SpanTags["destination.name"].StringValue = "destination-deployment-1";
            instance2.SpanTags["destination.labels.appinsights.monitoring.enabled"].StringValue = "";
            instance2.SpanTags["destination.role.name"].StringValue = "destination-deployment";
            instance2.SpanTags["destination.role.instance"].StringValue = "destination-deployment-1";
            instance2.SpanTags["request.scheme"].StringValue = "http";
            instance2.SpanTags["request.path"].StringValue = "/some/path";
            instance2.SpanTags["http.useragent"].StringValue = "GSM";
            instance2.SpanTags["host"].StringValue = "destination-deployment-1";
            instance2.SpanTags["http.status_code"].StringValue = "200";
            instance2.SpanTags["http.path"].StringValue = "/some/path";
            instance2.SpanTags["http.method"].StringValue = "GET";
            instance2.SpanTags["destination.port"].StringValue = "8888";
            instance2.SpanTags["request.headers.request.id"].StringValue = "|29b9f7da25a34eb183dfa589c30ee9e9.0";
            instance2.SpanTags["request.headers.synthetictest.runid"].StringValue = "29b9f7da25a34eb183dfa589c30ee9e9";
            instance2.SpanTags["request.headers.synthetictest.location"].StringValue = "us-va-ash-azr";
            instance2.SpanTags["request.headers.request.context"].StringValue = "appId=cid-v1:b146db49-7bee-425a-9dc5-a302329eb7b2";
            instance2.SpanTags["response.headers.request.context"].StringValue = "";

            // ACT
            ITelemetry[] telemetryItems = telemetryGenerator.Generate(instance1, instance2).ToArray();

            // ASSERT
            // 2 items for gateway, 1 items for destination-deployment
            Assert.AreEqual(3, telemetryItems.Length);

            var gatewayRequest = telemetryItems[0] as RequestTelemetry;
            var gatewayDependency = telemetryItems[1] as DependencyTelemetry;
            var destinationRequest = telemetryItems[2] as RequestTelemetry;
            
            Assert.IsNotNull(gatewayRequest);
            Assert.IsNotNull(gatewayDependency);
            Assert.IsNotNull(destinationRequest);
            
            ValidateTelemetrySource(gatewayRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(gatewayDependency, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            ValidateTelemetrySource(destinationRequest, "istio-ingressgateway", "istio-ingressgateway", "istio-system", "destination-deployment-1", "destination-deployment", "default", "kubernetes://destination-deployment-1", "inbound");
            
            ValidateTelemetryOperationData(gatewayRequest, @"^\|29b9f7da25a34eb183dfa589c30ee9e9.0[A-Za-z0-9]{8}_$", @"^\|29b9f7da25a34eb183dfa589c30ee9e9\.0$", @"^29b9f7da25a34eb183dfa589c30ee9e9$");
            ValidateTelemetryOperationData(gatewayDependency, @"^\|29b9f7da25a34eb183dfa589c30ee9e9.0[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^\|29b9f7da25a34eb183dfa589c30ee9e9.0[A-Za-z0-9]{8}_$", @"^29b9f7da25a34eb183dfa589c30ee9e9$");
            ValidateTelemetryOperationData(destinationRequest, @"^\|29b9f7da25a34eb183dfa589c30ee9e9.0[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.[A-Za-z0-9]{8}_$", @"^\|29b9f7da25a34eb183dfa589c30ee9e9.0[A-Za-z0-9]{8}_[A-Za-z0-9]{8}\.$", @"^29b9f7da25a34eb183dfa589c30ee9e9$");

            Assert.AreEqual(gatewayRequest.Id, gatewayDependency.Context.Operation.ParentId);
            Assert.AreEqual(gatewayDependency.Id, destinationRequest.Context.Operation.ParentId);
        }

        [TestMethod]
        public void TelemetryGeneratorTests_MoreNeeded()
        {
            // ARRANGE
            var telemetryGenerator = new TelemetryGenerator(new []{"default"}, new string[]{});

            // ACT
            // telemetryGenerator.Generate(new InstanceMsg(){})
            // ASSERT
            Assert.Inconclusive("Implement unit tests here when more strict requirements are defined");
        }

        #region Helpers

        private static void ValidateTelemetrySource(ITelemetry telemetry, string sourceName, string sourceWorkloadName, string sourceWorkloadNamespace, string destinationName, string destinationWorkloadName, string destinationWorkloadNamespace, string reporterUid,
            string repoterKind)
        {
            Assert.AreEqual(reporterUid, telemetry.Context.Properties["k8s.context.reporter.uid"], "reporterUid did not match");
            Assert.AreEqual(repoterKind, telemetry.Context.Properties["k8s.context.reporter.kind"], "reporterKind did not match");

            Assert.AreEqual($"kubernetes://{sourceName}.{sourceWorkloadNamespace}", telemetry.Context.Properties["k8s.source.uid"], "k8s.source.uid did not match");
            Assert.AreEqual(sourceName, telemetry.Context.Properties["k8s.source.name"], "k8s.source.name did not match");
            Assert.AreEqual(sourceWorkloadName, telemetry.Context.Properties["k8s.source.workload.name"], "k8s.source.workload.name did not match");
            Assert.AreEqual(sourceWorkloadNamespace, telemetry.Context.Properties["k8s.source.workload.namespace"], "k8s.source.workload.namespace did not match");

            Assert.AreEqual($"kubernetes://{destinationName}.{destinationWorkloadNamespace}", telemetry.Context.Properties["k8s.destination.uid"], "kubernetes.destination.uid did not match");
            Assert.AreEqual(destinationName, telemetry.Context.Properties["k8s.destination.name"], "k8s.destination.name did not match");
            Assert.AreEqual(destinationWorkloadName, telemetry.Context.Properties["k8s.destination.workload.name"], "k8s.destination.workload.name did not match");
            Assert.AreEqual(destinationWorkloadNamespace, telemetry.Context.Properties["k8s.destination.workload.namespace"], "k8s.destination.workload.namespace did not match");
        }

        private static void ValidateTelemetryOperationData(OperationTelemetry telemetry, string id, string operationParentId, string operationId)
        {
            Assert.IsTrue(new Regex(id).IsMatch(telemetry.Id), $"Id did not match. Regex: {id}, value: {telemetry.Id}");
            Assert.IsTrue(new Regex(operationParentId).IsMatch(telemetry.Context.Operation.ParentId), $"operationParentId did not match. Regex: {id}, value: {telemetry.Context.Operation.ParentId}");
            Assert.IsTrue(new Regex(operationId).IsMatch(telemetry.Context.Operation.Id), $"operationId did not match. Regex: {id}, value: {telemetry.Context.Operation.Id}");
        }
        #endregion
    }
}