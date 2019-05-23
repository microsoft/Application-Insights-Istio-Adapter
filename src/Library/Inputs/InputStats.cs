namespace Microsoft.IstioMixerPlugin.Library.Inputs
{
    /// <summary>
    /// Statistics regarding the current state of an Input.
    /// </summary>
    class InputStats
    {
        public int ConnectionCount = 0;

        public long InstancesSucceeded = 0;

        public long RequestsReceived = 0;

        public long InstancesFailed = 0;

        public override string ToString()
        {
            return $"ConnectionCount: {this.ConnectionCount}, RequestsReceived: {this.RequestsReceived}, InstancesSucceeded: {this.InstancesSucceeded}, InstancesFailed: {this.InstancesFailed}";
        }
    }
}
