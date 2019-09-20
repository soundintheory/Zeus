using System;
using Zeus.Web.UI;
using Spark;
using System.Collections.Generic;

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
                CacheSignal signalForContentItem;
                if (_allDataSignals.TryGetValue(currentItem.CacheID, out signalForContentItem))
                {
                    _allDataSignal = signalForContentItem;
                }
                else
                {
                    //the signal doesn't exist, so add it to the list
                    _allDataSignal = new CacheSignal();
                    if (!_allDataSignals.ContainsKey(currentItem.CacheID))
					{
						_allDataSignals.Add(currentItem.CacheID, _allDataSignal);
					}
				}

                //fire changed signal if needed
                ChangeSignalFired = false;

                //check watchers
                var bWatcherChanged = false;
                if (CacheWatchers != null)
                {
                    foreach (var ci in CacheWatchers)
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
                var itemChanged = false;
                if (CurrentItem.CheckItselfForCaching)
				{
					itemChanged = (SessionVal == null) || (SessionVal != null && (System.DateTime)SessionVal != currentItem.Updated);
				}

				if (bWatcherChanged || itemChanged)
                {
                    _allDataSignal.FireChanged();
                    ChangeSignalFired = true;

                    if (itemChanged)
					{
						System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + currentItem.CacheID] = currentItem.Updated;
					}
				}
            }
		}

        public virtual void Initialise()
        {
            //override this to do stuff before the base constructor!!
        }

        public virtual List<ContentItem> CacheWatchers { get; set; }

        public bool ChangeSignalFired { get; set; }
        public static Dictionary<int, CacheSignal> _allDataSignals = new Dictionary<int, CacheSignal>();
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
                var itemChanged = (SessionVal == null) || (SessionVal != null && (System.DateTime)SessionVal != item.Updated);
                if (itemChanged)
                {
                    res.FireChanged();

                    if (itemChanged)
					{
						System.Web.HttpContext.Current.Cache["zeusChange_" + ActionForCache + "_" + item.CacheID] = item.Updated;
					}
				}
            }
            return res;
        }
	}
}