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
        private readonly Configuration config;

        internal bool IsRunning
        {
            get
            {
                return this.listener?.IsListening ?? false;
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

            if (String.IsNullOrEmpty(config))
            {
                string logLine = FormattableString.Invariant($"config missing");
                Diagnostics.LogError(logLine);
                throw new ArgumentException(logLine);
            }

            this.config = new Configuration(config);
        }

        public void Start()
        {
            if (this.IsRunning)
            {
                Diagnostics.LogInfo("WebServer already running .. exiting");
                return;
            }
            // Create a listener.
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(this.config.HttpListenerPrefix);

            try
            {
                this.listener.Start();
                listener.BeginGetContext(new AsyncCallback(this.ListenerCallbackAsync), this.listener);
            }
            catch (Exception e)
            {
                Diagnostics.LogError(e.ToString());
                throw e;
            }

            Diagnostics.LogInfo(FormattableString.Invariant($"Webserver running"));
        }

        public void Stop()
        {
            if (!this.IsRunning)
            {
                Diagnostics.LogInfo("WebServer already stopped .. exiting");
                return;
            }

            try
            {
               this.listener.Close();
            }
            catch (Exception e)
            {
                Diagnostics.LogError(e.ToString());
                throw e;
            }
            Diagnostics.LogInfo("WebServer stopped from taking any new requests");
        }

        internal void ListenerCallbackAsync(IAsyncResult result)
        {
            if (this.IsRunning)
            {
                HttpListener listener = (HttpListener)result.AsyncState;
                HttpListenerContext context = listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                try
                {

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
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(ClusterIdPayloadObject));
                        ClusterIdPayloadObject payloadObject = (ClusterIdPayloadObject)serializer.ReadObject(request.InputStream);
                        Diagnostics.LogInfo(FormattableString.Invariant($"received payload with cluster id : {payloadObject.clusterId}"));
                        response.StatusCode = (int)HttpStatusCode.Accepted;
                    }

                }
                catch (Exception e)
                {
                    Diagnostics.LogError($"error processing request : {e.ToString()}");
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
                finally
                {
                    response.Close();
                }

                this.listener.BeginGetContext(new AsyncCallback(this.ListenerCallbackAsync), this.listener);
                Diagnostics.LogInfo("Restarting listening");
            }
        }
    }
}