namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
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
    using Uri = System.Uri;

    [TestClass]
    public class WebServerTests
    {
        private string config;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
        private WebServer webServer;
        ConcurrentQueue<ITelemetry> sentItems;

        [TestInitialize]
        public void Init()
        {
            config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
                        <Configuration>
                            <WebServer>
                                <HttpListenerPrefix>http://*:8888/test/</HttpListenerPrefix>
                            </WebServer>
                        </Configuration>
                        ";
            TelemetryClient telemetryClient = Common.SetupStubTelemetryClient(out sentItems);

            webServer = new WebServer(config, telemetryClient);
            Assert.IsFalse(webServer.IsRunning);
        }

        [TestCleanup]
        public void Cleanup()
        {
            webServer.Stop();
            Assert.IsFalse(webServer.IsRunning);
            sentItems.Clear();
            sentItems = null;
        }
        [TestMethod]
        public void WebServerTests_InitialState()
        {
            Assert.IsFalse(webServer.IsRunning);
        }

        [TestMethod]
        public void WebServerTests_StopBeforeStart()
        {
            Assert.IsFalse(webServer.IsRunning);
        }


        [TestMethod]
        public void WebServerTests_Start()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);
        }

        [TestMethod]
        public void WebServerTests_StartTwice()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);
        }

        [TestMethod]
        public async Task WebServerTests_Start_NonPostMethod()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);
            HttpClient client = new HttpClient();
            try
            {
                await client.GetStringAsync("http://127.0.0.1:8888/test/");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual<string>(e.Message, @"Response status code does not indicate success: 405 (Method Not Allowed).");
                Common.AssertIsTrueEventually(() => sentItems.Count == 0);
            }
        }

        [TestMethod]
        public async Task WebServerTests_Start_Post_NotJson()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri("http://127.0.0.1:8888/test/"),
                Method = HttpMethod.Post,
            };

            Dictionary<string, string> values = new Dictionary<string, string>{
                                                           { "ana", "are" },
                                                           { "mere", "pere" }
                                                       };
            var content = new FormUrlEncodedContent(values);
            HttpResponseMessage response = await client.PostAsync("http://127.0.0.1:8888/test/", content);
            Assert.AreEqual<string>(response.ReasonPhrase, "Unsupported Media Type");
            Common.AssertIsTrueEventually(() => sentItems.Count == 0);
        }

        [TestMethod]
        public void WebServerTests_Write_Valid_Json()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8888/test/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"clusterId\" : \"66010356-d8a5-42d3-8593-6aaa3aeb1c11\"}";

            streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Assert.AreEqual<HttpStatusCode>(httpResponse.StatusCode, HttpStatusCode.Accepted);
            Common.AssertIsTrueEventually(() => sentItems.Count == 1);
        }

        [TestMethod]
        public void WebServerTests_Write_Invalid_Json()
        {
            webServer.Start();
            Assert.IsTrue(webServer.IsRunning);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://127.0.0.1:8888/test/");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = "{\"clusterId\" : \"66010356-d8a5-42d3-8593-6aaa3aeb1c11";

                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }
            try
            {
                httpWebRequest.GetResponse();
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual<string>(e.Message, "The remote server returned an error: (400) Bad Request.");
                Common.AssertIsTrueEventually(() => sentItems.Count == 0);
            }
        }
    }
}
