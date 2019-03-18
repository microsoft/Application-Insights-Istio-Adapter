namespace Microsoft.IstioMixerPlugin.LibraryTest
{
    using Grpc.Core;
    using System;
    using System.Threading.Tasks;
    using Tracespan;

    public class GrpcWriter
    {
        private readonly HandleTraceSpanService.HandleTraceSpanServiceClient client;
        private readonly int port;

        public GrpcWriter(int port)
        {
            this.port = port;

            try
            {
                var channel = new Channel($"127.0.0.1:{this.port}", ChannelCredentials.Insecure);

                this.client = new HandleTraceSpanService.HandleTraceSpanServiceClient(channel);
                //this.handleTraceSpanAsyncCall = client.HandleTraceSpanAsync();
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error initializing the gRPC test client. {e.ToString()}"));
            }
        }

        public async Task Write(HandleTraceSpanRequest request)
        {
            try
            {
                await this.client.HandleTraceSpanAsync(request, new CallOptions()).ResponseAsync.ConfigureAwait(false);
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Error sending a message via gRpc. {e.ToString()}"));
            }
        }
    }
}
