using System.Configuration;

namespace Zeus.Configuration
{
    public class RoutingSection : ConfigurationSection
    {
        [ConfigurationProperty("controllers")]
        [ConfigurationCollection(typeof(NamespaceElementCollection))]
        public NamespaceElementCollection Controllers
        {
            get { return (NamespaceElementCollection)this["controllers"]; }
            set { this["controllers"] = value; }
        }
    }
}
