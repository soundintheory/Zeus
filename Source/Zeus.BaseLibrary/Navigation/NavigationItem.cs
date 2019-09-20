using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zeus.BaseLibrary.Navigation
{
    public class NavigationItem
    {
        public string Title;
        public string Url;
        public int ID;
        public string ParentUrl { get; set; }
        public IList<NavigationItem> SubNav { get; set; }

        public bool HasSubNav { get { return SubNav == null ? false : SubNav.Count > 0; } }

        public bool IsCurrentPage(string url)
        {
            if (url == Url)
			{
				return true;
			}
			else if (this.SubNav != null)
            {
                foreach (var subNavItem in this.SubNav)
                {
                    if (subNavItem.Url == url)
					{
						return true;
					}

					if (subNavItem.SubNav != null)
                    {
                        foreach (var tertiaryNavItem in subNavItem.SubNav)
                        {
                            if (tertiaryNavItem.Url == url)
							{
								return true;
							}

							if (tertiaryNavItem.SubNav != null)
                            {
                                foreach (var fourthLevelNavItem in tertiaryNavItem.SubNav)
                                {
                                    if (fourthLevelNavItem.Url == url)
									{
										return true;
									}
								}
                            }
                        }

                    }
                }
            }
            return false;
        }
    }
}
