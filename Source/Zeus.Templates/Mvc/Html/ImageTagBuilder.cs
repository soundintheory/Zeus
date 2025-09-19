using Ext.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace Zeus.Templates.Mvc.Html
{
    public class ImageTagBuilder : TagBuilder
    {
        public ImageTagBuilder() : base("img")
        {
        }

        public ImageTagBuilder Attr(string name, string value = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Attribute name cannot be null or empty");
            }

            Attributes[name] = value;

            return this;
        }

        public ImageTagBuilder Lazy()
        {
            return Attr("loading", "lazy");
        }

        public override string ToString()
        {
            if (!Attributes.ContainsKey("src") || string.IsNullOrEmpty(Attributes["src"]))
            {
                return string.Empty;
            }

            return ToString(TagRenderMode.SelfClosing);
        }

        public HtmlString ToHtmlString()
        {
            return new HtmlString(ToString());
        }
    }
}