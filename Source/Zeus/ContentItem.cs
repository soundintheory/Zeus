using System;
using System.Text;
using Ext.Net;
using Zeus.Admin;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Globalization;
using Zeus.Integrity;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Linq;
using Zeus.Linq;
using Zeus.Persistence;
using Zeus.Web;
using Zeus.Security;
using System.Security.Principal;
using Zeus.Web.Hosting;
using System.Threading;
using MongoDB.Bson.Serialization.Attributes;
using Zeus.Web.Caching;

namespace Zeus
{
    [RestrictParents(typeof(ContentItem))]
    [System.Serializable]
    public abstract class ContentItem : INode, IEditableObject
    {
        #region Private Fields

        private IList<AuthorizationRule> _authorizationRules;
        private IList<LanguageSetting> _languageSettings;
        private string _name;
        private DateTime? _expires;
        private IList<ContentItem> _children = new List<ContentItem>();
        private IList<ContentItem> _translations = new List<ContentItem>();
        private IDictionary<string, PropertyData> _details = new Dictionary<string, PropertyData>();
        private IDictionary<string, PropertyCollection> _detailCollections = new Dictionary<string, PropertyCollection>();
        private string _url;
        private bool _visible;

        #endregion

        #region Public Properties (persisted)

        /// <summary>Gets or sets item ID.</summary>
        public virtual int ID { get; set; }

        /// <summary>Gets or sets this item's parent. This can be null for root items and previous versions but should be another page in other situations.</summary>
        [BsonIgnore]
        public virtual ContentItem Parent { get; set; }

        /// <summary>Gets or sets the item's title. This is used in edit mode and probably in a custom implementation.</summary>
        [Copy]
        public virtual string Title { get; set; }

        /// <summary>Gets or sets the item's name. This is used to compute the item's url and can be used to uniquely identify the item among other items on the same level.</summary>
        [Copy]
        public virtual string Name
        {
            get
            {
                return _name ?? (ID > 0 ? ID.ToString() : null);
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    _name = null;
                else
                    _name = value;
                _url = null;
            }
        }

        /// <summary>Gets or sets when this item was initially created.</summary>
        [Copy]
        public virtual DateTime Created { get; set; }

        /// <summary>Gets or sets the date this item was updated.</summary>
        [Copy]
        public virtual DateTime Updated { get; set; }
        public virtual void ReorderAction() {  }

        /// <summary>Gets or sets the publish date of this item.</summary>
        [BsonIgnore]
        public virtual DateTime? Published { get; set; }

        /// <summary>Gets or sets the expiration date of this item.</summary>
        [BsonIgnore]
        public virtual DateTime? Expires
        {
            get { return _expires; }
            set { _expires = value != DateTime.MinValue ? value : null; }
        }

        /// <summary>Gets or sets the sort order of this item.</summary>
        [Copy]
        [BsonIgnore]
        public virtual int SortOrder { get; set; }

