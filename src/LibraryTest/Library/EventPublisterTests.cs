namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Channel;
    using Microsoft.ApplicationInsights.Extensibility.Implementation;
    using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
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
            DiagnosticsTelemetryModule dm = new DiagnosticsTelemetryModule();
            TelemetryModules.Instance.Modules.Add(dm);
            publisher = new EventPublisher();
        }

     

        [TestMethod]
        public void EventPublisterTests_SendOneItem()
        {
           Assert.IsTrue(publisher.UpdateClusterId("123abc"));
        }

        [TestMethod]
        public void EventPublisterTests_SendMultipleItems()
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.IsTrue(publisher.UpdateClusterId("123abc"));
            }
        }

        [TestMethod]
        public void EventPublisterTests_SendOneEmptyItem()
        {
            Assert.IsFalse(publisher.UpdateClusterId(null));
            Assert.IsTrue(publisher.UpdateClusterId("123abc"));
        }

        [TestMethod]
        public void EventPublisterTests_NoModule()
        {
            TelemetryModules.Instance.Modules.Clear();
            publisher = new EventPublisher();
            Assert.IsFalse(publisher.UpdateClusterId("123abc"));
        }
    }
}
