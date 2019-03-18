namespace Microsoft.IstioMixerPlugin.Library.Inputs.GrpcInput
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Common;
    using Grpc.Core;

    using System.Linq;
    using Istio.Mixer.Adapter.Model.V1Beta1;
    using Istio.Policy.V1Beta1;
    using Tracespan;

    /// <summary>
    /// gRPC input for receiving instances from Istio's Mixer
    /// </summary>
    class GrpcInput
    {
        private CancellationTokenSource cts;
        private Server server;
        private InputStats stats;
        private readonly string host;
        private readonly int port;
        private readonly Action<InstanceMsg> onProcessInstance;
        
        public GrpcInput(string host, int port, Action<InstanceMsg> onProсessInstance)
        {
            this.host = host;
            this.port = port;
            this.onProcessInstance = onProсessInstance;
        }

        public void Start()
        {
            if (this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Start the input, it's already running"));
            }

            this.stats = new InputStats();
            this.cts = new CancellationTokenSource();

            try
            {
                this.server = new Server
                {
                    Services =
                    {
                        Tracespan.HandleTraceSpanService.BindService(new IstioMixerTraceSpanService( (request, context) =>
                        {
                            this.OnDataReceived(request, context);
                            return Task.FromResult(new ReportResult());
                        }))
                    },
                    Ports = {new ServerPort(this.host, this.port, ServerCredentials.Insecure)}
                };

                this.server.Start();

                this.IsRunning = true;
            }
            catch (System.Exception e)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Could not initialize the gRPC server. {e.ToString()}"));
            }
        }

        public void Stop()
        {
            if (!this.IsRunning)
            {
                throw new InvalidOperationException(
                    FormattableString.Invariant($"Can't Stop the input, it's not currently running"));
            }

            try
            {
                this.server.KillAsync().Wait(TimeSpan.FromSeconds(5));
            }
            finally
            {

                this.cts.Cancel();

                this.IsRunning = false;
            }
        }

        public bool IsRunning { get; private set; }

        public InputStats GetStats()
        {
            return this.stats;
        }

        private void OnDataReceived(HandleTraceSpanRequest request, ServerCallContext context)
        {
            try
            {
                Interlocked.Increment(ref this.stats.RequestsReceived);

                foreach(var instance in request.Instances)
                {
                    try
                    {
                        this.onProcessInstance?.Invoke(instance);

                        Interlocked.Increment(ref this.stats.InstancesReceived);
                    }
                    catch (System.Exception e)
                    {
                        // unexpected exception occured while processing the batch
                        Interlocked.Increment(ref this.stats.InstancesFailed);

                        Diagnostics.LogError(FormattableString.Invariant($"Unknown exception while processing a batch received through the gRPC input. {e.ToString()}"));
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // we have been stopped

            }
            catch (System.Exception e)
            {
                // unexpected exception occured
                Diagnostics.LogError(FormattableString.Invariant($"Unknown exception while reading from gRPC stream. {e.ToString()}"));

                this.Stop();
            }
        }

        #region gRPC servers

        public class IstioMixerTraceSpanService : Tracespan.HandleTraceSpanService.HandleTraceSpanServiceBase
        {
            private readonly Func<HandleTraceSpanRequest, ServerCallContext, Task<ReportResult>> onDataReceived;

            public IstioMixerTraceSpanService(Func<HandleTraceSpanRequest, ServerCallContext, Task<ReportResult>> onDataReceived)
            {
                this.onDataReceived = onDataReceived ?? throw new ArgumentNullException(nameof(onDataReceived));
            }

            public override async Task<ReportResult> HandleTraceSpan(HandleTraceSpanRequest request, ServerCallContext context)
            {
                return await this.onDataReceived(request, context).ConfigureAwait(false);
            }
        }

        #endregion
    }
}