namespace Microsoft.IstioMixerPlugin.Library.Inputs
{
    class JsonPayloadObject
    {
        public int id = -1;

        public override string ToString()
        {
            return $"id : {this.id}";
        }
    }
}