        /// <summary>Gets or sets whether this item is visible. This is normally used to control it's visibility in the site map provider.</summary>
        [Copy]
        public virtual bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
            }
        }

        /// <summary>Gets or sets the published version of this item. If this value is not null then this item is a previous version of the item specified by VersionOf.</summary>
        [BsonIgnore]
        public virtual ContentItem VersionOf { get; set; }

        /// <summary>
        /// Gets or sets the version number of this item. This starts at 1, and increases for later versions.
        /// </summary>
        [Copy]
        [BsonIgnore]
        public virtual int Version { get; set; }

        /// <summary>Gets or sets the original language version of this item. If this value is not null then this item is a translated version of the item specified by TranslationOf.</summary>
        [BsonIgnore]
        public virtual ContentItem TranslationOf { get; set; }

        /// <summary>
        /// Gets or sets the language code of this item.
        /// </summary>
        [Copy]
        [BsonIgnore]
        public virtual string Language { get; set; }

        /// <summary>Gets or sets the name of the identity who saved this item.</summary>
        [Copy]
        public virtual string SavedBy { get; set; }

        /// <summary>Gets or sets the details collection. These are usually accessed using the e.g. item["Detailname"]. This is a place to store content data.</summary>
        [BsonIgnore]
        public IDictionary<string, PropertyData> Details
        {
            get { return _details; }
            set { _details = value; }
        }

        /// <summary>Gets or sets the details collection collection. These are details grouped into a collection.</summary>
        [BsonIgnore]
        public IDictionary<string, PropertyCollection> DetailCollections
        {
            get { return _detailCollections; }
            set { _detailCollections = value; }
        }

        /// <summary>Gets or sets all a collection of child items of this item ignoring permissions. If you want the children the current user has permission to use <see cref="GetChildren()"/> instead.</summary>
        [BsonIgnore]
        public virtual IList<ContentItem> Children
        {
            get { return _children; }
            set { _children = value; }
        }

        /// <summary>Gets or sets all a collection of child items of this item ignoring permissions. If you want the children the current user has permission to use <see cref="GetChildren()"/> instead.</summary>
        [BsonIgnore]
        public virtual IList<ContentItem> Translations
        {
            get { return _translations; }
            set { _translations = value; }
        }

        [BsonIgnore]
        public virtual int? OverrideCacheID { get; set; }

        public int CacheID { get { return OverrideCacheID ?? ID; } }

        #endregion

        #region Public Properties (generated)

        /// <summary>The default file extension for this content item, e.g. ".aspx".</summary>
        public virtual string Extension
        {
            get { return BaseLibrary.Web.Url.DefaultExtension; }
        }

        /// <summary>Gets whether this item is a page. This is used for site map purposes.</summary>
        public virtual bool IsPage
        {
            get { return true; }
        }

        /// <summary>Needs to be overridden and set to true for the code needed to match a Custom Url to kick in</summary>
        public virtual bool HasCustomUrl
        {
            get { return false; }
        }

        /// <summary>Needs to be overridden and set to true for the code needed to match a Custom Url to kick in</summary>
        public virtual bool CheckItselfForCaching
        {
            get { return true; }
        }

        /// <summary>If set to false it will stop the update going up the tree - useful when you don't want caches kicked for simple updates on large sites - cache dependencies are still easy enough to set</summary>
        public virtual bool PropogateUpdate
        {
            get { return true; }
        }

        /// <summary>Only to be used on Parents with children set to be invisible in tree</summary>
        public virtual bool IgnoreOrderOnSave
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the public url to this item. This is computed by walking the 
        /// parent path and prepending their names to the url.
        /// </summary>
        public virtual string Url
        {
            get
            {
                if (_url == null)
                {
                    _url = Context.Current.UrlParser.BuildUrl(this);
                }
                return _url;
            }
        }

        public string HierarchicalTitle
        {
            get
            {
                string result = this.Title;
                if (Parent != null)
                    result = Parent.HierarchicalTitle + " - " + result;
                return result;
            }
        }

        /// <summary>Gets the icon of this item. This can be used to distinguish item types in edit mode.</summary>
        public virtual string IconUrl
        {
            get { return Utility.GetCooliteIconUrl(Icon); }
        }

        protected virtual Icon Icon
        {
            get { return (IsPage) ? Icon.Page : Icon.PageWhite; }
        }

        /// <summary>The logical path to the node from the root node.</summary>
        public string Path
        {
            get
            {
                string path = "/";
                var startingParent = Parent;
                if (startingParent != null)
                    path += Name;
                for (var item = startingParent; item != null && item.Parent != null; item = item.Parent)
                    path = "/" + item.Name + path;
                return path;
            }
        }

        #endregion

        /// <summary>Gets an array of roles allowed to read this item. Null or empty list is interpreted as this item has no access restrictions (anyone may read).</summary>
        [BsonIgnore]
        public virtual IList<AuthorizationRule> AuthorizationRules
        {
            get
            {
                if (_authorizationRules == null)
                    _authorizationRules = new List<AuthorizationRule>();
                return _authorizationRules;
            }
            set { _authorizationRules = value; }
        }

        /// <summary>Gets an array of language settings for this item. Null or empty list is interpreted as this item inheriting its settings from its parent.</summary>
        [BsonIgnore]
        public virtual IList<LanguageSetting> LanguageSettings
        {
            get
            {
                if (_languageSettings == null)
                    _languageSettings = new List<LanguageSetting>();
                return _languageSettings;
            }
            set { _languageSettings = value; }
        }

        #region this[]

        /// <summary>Gets or sets the detail or property with the supplied name. If a property with the supplied name exists this is always returned in favour of any detail that might have the same name.</summary>
        /// <param name="detailName">The name of the propery or detail.</param>
        /// <returns>The value of the property or detail. If now property exists null is returned.</returns>
        [BsonIgnore]
        public virtual object this[string detailName]
        {
            get
            {
                if (detailName == null)
                    throw new ArgumentNullException("detailName");

                switch (detailName)
                {
                    case "ID":
                        return ID;
                    case "Title":
                        return Title;
                    case "Name":
                        return Name;
                    case "Url":
                        return Url;
                    default:
                        return Utility.Evaluate(this, detailName)
                            ?? GetDetail(detailName)
                            ?? GetDetailCollection(detailName, false);
                }
            }
            set
            {
                if (string.IsNullOrEmpty(detailName))
                    throw new ArgumentNullException("detailName", "Parameter 'detailName' cannot be null or empty.");

                PropertyInfo info = GetType().GetProperty(detailName);
                if (info != null && info.CanWrite)
                {
                    if (value != null && info.PropertyType != value.GetType())
                        value = Utility.Convert(value, info.PropertyType);
                    info.SetValue(this, value, null);
                }
                else if (value is PropertyCollection)
                {
                    DetailCollections[detailName] = (PropertyCollection)value;
                    //throw new ZeusException("Cannot set a detail collection this way, add it to the DetailCollections collection instead.");
                }
                else
                {
                    SetDetail(detailName, value);
                }
            }
        }

        #endregion
        
        protected ContentItem()
        {
            Created = DateTime.Now;
            Updated = DateTime.Now;
            Published = DateTime.Now;
            Visible = true;
            Version = 1;
        }

        #region GetDetail & SetDetail<T> Methods

        /// <summary>Gets a detail from the details bag.</summary>
        /// <param name="detailName">The name of the value to get.</param>
        /// <returns>The value stored in the details bag or null if no item was found.</returns>
        public virtual object GetDetail(string detailName)
        {
            var source = Details;
            lock (source)
            {
                var details = new Dictionary<string, PropertyData>(Details);

                return details.ContainsKey(detailName)
                    ? details[detailName].Value
                    : null;
            }
        }

        /// <summary>Gets a detail from the details bag.</summary>
        /// <param name="detailName">The name of the value to get.</param>
        /// <param name="defaultValue">The default value to return when no detail is found.</param>
        /// <returns>The value stored in the details bag or null if no item was found.</returns>
        public virtual T GetDetail<T>(string detailName, T defaultValue)
        {
            var source = Details;
            lock (source)
            {
                var details = new Dictionary<string, PropertyData>(Details);

                if (details.ContainsKey(detailName))
                    return Utility.Convert<T>(details[detailName].Value);

                return defaultValue;
            }
        }

        public virtual void SetDetail(string detailName, object value)
        {
            if (string.IsNullOrEmpty(detailName))
                throw new ArgumentNullException("detailName");
            
            lock (_details)
            {
                PropertyData detail = Details.ContainsKey(detailName) ? Details[detailName] : null;

                if (detail != null && value != null && value.GetType().IsAssignableFrom(detail.ValueType))
                {
                    // update an existing detail
                    detail.Value = value;
                }
                else
                {
                    if (detail != null)
                        // delete detail or remove detail of wrong type
                        Details.Remove(detailName);
                    if (value != null)
                    {
                        // add new detail
                        PropertyData propertyData = Context.ContentTypes.GetContentType(GetType()).GetProperty(detailName, value).CreatePropertyData(this, value);
                        Details.Add(detailName, propertyData);
                    }
                }
            }
        }

        /// <summary>Set a value into the <see cref="Details"/> bag. If a value with the same name already exists it is overwritten. If the value equals the default value it will be removed from the details bag.</summary>
        /// <param name="detailName">The name of the item to set.</param>
        /// <param name="value">The value to set. If this parameter is null or equal to defaultValue the detail is removed.</param>
        /// <param name="defaultValue">The default value. If the value is equal to this value the detail will be removed.</param>
        protected virtual void SetDetail<T>(string detailName, T value, T defaultValue)
        {
            if (value == null || !value.Equals(defaultValue))
                SetDetail(detailName, value);
            else if (Details.ContainsKey(detailName))
                Details.Remove(detailName);
        }

        /// <summary>Set a value into the <see cref="Details"/> bag. If a value with the same name already exists it is overwritten.</summary>
        /// <param name="detailName">The name of the item to set.</param>
        /// <param name="value">The value to set. If this parameter is null the detail is removed.</param>
        protected virtual void SetDetail<T>(string detailName, T value)
        {
            SetDetail(detailName, (object)value);
        }

        #endregion

        #region GetDetailCollection

        /// <summary>Gets a named detail collection.</summary>
        /// <param name="collectionName">The name of the detail collection to get.</param>
        /// <param name="createWhenEmpty">Wether a new collection should be created if none exists. Setting this to false means null will be returned if no collection exists.</param>
        /// <returns>A new or existing detail collection or null if the createWhenEmpty parameter is false and no collection with the given name exists..</returns>
        public virtual PropertyCollection GetDetailCollection(string collectionName, bool createWhenEmpty)
        {
            if (DetailCollections.ContainsKey(collectionName))
                return DetailCollections[collectionName];
            else if (createWhenEmpty)
            {
                PropertyCollection collection = new PropertyCollection(this, collectionName);
                DetailCollections.Add(collectionName, collection);
                return collection;
            }
            else
                return null;
        }

        #endregion

        #region Methods

        private const int SORT_ORDER_THRESHOLD = 9999;

        /// <summary>Adds an item to the children of this item updating it's parent refernce.</summary>
        /// <param name="newParent">The new parent of the item. If this parameter is null the item is detached from the hierarchical structure.</param>
        public virtual void AddTo(ContentItem newParent)
        {
            if (Parent != null && Parent != newParent && Parent.Children.Contains(this))
                Parent.Children.Remove(this);

            Parent = newParent;

            //see if we care about ordering...
            if (newParent != null && !newParent.Children.Contains(this))
            {
                IList<ContentItem> siblings = newParent.Children;
                if (siblings.Count > 0 && !newParent.IgnoreOrderOnSave)
                {
                    int lastOrder = siblings[siblings.Count - 1].SortOrder;

                    for (int i = siblings.Count - 2; i >= 0; i--)
                    {
                        if (siblings[i].SortOrder < lastOrder - SORT_ORDER_THRESHOLD)
                        {
                            siblings.Insert(i + 1, this);
                            return;
                        }
                        lastOrder = siblings[i].SortOrder;
                    }

                    if (lastOrder > SORT_ORDER_THRESHOLD)
                    {
                        siblings.Insert(0, this);
                        return;
                    }
                }
                
                siblings.Add(this);
            }
        }

        public virtual ContentItem Clone(bool includeChildren)
        {
            return Clone(includeChildren, false);
        }

        /// <summary>Creats a copy of this item including details, authorization rules, and language settings, while resetting ID.</summary>
        /// <param name="includeChildren">Specifies whether this item's child items also should be cloned.</param>
        /// <param name="includeTranslations"></param>
        /// <returns>The cloned item with or without cloned child items.</returns>
        public virtual ContentItem Clone(bool includeChildren, bool includeTranslations)
        {
            ContentItem cloned = (ContentItem)MemberwiseClone();
            cloned.ID = 0;
            cloned._url = null;

            CloneDetails(cloned);
            CloneChildren(includeChildren, cloned);
            CloneTranslations(includeTranslations, cloned);
            CloneAuthorizationRules(cloned);
            CloneLanguageSettings(cloned);

            return cloned;
        }

        #region Clone Helper Methods

        private void CloneAuthorizationRules(ContentItem cloned)
        {
            if (AuthorizationRules != null)
            {
                cloned.AuthorizationRules = new List<AuthorizationRule>();
                foreach (AuthorizationRule rule in AuthorizationRules)
                {
                    AuthorizationRule clonedRule = rule.Clone();
                    clonedRule.EnclosingItem = cloned;
                    cloned.AuthorizationRules.Add(clonedRule);
                }
            }
        }

        private void CloneLanguageSettings(ContentItem cloned)
        {
            if (LanguageSettings != null)
            {
                cloned.LanguageSettings = new List<LanguageSetting>();
                foreach (LanguageSetting languageSetting in LanguageSettings)
                {
                    LanguageSetting clonedLanguageSetting = languageSetting.Clone();
                    clonedLanguageSetting.EnclosingItem = cloned;
                    cloned.LanguageSettings.Add(clonedLanguageSetting);
                }
            }
        }

        private void CloneChildren(bool includeChildren, ContentItem cloned)
        {
            cloned.Children = new List<ContentItem>();
            if (includeChildren)
                foreach (ContentItem child in Children)
                {
                    ContentItem clonedChild = child.Clone(true);
                    clonedChild.AddTo(cloned);
                }
        }

        private void CloneTranslations(bool includeTranslations, ContentItem cloned)
        {
            cloned.Translations = new List<ContentItem>();
            if (includeTranslations)
                foreach (ContentItem translation in Translations)
                {
                    ContentItem clonedTranslation = translation.Clone(true);
                    clonedTranslation.AddTo(cloned);
                }
        }

        private void CloneDetails(ContentItem cloned)
        {
            cloned.Details = new Dictionary<string, PropertyData>();
            foreach (var detail in Details.Values)
                cloned[detail.Name] = detail.Value;

            cloned.DetailCollections = new Dictionary<string, PropertyCollection>();
            foreach (PropertyCollection collection in DetailCollections.Values)
            {
                PropertyCollection clonedCollection = collection.Clone();
                clonedCollection.EnclosingItem = cloned;
                cloned.DetailCollections[collection.Name] = clonedCollection;
            }
        }

        #endregion

        public TAncestor FindFirstAncestor<TAncestor>()
            where TAncestor : ContentItem
        {
            return FindFirstAncestorRecursive<TAncestor>(this);
        }

        private static TAncestor FindFirstAncestorRecursive<TAncestor>(ContentItem contentItem)
            where TAncestor : ContentItem
        {
            if (contentItem == null)
                return null;

            if (contentItem is TAncestor)
                return (TAncestor)contentItem;

            return FindFirstAncestorRecursive<TAncestor>(contentItem.Parent);
        }

        /// <summary>
        /// Gets child items that the user is allowed to access.
        /// It doesn't have to return the same collection as
        /// the Children property.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ContentItem> GetChildren()
        {
            return GetChildrenInternal().Authorized(HttpContext.Current.User, Context.SecurityManager, Operations.Read);
        }

        public virtual IEnumerable<T> GetChildren<T>()
        {
            return GetChildrenInternal().OfType<T>();
        }

        private IEnumerable<ContentItem> GetChildrenInternal() => Children;

        public virtual PathData FindPath(string remainingUrl)
        {
            remainingUrl = remainingUrl?.TrimStart('/') ?? "";
            PathData data = null;

            var cachedData = HttpContext.Current.Cache.GetOrAdd($"pathdata.{ID}_{remainingUrl}", () =>
            {
                var itemIds = new List<int>();
                data = FindPathInternal(remainingUrl, ref itemIds);
                return new CacheEntry<PathData>(new PathData(data), Context.Current.Cache.Dependencies.ForItem(itemIds.Distinct().ToArray()));
            });

            if (data == null && cachedData != null)
            {
                data = cachedData.Attach(Context.Current.Persister);
            }

            return data;
        }

        internal PathData FindPathInternal(string remainingUrl, ref List<int> itemIds)
        {
            itemIds.Add(ID);

            if (remainingUrl == null || remainingUrl.Length == 0)
                return GetTemplate(string.Empty);

            int slashIndex = remainingUrl.IndexOf('/');
            string nameSegment = slashIndex < 0 ? remainingUrl : remainingUrl.Substring(0, slashIndex);

            foreach (ContentItem child in GetChildrenInternal())
            {
                if (child.Equals(nameSegment))
                {
                    remainingUrl = slashIndex < 0 ? null : remainingUrl.Substring(slashIndex + 1);
                    return child.FindPathInternal(remainingUrl, ref itemIds);
                }
            }

            return GetTemplate(remainingUrl);
        }

        private PathData GetTemplate(string remainingUrl)
        {
            IPathFinder[] finders = PathDictionary.GetFinders(GetType());

            foreach (IPathFinder finder in finders)
            {
                PathData data = finder.GetPath(this, remainingUrl);
                if (data != null)
                    return data;
            }

            return PathData.Empty;
        }

        /// <summary>
        /// Checks whether this content item contains any properties or property collections.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsEmpty()
        {
            return string.IsNullOrEmpty(Title) && !Details.Any() && !DetailCollections.Any();
        }

        /// <summary>
        /// Translations don't have their Parent object set, so this is an abstraction to allow
        /// translations to act as normal content items.
        /// </summary>
        /// <returns></returns>
        public virtual ContentItem GetParent() => Parent;

        /// <summary>
        /// Tries to get a child item with a given name. This method ignores
        /// user permissions and any trailing '.aspx' that might be part of
        /// the name.
        /// </summary>
        /// <param name="childName">The name of the child item to get.</param>
        /// <returns>The child item if it is found otherwise null.</returns>
        /// <remarks>If the method is passed an empty or null string it will return itself.</remarks>
        public virtual ContentItem GetChild(string childName)
        {
            if (string.IsNullOrEmpty(childName))
                return null;

            int slashIndex = childName.IndexOf('/');
            if (slashIndex == 0) // starts with slash
            {
                if (childName.Length == 1)
                    return this;
                return GetChild(childName.Substring(1));
            }

            if (slashIndex > 0) // contains a slash further down
            {
                string nameSegment = childName.Substring(0, slashIndex);
                foreach (ContentItem child in GetChildrenInternal())
                    if (child.Equals(nameSegment))
                        return child.GetChild(childName.Substring(slashIndex));
                return null;
            }

            // no slash, only a name
            foreach (ContentItem child in GetChildrenInternal())
                if (child.Equals(childName))
                    return child;

            return null;
        }

        public override bool Equals(object obj)
        {
            if (this == obj) return true;
            if ((obj == null) || (obj.GetType() != GetType())) return false;
            ContentItem item = obj as ContentItem;
            if (ID != 0 && item.ID != 0)
                return ID == item.ID;
            else
                return ReferenceEquals(this, item);
        }

        /// <summary>Gets a hash code based on the item's id.</summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        protected virtual bool Equals(string name)
        {
            if (Name == null)
                return false;
            return Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
                || HttpUtility.UrlDecode(Name).Equals(name, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>Gets wether a certain user is authorized to view this item.</summary>
        /// <param name="user">The user to check.</param>
        /// <param name="operation"></param>
        /// <returns>True if the item is open for all or the user has the required permissions.</returns>
        public virtual bool IsAuthorized(IPrincipal user, string operation)
        {
            if (AuthorizationRules == null || AuthorizationRules.Count == 0)
                return true;

            // Iterate rules to find a rule that matches
            foreach (AuthorizationRule auth in AuthorizationRules)
                if (auth.IsAuthorized(user, operation))
                    return true;

            return false;
        }

        public virtual bool IsPublished()
        {
            return (Published != null && Published.Value <= DateTime.Now)
                && !(Expires != null && Expires.Value < DateTime.Now);
        }

        public virtual bool HasMinRequirementsForSaving()
        {
            return true;
        }

        public virtual bool IsVisibleInTree
        {
            get
            {
                return ((Context.ContentTypes[GetType()].Visibility & AdminSiteTreeVisibility.Visible) == AdminSiteTreeVisibility.Visible)
                    && (Parent == null || (Context.ContentTypes[Parent.GetType()].Visibility & AdminSiteTreeVisibility.ChildrenHidden) != AdminSiteTreeVisibility.ChildrenHidden);
            }
        }

        /// <summary>
        /// Return something other than null to group items in the admin site tree.
        /// </summary>
        /// <returns></returns>
        public virtual string FolderPlacementGroup
        {
            get { return null; }
        }

        #endregion

        #region INode Members

        string INode.PreviewUrl
        {
            get
            {
                if (IsPage)
                    return Url;
                return Context.Current.Resolve<IEmbeddedResourceManager>().GetServerResourceUrl(Context.Current.Resolve<IAdminAssemblyManager>().Assembly, "Zeus.Admin.View.aspx") + "?selected=" + Path;
            }
        }

        string INode.ClassNames
        {
            get
            {
                StringBuilder className = new StringBuilder();

                if (!Published.HasValue || Published > DateTime.Now)
                    className.Append("unpublished ");
                else if (Published > DateTime.Now.AddDays(-1))
                    className.Append("day ");
                else if (Published > DateTime.Now.AddDays(-7))
                    className.Append("week ");
                else if (Published > DateTime.Now.AddMonths(-1))
                    className.Append("month ");

                if (Expires.HasValue && Expires <= DateTime.Now)
                    className.Append("expired ");

                if (!Visible)
                    className.Append("invisible ");

                if (AuthorizationRules != null && AuthorizationRules.Count > 0)
                    className.Append("locked ");

                return className.ToString();
            }
        }

        #endregion

        #region ILink Members

        string ILink.Contents
        {
            get { return Title; }
        }

        string ILink.ToolTip
        {
            get { return string.Empty; }
        }

        string ILink.Target
        {
            get { return string.Empty; }
        }

        #endregion
    }
}
