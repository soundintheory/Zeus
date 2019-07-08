using System.Configuration;

namespace Zeus.Configuration
{
    public class NamespaceElement : ConfigurationElement
    {
        [ConfigurationProperty("namespace")]
        public string Namespace
        {
            get { return (string)this["namespace"]; }
            set { this["namespace"] = value; }
        }
    }
}
