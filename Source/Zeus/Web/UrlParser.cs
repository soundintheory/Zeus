using System;
using System.IO;
using System.Web;
using Zeus.BaseLibrary.Web;
using Zeus.Configuration;
using Zeus.Globalization;
using Zeus.Globalization.ContentTypes;
using Zeus.Persistence;
using System.Linq;
using System.Collections.Generic;
using NHibernate;
using System.Text.RegularExpressions;

namespace Zeus.Web
{
    public class UrlParser : IUrlParser
    {
        #region Fields

        private readonly IPersister _persister;
        private readonly IHost _host;
        protected readonly IWebContext _webContext;
        private readonly bool _ignoreExistingFiles;

        #endregion

        #region Constructors

        public UrlParser(IPersister persister, IHost host, IWebContext webContext, HostSection config)
            : this(persister, host, webContext, config, null)
        {

        }

        public UrlParser(IPersister persister, IHost host, IWebContext webContext, HostSection config, GlobalizationSection globalizationConfig)
        {
            _persister = persister;
            _host = host;
            _webContext = webContext;

            _ignoreExistingFiles = config.Web.IgnoreExistingFiles;

            DefaultDocument = "default";
        }

        #endregion

        #region Events

        public event EventHandler<PageNotFoundEventArgs> PageNotFound;

        #endregion

        #region Properties

        protected IHost Host
        {
            get { return _host; }
        }

        protected IPersister Persister
        {
            get { return _persister; }
        }

        /// <summary>Gets or sets the default content document name. This is usually "/default.aspx".</summary>
        public string DefaultDocument { get; set; }

        /// <summary>Gets the current start page.</summary>
        public virtual ContentItem StartPage
        {
            get { return _persister.Repository.Load(_host.CurrentSite.StartPageID); }
        }

        public ContentItem CurrentPage
        {
            get { return _webContext.CurrentPage ?? (_webContext.CurrentPage = (ResolvePath(_webContext.Url).CurrentItem)); }
        }

        #endregion

        #region Methods

        public virtual string BuildUrl(ContentItem item)
        {
            return BuildUrlInternal(item, out _);
        }

        protected string BuildUrlInternal(ContentItem item, out ContentItem startPage)
        {
            startPage = null;
            ContentItem current = item;
            Url url = new Url("/");

            // Walk the item's parent items to compute it's url
            do
            {
                if (IsStartPage(current))
                {
                    startPage = current;

                    if (item.VersionOf != null)
                        url = url.AppendQuery(PathData.PageQueryKey, item.ID);

                    // we've reached the start page so we're done here
                    return VirtualPathUtility.ToAbsolute("~" + url.PathAndQuery);
                }

                url = url.PrependSegment(current.Name, current.Extension);

                current = current.GetParent();
            } while (current != null);

            // If we didn't find the start page, it means the specified
            // item is not part of the current site.
            return "/?" + PathData.PageQueryKey + "=" + item.ID;
            //return item.FindPath(PathData.DefaultAction).RewrittenUrl;
        }

        protected virtual bool IsStartPage(ContentItem item)
        {
            return IsStartPage(item, _host.CurrentSite);
        }

        protected static bool IsStartPage(ContentItem item, Site site)
        {
            return item.ID == site.StartPageID;
        }

        /// <summary>Handles virtual directories and non-page items.</summary>
        /// <param name="url">The relative url.</param>
        /// <param name="item">The item whose url is supplied.</param>
        /// <returns>The absolute url to the item.</returns>
        protected virtual string ToAbsolute(string url, ContentItem item)
        {
            if (string.IsNullOrEmpty(url) || url == "/")
                url = _webContext.ToAbsolute("~/");
            else
                url = _webContext.ToAbsolute("~" + url + item.Extension);

            return url;
        }

        /// <summary>Checks if an item is startpage or root page</summary>
        /// <param name="item">The item to compare</param>
        /// <returns>True if the item is a startpage or a rootpage</returns>
        public virtual bool IsRootOrStartPage(ContentItem item)
        {
            return item.ID == _host.CurrentSite.RootItemID || IsStartPage(item);
        }

        private int? FindQueryStringReference(string url, params string[] parameters)
        {
            string queryString = Url.QueryPart(url);
            if (!string.IsNullOrEmpty(queryString))
            {
                string[] queries = queryString.Split('&');

                foreach (string parameter in parameters)
                {
                    int parameterLength = parameter.Length + 1;
                    foreach (string query in queries)
                    {
                        if (query.StartsWith(parameter + "=", StringComparison.InvariantCultureIgnoreCase))
                        {
                            int id;
                            if (int.TryParse(query.Substring(parameterLength), out id))
                            {
                                return id;
                            }
                        }
                    }
                }
            }
            return null;
        }

        protected virtual bool TryLoadingFromQueryString(string url, out ContentItem item, params string[] parameters)
        {
            item = LoadFromQueryString(url, parameters);
            return item != null;
        }

        protected virtual ContentItem LoadFromQueryString(string url, params string[] parameters)
        {
            int? itemID = FindQueryStringReference(url, parameters);
            if (itemID.HasValue)
                return _persister.Get<ContentItem>(itemID.Value);
            return null;
        }

