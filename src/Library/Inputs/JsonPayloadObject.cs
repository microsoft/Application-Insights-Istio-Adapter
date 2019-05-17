namespace Microsoft.IstioMixerPlugin.Library.Inputs
{
    using System;
    public class JsonPayloadObject
    {
        public  Guid clusterId = Guid.Empty;

        public override string ToString()
        {
            return $"clusterId : {this.clusterId}";
        }
    }
}
