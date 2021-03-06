﻿using System;
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
        private readonly ILanguageManager _languageManager;
        private readonly bool _useBrowserLanguagePreferences;
        private readonly CustomUrlsSection _configUrlsSection;

        #endregion

        #region Constructors

        public UrlParser(IPersister persister, IHost host, IWebContext webContext, IItemNotifier notifier, HostSection config, ILanguageManager languageManager, CustomUrlsSection urls)
            : this(persister, host, webContext, notifier, config, languageManager, urls, null)
        {

        }

        public UrlParser(IPersister persister, IHost host, IWebContext webContext, IItemNotifier notifier, HostSection config, ILanguageManager languageManager, CustomUrlsSection urls, GlobalizationSection globalizationConfig)
        {
            _persister = persister;
            _host = host;
            _webContext = webContext;

            _ignoreExistingFiles = config.Web.IgnoreExistingFiles;

            notifier.ItemCreated += OnItemCreated;

            _languageManager = languageManager;

            DefaultDocument = "default";

            _useBrowserLanguagePreferences = (globalizationConfig != null) ? globalizationConfig.UseBrowserLanguagePreferences : false;

            _configUrlsSection = urls;
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

        public string BuildUrl(ContentItem item)
        {
            return BuildUrl(item, item.Language);
        }

        public virtual string BuildUrl(ContentItem item, string languageCode)
        {
            ContentItem startPage;
            return BuildUrlInternal(item, languageCode, out startPage);
        }

        protected string BuildUrlInternal(ContentItem item, string languageCode, out ContentItem startPage)
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

                    // Prepend language identifier, if this is not the default language.
                    if (_languageManager.Enabled && !LanguageSelection.IsHostLanguageMatch(ContentLanguage.PreferredCulture.Name))
                    {
                        if (LanguageSelection.GetHostFromLanguage(ContentLanguage.PreferredCulture.Name) != _webContext.LocalUrl.Authority)
                            url = url.SetAuthority(LanguageSelection.GetHostFromLanguage(ContentLanguage.PreferredCulture.Name));
                        else if (!string.IsNullOrEmpty(languageCode))
                            url = url.PrependSegment(languageCode, true);
                    }

                    // we've reached the start page so we're done here
                    return VirtualPathUtility.ToAbsolute("~" + url.PathAndQuery);
                }

                url = url.PrependSegment(current.Name, current.Extension);

                current = current.GetParent(languageCode);
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
            return item.ID == site.StartPageID || (item.TranslationOf != null && item.TranslationOf.ID == site.StartPageID);
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

        /// <summary>Invoked when an item is created or loaded from persistence medium.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnItemCreated(object sender, ItemEventArgs e)
        {
            ((IUrlParserDependency)e.AffectedItem).SetUrlParser(this);
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

        protected virtual ContentItem TryLoadingFromQueryString(string url, params string[] parameters)
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

            // Check if start of URL contains a language identifier.
            foreach (Language language in Context.Current.LanguageManager.GetAvailableLanguages())
                if (url.StartsWith(language.Name, StringComparison.InvariantCultureIgnoreCase))
                    throw new NotImplementedException();
            ContentItem notFound = NotFoundPage(url);

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

            return TryLoadingFromQueryString(url, PathData.ItemQueryKey, PathData.PageQueryKey) ?? Parse(startingPoint, url);
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

        private void LogIt(string what)
        {
            // create a writer and open the file
            TextWriter tw = new StreamWriter("c:\\sites\\zeus\\debugger.txt", true);

            // write a line of text to the file
            tw.WriteLine(System.DateTime.Now + " // " + what);

            // close the stream
            tw.Close();
        }/*
                        LogIt("In the cache all section : session says " + 
                            (System.Web.HttpContext.Current.Cache["customUrlCacheActivated"] == null ? "No setting" : "Setting Found") +
                            " : requestedUrl.Path = " + requestedUrl.Path +
                            " : _webContext.Url.Path = " + _webContext.Url.Path +
                            " : isFile? = " + isFile(_webContext.Url.Path));
                         */

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
                PathData data = new PathData();
                data.IsRewritable = false;
                return data;
            }
            else
            {
                ContentItem item = TryLoadingFromQueryString(requestedUrl, PathData.PageQueryKey);
                if (item != null)
                    return item.FindPath(requestedUrl["action"] ?? PathData.DefaultAction)
                            .SetArguments(requestedUrl["arguments"])
                            .UpdateParameters(requestedUrl.GetQueries());

                ContentItem startPage = GetStartPage(requestedUrl);
                string languageCode = GetLanguage(ref requestedUrl);
                string path = Url.ToRelative(requestedUrl.Path).TrimStart('~');
                PathData data = startPage.FindPath(path, languageCode).UpdateParameters(requestedUrl.GetQueries());

                if (data.IsEmpty())
                {
                    if (path.EndsWith(DefaultDocument, StringComparison.OrdinalIgnoreCase))
                    {
                        // Try to find path without default document.
                        data = StartPage
                                .FindPath(path.Substring(0, path.Length - DefaultDocument.Length))
                                .UpdateParameters(requestedUrl.GetQueries());
                    }

                    //cache data first time we go through this
                    if ((_configUrlsSection.ParentIDs.Count > 0) && (System.Web.HttpContext.Current.Cache["customUrlCacheActivated"] == null))
                    {
                        //the below takes resource and time, we only want one request doing this at a time, so set the flag immediately
                        System.Web.HttpContext.Current.Cache["customUrlCacheActivated"] = "true";
                        System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopLastRun"] = System.DateTime.Now;

                        foreach (CustomUrlsIDElement id in _configUrlsSection.ParentIDs)
                        {
                            // First check that the page referenced in web.config actually exists.
                            ContentItem customUrlPage = Persister.Get(id.ID);
                            if (customUrlPage == null)
                                continue;

                            //need to check all children of these nodes to see if there's a match
                            IEnumerable<ContentItem> AllContentItemsWithCustomUrls = Find.EnumerateAccessibleChildren(customUrlPage, id.Depth);
                            foreach (ContentItem ci in AllContentItemsWithCustomUrls)
                            {
                                if (ci.HasCustomUrl)
                                {
                                    System.Web.HttpContext.Current.Cache["customUrlCache_" + ci.Url] = ci.ID;
                                    System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + ci.Url] = "";
                                }
                            }
                        }

                        System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopComplete"] = System.DateTime.Now;
                    }

                    bool bTryCustomUrls = false;
                    {
                        if (!isFile(requestedUrl.Path, true))
                        {
                            foreach (CustomUrlsMandatoryStringsElement stringToFind in _configUrlsSection.MandatoryStrings)
                            {
                                if (stringToFind.IsRegex)
                                {
                                    if (Regex.IsMatch(_webContext.Url.Path, stringToFind.Value))                                        
                                    {
                                        bTryCustomUrls = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (_webContext.Url.Path.IndexOf(stringToFind.Value) > -1)
                                    {
                                        bTryCustomUrls = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (data.IsEmpty() && requestedUrl.Path.IndexOf(".") == -1 && bTryCustomUrls)
                    {

                        DateTime lastRun = Convert.ToDateTime(System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopLastRun"]);
                        DateTime lastComplete = System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopComplete"] == null ? DateTime.MinValue : Convert.ToDateTime(System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopLastRun"]);

                        //temp measure - sleep until the initial loop is complete
                        int loops = 0;
                        while (lastComplete < lastRun && loops < 20)
                        {
                            loops++;
                            System.Threading.Thread.Sleep(1000);
                            lastComplete = System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopComplete"] == null ? DateTime.MinValue : Convert.ToDateTime(System.Web.HttpContext.Current.Cache["customUrlCacheInitialLoopLastRun"]);
                        }

                        //this code can freak out the 2nd level caching in NHibernate, so clear it if within 5 mins of the last time the cache everything loop was called
                        //TO DO: implement this
                        /*
                        if (DateTime.Now.Subtract(lastRun).TotalMinutes < 5)
                        {
                            NHibernate.Cfg.Configuration Configuration = new NHibernate.Cfg.Configuration();
                            ISessionFactory sessionFactory = Configuration.BuildSessionFactory();

                            sessionFactory.EvictQueries();
                            foreach (var collectionMetadata in sessionFactory.GetAllCollectionMetadata())
                                sessionFactory.EvictCollection(collectionMetadata.Key);
                            foreach (var classMetadata in sessionFactory.GetAllClassMetadata())
                                sessionFactory.EvictEntity(classMetadata.Key);
                        }
                        */

                        //check cache for previously mapped item
                        if (System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path] == null)
                        {
                            //HACK!!  Using the RapidCheck elements in config try to pre-empt this being a known path with the action
                            //This needed to be implemented for performance reasons
                            string fullPath = _webContext.Url.Path;
                            string pathNoAction = "";
                            string action = "";
                            if (fullPath.LastIndexOf("/") > -1)
                            {
                                pathNoAction = fullPath.Substring(0, fullPath.LastIndexOf("/"));
                                action = fullPath.Substring(fullPath.LastIndexOf("/") + 1);
                            }

                            foreach (CustomUrlsRapidCheckElement possibleAction in _configUrlsSection.RapidCheck)
                            {
                                if (possibleAction.Action == action)
                                {
                                    //check for cache
                                    //see whether we have the root item in the cache...
                                    if (System.Web.HttpContext.Current.Cache["customUrlCache_" + pathNoAction] != null)
                                    {
                                        //we now have a match without any more calls to the database
                                        ContentItem ci = _persister.Get((int)System.Web.HttpContext.Current.Cache["customUrlCache_" + pathNoAction]);
                                        data = ci.FindPath(action);
                                        System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path] = ci.ID;
                                        System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + _webContext.Url.Path] = action;
                                        return data;
                                    }
                                }

                            }

                            // Check for Custom Urls (could be done in a service that subscribes to the IUrlParser.PageNotFound event)...
                            foreach (CustomUrlsIDElement id in _configUrlsSection.ParentIDs)
                            {
                                // First check that the page referenced in web.config actually exists.
                                ContentItem customUrlPage = Persister.Get(id.ID);
                                if (customUrlPage == null)
                                    continue;

                                //only search inside the parent id if we find that it has changed...
                                DateTime? parentUpdated = System.Web.HttpContext.Current.Cache["customUrlCacheParent_" + id.ID] == null ? null : (DateTime?)System.Web.HttpContext.Current.Cache["customUrlCacheParent_" + id.ID];

                                if (parentUpdated == null || parentUpdated.Value != customUrlPage.Updated)
                                {
                                    System.Web.HttpContext.Current.Cache["customUrlCacheParent_" + id.ID] = customUrlPage.Updated;

                                    //need to check all children of these nodes to see if there's a match
                                    ContentItem tryMatch =
                                        Find.EnumerateAccessibleChildren(customUrlPage, id.Depth).SingleOrDefault(
                                            ci => ci.Url.Equals(_webContext.Url.Path, StringComparison.InvariantCultureIgnoreCase));

                                    if (tryMatch != null)
                                    {
                                        data = tryMatch.FindPath(PathData.DefaultAction);
                                        System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path] = tryMatch.ID;
                                        System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + _webContext.Url.Path] = "";
                                        break;
                                    }
                                    //now need to check for an action...
                                    if (fullPath.LastIndexOf("/") > -1)
                                    {

                                        //see whether we have the root item in the cache...
                                        if (System.Web.HttpContext.Current.Cache["customUrlCache_" + pathNoAction] == null)
                                        {
                                            ContentItem tryMatchAgain =
                                                Find.EnumerateAccessibleChildren(Persister.Get(id.ID), id.Depth).SingleOrDefault(
                                                    ci => ci.Url.Equals(pathNoAction, StringComparison.InvariantCultureIgnoreCase));

                                            if (tryMatchAgain != null)
                                            {
                                                data = tryMatchAgain.FindPath(action);
                                                System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path] = tryMatchAgain.ID;
                                                System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + _webContext.Url.Path] = action;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            ContentItem ci = _persister.Get((int)System.Web.HttpContext.Current.Cache["customUrlCache_" + pathNoAction]);
                                            data = ci.FindPath(action);
                                            System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path] = ci.ID;
                                            System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + _webContext.Url.Path] = action;
                                            break;
                                        }
                                    }
                                }
                                else
                                { 
                                    //ignore, if parent hasn't been updated, no new URLs to search
                                }
                            }
                        }
                        else
                        {
                            ContentItem ci = _persister.Get((int)System.Web.HttpContext.Current.Cache["customUrlCache_" + _webContext.Url.Path]);
                            string act = System.Web.HttpContext.Current.Cache["customUrlCacheAction_" + _webContext.Url.Path].ToString();
                            if (string.IsNullOrEmpty(act))
                                return ci.FindPath(PathData.DefaultAction);
                            else
                                return ci.FindPath(act);
                        }
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

                data.IsRewritable = IsRewritable(_webContext.PhysicalPath);
                
                return data;
            }
        }

        protected virtual string GetLanguage(ref Url url)
        {
            // Check if start of URL contains a language identifier.
            string priorityLanguage = null;
            foreach (Language language in Context.Current.LanguageManager.GetAvailableLanguages())
                if (url.Path.Equals("/" + language.Name, StringComparison.InvariantCultureIgnoreCase) || url.Path.StartsWith("/" + language.Name + "/", StringComparison.InvariantCultureIgnoreCase))
                {
                    url = url.RemoveSegment(0);
                    priorityLanguage = language.Name;
                }

            ContentLanguage.Instance.SetCulture(priorityLanguage);
            return ContentLanguage.PreferredCulture.Name;
        }

        private bool IsRewritable(string path)
        {
            return _ignoreExistingFiles || (!File.Exists(path) && !Directory.Exists(path));
        }

        #endregion
    }
}