namespace Microsoft.IstioMixerPlugin.Library.Inputs
{
    /// <summary>
    /// Statistics regarding the current state of an Input.
    /// </summary>
    class InputStats
    {
        public int ConnectionCount = 0;

        public long InstancesReceived = 0;

        public long RequestsReceived = 0;

        public long InstancesFailed = 0;

        public override string ToString()
        {
            return $"ConnectionCount: {this.ConnectionCount}, RequestsReceived: {this.RequestsReceived}, InstancesReceived: {this.InstancesReceived}, InstancesFailed: {this.InstancesFailed}";
        }
    }
}
