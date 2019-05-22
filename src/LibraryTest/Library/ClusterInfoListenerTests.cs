namespace Microsoft.IstioMixerPlugin.LibraryTest.Library
{
    using IstioMixerPlugin.Library;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using VisualStudio.TestTools.UnitTesting;
    using Uri = System.Uri;

    [TestClass]
    public class ClusterInfoListenerTests
    {
        private string config;
        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(1);
        private ClusterInfoListener ciListener;

        [TestInitialize]
        public void Init()
        {
            config = $@"<?xml version=""1.0"" encoding=""utf-8"" ?>
                        <Configuration>
                            <ClusterInfoListener>
                                <HttpListenerPrefix>http://*:8888/test/</HttpListenerPrefix>
                            </ClusterInfoListener>
                        </Configuration>
                        ";

            ciListener = new ClusterInfoListener(config);
            Assert.IsFalse(ciListener.IsRunning);
        }

        [TestCleanup]
        public void Cleanup()
        {
            ciListener.Stop();
            Assert.IsFalse(ciListener.IsRunning);
        }
        [TestMethod]
        public void ClusterInfoListenerTests_InitialState()
        {
            Assert.IsFalse(ciListener.IsRunning);
        }

        [TestMethod]
        public void ClusterInfoListenerTests_StopBeforeStart()
        {
            Assert.IsFalse(ciListener.IsRunning);
        }


        [TestMethod]
        public void ClusterInfoListenerTests_Start()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);
        }

        [TestMethod]
        public void ClusterInfoListenerTests_StartTwice()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);
        }

        [TestMethod]
        public async Task ClusterInfoListenerTestsTests_Start_NonPostMethod()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);
            HttpClient client = new HttpClient();
            try
            {
                await client.GetStringAsync("http://127.0.0.1:8888/test/");
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.AreEqual<string>(e.Message, @"Response status code does not indicate success: 405 (Method Not Allowed).");
            }
        }

        [TestMethod]
        public async Task ClusterInfoListenerTests_Start_Post_NotJson()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);
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
        }

        [TestMethod]
        public void ClusterInfoListenerTests_Write_Valid_Json()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);

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
        }

        [TestMethod]
        public void ClusterInfoListenerTests_Write_Invalid_Json()
        {
            ciListener.Start();
            Assert.IsTrue(ciListener.IsRunning);

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
            }
        }
    }
}
