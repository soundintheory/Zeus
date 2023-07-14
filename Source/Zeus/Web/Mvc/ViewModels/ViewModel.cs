using System;
using Zeus.Web.UI;
using Spark;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;

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
		}

        public virtual void Initialise()
        {
            //override this to do stuff before the base constructor!!
        }

		/// <summary>Gets the item associated with the item container.</summary>
		ContentItem IContentItemContainer.CurrentItem
		{
			get { return CurrentItem; }
		}

		public new T CurrentItem { get; set; }
	}
}