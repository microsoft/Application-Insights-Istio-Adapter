namespace Microsoft.IstioMixerPlugin.Library
{
    using Microsoft.IstioMixerPlugin.Common;
    using Microsoft.IstioMixerPlugin.Library.Inputs;
    using System;
    using System.Net;
    using System.Runtime.Serialization.Json;

    public class WebServer
    {
        private HttpListener listener;
        private volatile bool isRunning = false;
        private readonly Configuration config;
        private readonly Object syncObject = new Object();
        internal bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
        }

        public WebServer(string config)
        {
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
        }

        public void Start()
        {
            if (this.isRunning)
            {
                Diagnostics.LogInfo("WebServer already running .. exiting");
                return;
            }
            // Create a listener.
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(this.config.HttpPrefix);

            lock (this.syncObject)
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

        public void Stop()
        {
            lock (this.syncObject)
            {
                if (!this.isRunning)
                {
                    Diagnostics.LogInfo("WebServer already stopped .. exiting");
                    return;
                }

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
                Diagnostics.LogInfo("WebServer stopped from taking any new requests");

            }
        }

        internal void ListenerCallbackAsync(IAsyncResult result)
        {
            lock (this.syncObject)
            {
                try
                {
                    HttpListener listener = (HttpListener)result.AsyncState;
                    HttpListenerContext context = listener.EndGetContext(result);
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;
                    if (!request.HttpMethod.Equals("Post", StringComparison.InvariantCultureIgnoreCase))
                    {
                        //we only support post method
                        Diagnostics.LogInfo(FormattableString.Invariant($"invalid http method : {request.HttpMethod}"));
                        response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    }
                    else if (!request.Headers["Content-Type"].Equals("application/json"))
                    {
                        // we only support json payloads
                        Diagnostics.LogInfo(FormattableString.Invariant($"invalid content type : {request.Headers["Content-Type"]}"));
                        //https://tools.ietf.org/html/rfc7231#section-6.5.13 
                        response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                    }
                    else
                    {
                        DataContractJsonSerializer serializer =
                            new DataContractJsonSerializer(typeof(JsonPayloadObject));
                        JsonPayloadObject payloadObject = (JsonPayloadObject)serializer.ReadObject(request.InputStream);
                        Diagnostics.LogInfo(FormattableString.Invariant($"received payload with cluster id : {payloadObject.clusterId}"));
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                    }
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