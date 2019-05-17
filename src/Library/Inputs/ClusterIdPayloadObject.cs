namespace Microsoft.IstioMixerPlugin.Library.Inputs
{
    using System;
    public class ClusterIdPayloadObject
    {
        public  string clusterId = string.Empty;

        public override string ToString()
        {
            return $"clusterId : {this.clusterId}";
        }
    }
}
