using System;
using System.Web;
using System.Web.Caching;
using System.Xml;

namespace Zeus.BaseLibrary.Xml
{
	/// <summary>
	/// Summary description for XmlCachedDocument.
	/// </summary>
	public class CachedXmlDocument
	{
		public XmlDocument Document { get; }

		public CachedXmlDocument(string sFilename)
		{
			var pContext = HttpContext.Current;

			var sKey = "Zeus.BaseLibrary.Xml.CachedXmlDocument: " + sFilename.ToLower();

			// see if object is in cache
			var pObject = pContext.Cache.Get(sKey);
			if (pObject != null)
			{
				Document = (XmlDocument) pObject;
			}
			else
			{
				// place file in cache
				if (sFilename.IndexOf(":\\") > 0)
				{
					// absolute location specified
				}
				else
				{
					sFilename = pContext.Server.MapPath(sFilename);
				}

				Document = new XmlDocument();
				Document.Load(sFilename);
				pContext.Cache.Insert(sKey, Document, new CacheDependency(sFilename));
			}
		}
	}
}