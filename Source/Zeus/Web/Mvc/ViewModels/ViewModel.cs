using System;
using Zeus.Web.UI;
using Spark;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Zeus.Web.Mvc.ViewModels
{
	public abstract class ViewModel
	{
		public ContentItem CurrentItem
		{
			get { throw new NotSupportedException(); }
		}
	}

    public class ViewModel<T> : ViewModel, IContentItemContainer<T>
		where T : ContentItem
	{
        public ViewModel(T currentItem)
		{
            CurrentItem = currentItem;
            Initialise();

            if (currentItem == null)
            {
                //no model, so fire changes (essentially denying the page caching)
                _allDataSignal = new CacheSignal();
                _allDataSignal.FireChanged();
                ChangeSignalFired = true;
            }
            else
            {
                //set up the signal for this object
                _allDataSignal = _allDataSignals.GetOrAdd(currentItem.CacheID, _ => new CacheSignal());

                //fire changed signal if needed
                ChangeSignalFired = false;

                //check watchers
                bool bWatcherChanged = false;
                if (CacheWatchers != null)
                {
                    foreach (ContentItem ci in CacheWatchers)
                    {
                        var WatcherSessionVal = System.Web.HttpContext.Current.Cache["zeusWatchChange_" + ActionForCache + "_" + currentItem.CacheID + "_" + ci.ID];
                        if ((WatcherSessionVal == null) || (WatcherSessionVal != null && (System.DateTime)WatcherSessionVal != ci.Updated))
                        {
                            System.Web.HttpContext.Current.Cache["zeusWatchChange_" + ActionForCache + "_" + currentItem.CacheID + "_" + ci.ID] = ci.Updated;
                            bWatcherChanged = true;
                        }
                    }
                }

                var SessionVal = System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + currentItem.CacheID];
                
                //check itself
                bool itemChanged = false;
                if (CurrentItem.CheckItselfForCaching)
                    itemChanged = (SessionVal == null) || (SessionVal != null && (System.DateTime)SessionVal != currentItem.Updated);

                if (bWatcherChanged || itemChanged)
                {
                    _allDataSignal.FireChanged();
                    ChangeSignalFired = true;
                    
                    if (itemChanged)
                        System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + currentItem.CacheID] = currentItem.Updated;
                }
            }
		}

        public virtual void Initialise()
        {
            //override this to do stuff before the base constructor!!
        }

        public virtual List<ContentItem> CacheWatchers { get; set; }

        public bool ChangeSignalFired { get; set; }
        public static ConcurrentDictionary<int, CacheSignal> _allDataSignals = new ConcurrentDictionary<int, CacheSignal>();
        public CacheSignal _allDataSignal;

        public virtual string ActionForCache { get { return ""; } }

        public ICacheSignal GetSignalForContentID
        {
            get
            {
                return _allDataSignal;
            }
        }

		/// <summary>Gets the item associated with the item container.</summary>
		ContentItem IContentItemContainer.CurrentItem
		{
			get { return CurrentItem; }
		}

		public new T CurrentItem { get; set; }

        public ICacheSignal GetDataForItem(ContentItem item)
        {
            CacheSignal res;
            if (item == null)
            {
                //no model, so fire changes (essentially denying the page caching)
                res = new CacheSignal();
                res.FireChanged();                
            }
            else
            {
                //set up the signal for this object
                res = new CacheSignal();
                
                //check itself
                var SessionVal = System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + item.CacheID];
                bool itemChanged = (SessionVal == null) || (SessionVal != null && (System.DateTime)SessionVal != item.Updated);
                if (itemChanged)
                {
                    res.FireChanged();
                    
                    if (itemChanged)
                        System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + item.CacheID] = item.Updated;
                }
            }
            return res;
        }
	}
}