        protected virtual ContentItem Parse(ContentItem current, string url)
        {
            if (current == null) throw new ArgumentNullException("current");

            url = CleanUrl(url);

            if (url.Length == 0)
                return current;

            return current.GetChild(url) ?? NotFoundPage(url);
        }

        /// <summary>May be overridden to provide custom start page depending on authority.</summary>
        /// <param name="url">The host name and path information.</param>
        /// <returns>The configured start page.</returns>
        protected virtual ContentItem GetStartPage(Url url)
        {
            return StartPage;
        }

        /// <summary>Finds an item by traversing names from the start page.</summary>
        /// <param name="url">The url that should be traversed.</param>
        /// <returns>The content item matching the supplied url.</returns>
        public virtual ContentItem Parse(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");

            ContentItem startingPoint = GetStartPage(url);

            return LoadFromQueryString(url, PathData.ItemQueryKey, PathData.PageQueryKey) ?? Parse(startingPoint, url);
        }

        private string CleanUrl(string url)
        {
            url = Url.PathPart(url);
            url = _webContext.ToAppRelative(url);
            url = url.TrimStart('~', '/');
            if (url.EndsWith(".aspx", StringComparison.InvariantCultureIgnoreCase))
                url = url.Substring(0, url.Length - ".aspx".Length);
            return url;
        }

        protected virtual ContentItem NotFoundPage(string url)
        {
            if (url.Equals(DefaultDocument, StringComparison.InvariantCultureIgnoreCase))
                return StartPage;

            PageNotFoundEventArgs args = new PageNotFoundEventArgs(url);
            if (PageNotFound != null)
                PageNotFound(this, args);

            if (System.Web.HttpContext.Current != null)
                System.Web.HttpContext.Current.Response.StatusCode = 404;

            return args.AffectedItem;
        }

        private bool isFile(string path, bool includeImages)
        {
            path = path.ToLower();
            if (path.EndsWith(".css"))
                return true;
            else if (path.EndsWith(".gif") && includeImages)
                return true;
            else if (path.EndsWith(".png") && includeImages)
                return true;
            else if (path.EndsWith(".jpg") && includeImages)
                return true;
            else if (path.EndsWith(".jpeg") && includeImages)
                return true;
            else if (path.EndsWith(".js"))
                return true;
            else if (path.EndsWith(".axd"))
                return true;
            else if (path.EndsWith(".ashx"))
                return true;
            else if (path.EndsWith(".ico"))
                return true;
            else if (path.EndsWith(".css"))
                return true;
            else if (path.EndsWith(".swf"))
                return true;

            return false;
        }

        public PathData ResolvePath(string url)
        {
            Url requestedUrl = url;

            //look for files etc and ignore
            bool bNeedsProcessing = true;
            if (requestedUrl.Path.ToLower().StartsWith("/assets/"))
                bNeedsProcessing = false;
            if (requestedUrl.Path.ToLower().StartsWith("/content/"))
                bNeedsProcessing = false;
            else if (!requestedUrl.Path.StartsWith("/"))
                bNeedsProcessing = false;
            else if (isFile(requestedUrl.Path, false))
                bNeedsProcessing = false;

            if (!bNeedsProcessing)
            {
                return new PathData
                {
                    IsRewritable = false
                };
            }

            if (TryLoadingFromQueryString(requestedUrl, out var item, PathData.PageQueryKey))
            {
                return item.FindPath(requestedUrl["action"] ?? PathData.DefaultAction)
                        .SetArguments(requestedUrl["arguments"])
                        .UpdateParameters(requestedUrl.GetQueries());
            }

            ContentItem startPage = GetStartPage(requestedUrl);
            string path = Url.ToRelative(requestedUrl.Path).TrimStart('~');
            PathData data = startPage.FindPath(path).UpdateParameters(requestedUrl.GetQueries());

            if (data.IsEmpty())
            {
                if (path.EndsWith(DefaultDocument, StringComparison.OrdinalIgnoreCase))
                {
                    // Try to find path without default document.
                    data = StartPage
                            .FindPath(path.Substring(0, path.Length - DefaultDocument.Length))
                            .UpdateParameters(requestedUrl.GetQueries());
                }

                if (data.IsEmpty())
                {
                    // Allow user code to set path through event
                    if (PageNotFound != null)
                    {
                        PageNotFoundEventArgs args = new PageNotFoundEventArgs(requestedUrl);
                        args.AffectedPath = data;
                        PageNotFound(this, args);

                        if (args.AffectedItem != null)
                            data = args.AffectedItem.FindPath(PathData.DefaultAction);
                        else
                            data = args.AffectedPath;

                        data.Is404 = true;
                    }
                }
            }

            try
            {
                data.IsRewritable = IsRewritable(_webContext.PhysicalPath);
            }
            catch (PathTooLongException)
            {
                data.IsRewritable = false;
            }

            return data;
        }

        private bool IsRewritable(string path)
        {
            return _ignoreExistingFiles || (!File.Exists(path) && !Directory.Exists(path));
        }

        #endregion
    }
}