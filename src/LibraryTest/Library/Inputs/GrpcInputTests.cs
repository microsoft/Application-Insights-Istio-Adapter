namespace Microsoft.IstioMixerPlugin.LibraryTest.Library.Inputs.GrpcInput
{
    using IstioMixerPlugin.Library.Inputs.GrpcInput;
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Tracespan;
    using VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class GrpcInputTests
    {
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        [TestMethod]
        public void GrpcInputTests_StartsAndStops()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, null);

            // ACT
            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            input.Stop();

            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcInputTests.DefaultTimeout));

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GrpcInputTests_CantStartWhileRunning()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, null);

            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            // ACT
            input.Start();

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GrpcInputTests_CantStopWhileStopped()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, null);
            
            // ACT
            input.Stop();

            // ASSERT
        }

        [TestMethod]
        public async Task GrpcInputTests_ReceivesData()
        {
            // ARRANGE
            int instancesReceived = 0;
            InstanceMsg receivedInstance = null;

            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, instance =>
            {
                instancesReceived++;
                receivedInstance = instance;
            });
            input.Start();
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port);

            // ACT
            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg() { SourceName = "SourceName1"});

            await grpcWriter.Write(request).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().InstancesReceived == 1 && instancesReceived == 1 &&
                      receivedInstance.SourceName == "SourceName1", GrpcInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcInputTests.DefaultTimeout));
        }

        [TestMethod]
        public void GrpcInputTests_ReceivesDataFromMultipleClients()
        {
            // ARRANGE
            int instancesReceived = 0;
            InstanceMsg receivedInstance = null;
            
            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, instance =>
            {
                Interlocked.Increment(ref instancesReceived);
                receivedInstance = instance;
            });
            input.Start();
            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            // ACT
            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg() { SourceName = "SourceName1"});

            Parallel.For(0, 1000, new ParallelOptions() {MaxDegreeOfParallelism = 1000}, async i =>
            {
                var grpcWriter = new GrpcWriter(port);

                await grpcWriter.Write(request).ConfigureAwait(false);
            });

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.GetStats().InstancesReceived == 1000 && instancesReceived == 1000, GrpcInputTests.DefaultTimeout);

            input.Stop();
            Assert.IsTrue(SpinWait.SpinUntil(() => !input.IsRunning, GrpcInputTests.DefaultTimeout));
        }

        [TestMethod]
        public async Task GrpcInputTests_StopsWhileWaitingForData()
        {
            // ARRANGE
            int instancesReceived = 0;
            InstanceMsg receivedInstance = null;

            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, instance =>
            {
                instancesReceived++;
                receivedInstance = instance;
            });

            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port);

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg() {SourceName = "SourceName1"});

            await grpcWriter.Write(request).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().InstancesReceived == 1 && instancesReceived == 1 &&
                      receivedInstance.SourceName == "SourceName1", GrpcInputTests.DefaultTimeout);

            // ACT
            input.Stop();
            
            // ASSERT
            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().InstancesReceived == 1 && instancesReceived == 1 &&
                      receivedInstance.SourceName == "SourceName1", GrpcInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcInputTests_StopsAndRestarts()
        {
            // ARRANGE
            int instancesReceived = 0;
            InstanceMsg receivedInstance = null;

            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, instance =>
            {
                instancesReceived++;
                receivedInstance = instance;
            });

            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port);

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg() {SourceName = "SourceName1"});

            await grpcWriter.Write(request).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.GetStats().InstancesReceived == 1 && instancesReceived == 1 &&
                      receivedInstance.SourceName == "SourceName1", GrpcInputTests.DefaultTimeout);

            // ACT
            input.Stop();

            Common.AssertIsTrueEventually(
                () => !input.IsRunning && input.GetStats().InstancesReceived == 1 && instancesReceived == 1 &&
                      receivedInstance.SourceName == "SourceName1", GrpcInputTests.DefaultTimeout);

            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            grpcWriter = new GrpcWriter(port);
            request.Instances.Single().SourceName = "SourceName2";
            await grpcWriter.Write(request).ConfigureAwait(false);

            // ASSERT
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().InstancesReceived == 1 && instancesReceived == 2 &&
                      receivedInstance.SourceName == "SourceName2", GrpcInputTests.DefaultTimeout);
        }

        [TestMethod]
        public async Task GrpcInputTests_HandlesExceptionsInProcessingHandler()
        {
            // ARRANGE
            int port = Common.GetPort();
            var input = new GrpcInput("localhost", port, instance => throw new InvalidOperationException());

            input.Start();

            Assert.IsTrue(SpinWait.SpinUntil(() => input.IsRunning, GrpcInputTests.DefaultTimeout));

            var grpcWriter = new GrpcWriter(port);

            var request = new HandleTraceSpanRequest();
            request.Instances.Add(new InstanceMsg() {SourceName = "SourceName1"});

            // ACT
            await grpcWriter.Write(request).ConfigureAwait(false);

            // ASSERT

            // must have handled the exception by logging it
            // should still be able to process items
            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().InstancesReceived == 0 && input.GetStats().InstancesFailed == 1,
                GrpcInputTests.DefaultTimeout);

            await grpcWriter.Write(request).ConfigureAwait(false);

            Common.AssertIsTrueEventually(
                () => input.IsRunning && input.GetStats().InstancesReceived == 0 && input.GetStats().InstancesFailed == 2,
                GrpcInputTests.DefaultTimeout);
        }
    }
}
