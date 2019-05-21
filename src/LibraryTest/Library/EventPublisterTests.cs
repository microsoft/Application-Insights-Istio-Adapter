namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.IstioMixerPlugin.Library;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Text;

    [TestClass]
    public class EventPublisterTests
    {
        private EventPublisher publisher;
        ConcurrentQueue<ITelemetry> sentItems;

        [TestInitialize]
        public void Initialize()
        {
            // ARRANGE
            TelemetryClient telemetryClient = Common.SetupStubTelemetryClient(out sentItems);
            publisher = new EventPublisher(telemetryClient);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sentItems.Clear();
            sentItems = null;
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EventPublisterTests_FailInit()
        {
            EventPublisher ep = new EventPublisher(null);
        }

        [TestMethod]
        public void EventPublisterTests_SendOneItem()
        {
            publisher.UpdateClusterId("123abc");
            Common.AssertIsTrueEventually(() => sentItems.Count == 1);
        }

        [TestMethod]
        public void EventPublisterTests_SendMultipleItems()
        {
            for (int i = 0; i < 10; i++)
            {
                publisher.UpdateClusterId("123abc");
            }
            Common.AssertIsTrueEventually(() => sentItems.Count == 10);
        }

        [TestMethod]
        public void EventPublisterTests_SendOneEmptyItem()
        {
            publisher.UpdateClusterId(null);
            publisher.UpdateClusterId("123abc");
            Common.AssertIsTrueEventually(() => sentItems.Count == 1);
        }
    }
}
