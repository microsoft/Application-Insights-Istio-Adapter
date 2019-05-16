namespace Microsoft.IstioMixerPlugin.Library
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Net;
    using Microsoft.IstioMixerPlugin.Common;
    using System.IO;
    using System.Runtime.Serialization.Json;
    using Microsoft.IstioMixerPlugin.Library.Inputs;

    public class WebServer
    {
        private HttpListener listener;
        private volatile bool isRunning = false;
        private readonly Configuration config;
        public WebServer(string config)
        {
            if (isRunning)
            {
                Diagnostics.LogWarn(FormattableString.Invariant($"web server running . aborting"));
                return;
            }
            if (!HttpListener.IsSupported)
            {
                string logLine = FormattableString.Invariant($"HttpListener is not supported");
                Diagnostics.LogError(logLine);
                throw new InvalidOperationException(logLine);
            }

            if (config == null || config.Length == 0)
            {
                string logLine = FormattableString.Invariant($"config missing");
                Diagnostics.LogError(logLine);
                throw new ArgumentException(logLine);
            }

            this.config = new Configuration(config);

            // Create a listener.
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(this.config.HttpPrefix);

            lock (this.listener)
            {
                try
                {
                    this.listener.Start();
                    listener.BeginGetContext(new AsyncCallback(this.ListenerCallbackAsync), this.listener);
                    this.isRunning = true;
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(e.ToString());
                    // should not blow up if it cannot run the webserver
                }
            }

            Diagnostics.LogInfo(FormattableString.Invariant($"Webserver running"));
        }

        private void Stop()
        {
            lock (this.listener)
            {
                this.isRunning = false;
                try
                {
                    this.listener.Close();
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(e.ToString());
                    // should not blow up if it cannot run the webserver
                }
            }
        }

        private void ListenerCallbackAsync(IAsyncResult result)
        {
            lock (this.listener)
            {
                try
                {
                    HttpListener listener = (HttpListener)result.AsyncState;
                    // Call EndGetContext to complete the asynchronous operation.
                    HttpListenerContext context = listener.EndGetContext(result);

                    HttpListenerRequest request = context.Request;

                    DataContractJsonSerializer serializer =
                        new DataContractJsonSerializer(typeof(JsonPayloadObject));
                    JsonPayloadObject payloadObject = (JsonPayloadObject)serializer.ReadObject(request.InputStream);

                    Diagnostics.LogInfo(FormattableString.Invariant($"received payload with cluster id : {payloadObject.clusterId}"));

                    HttpListenerResponse response = context.Response;
                    response.StatusCode = (int)HttpStatusCode.Accepted;
                    response.Close();
                }
                catch (Exception e)
                {
                    Diagnostics.LogError(e.ToString());
                    // should not blow up if it cannot run the webserver
                }
                if (this.isRunning)
                {
                    this.listener.BeginGetContext(new AsyncCallback(this.ListenerCallbackAsync), this.listener);
                    Diagnostics.LogInfo("Restarting listening");
                }
            }
        }
    }
}