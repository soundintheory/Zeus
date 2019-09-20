using System.Collections.Generic;
using System.Configuration;

namespace Zeus.Configuration
{
    public class NamespaceElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new NamespaceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((NamespaceElement)element).Namespace;
        }

        public void Add(string ns)
        {
            base.BaseAdd(new NamespaceElement { Namespace = ns });
        }

        public string[] ToArray()
        {
            var output = new List<string>();

            foreach (NamespaceElement element in this)
                output.Add(element.Namespace + ".*");

            return output.ToArray();
        }
    }
}
