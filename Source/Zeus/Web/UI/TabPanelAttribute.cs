using Ext.Net;
using Zeus.ContentTypes;
using System.Web.UI;

namespace Zeus.Web.UI
{
	public class TabPanelAttribute : EditorContainerAttribute
	{
		public string Title { get; set; }

		public string ParentID { get; set; }

        public TabPanelAttribute(string name, string title, int sortOrder)
			: base(name, sortOrder)
		{
			Title = title;
		}

        public TabPanelAttribute(string name, string title, int sortOrder, string parentId)
            : base(name, sortOrder)
        {
            Title = title;
			ParentID = parentId;
        }

        public override Control AddTo(Control container)
		{
			var panelId = ParentID ?? "TabControl";

			// If the current container doesn't already contain the tab control, create one now.
			CustomTabPanel tabControl = container.FindControl(panelId) as CustomTabPanel;

			if (tabControl == null)
			{
                var controls = container is Component component ? component.ContentControls : container.Controls;
                tabControl = new CustomTabPanel { ID = panelId };

                controls.Add(tabControl);
                controls.Add(new LiteralControl("<br />"));
            }

			Panel tabItem = new Panel
			{
				AutoScroll = true,
				AutoHeight = true,
				AutoWidth = true,
				ID = $"tabItem{panelId}{Name}",
				Title = Title,
				BodyStyle = "padding:5px"
			};
			tabControl.Items.Add(tabItem);
			return tabItem;
		}
	}

    public class TabPanelContainerAttribute : EditorContainerAttribute
    {
        public string Title { get; set; }

        public bool Collapsible { get; set; }

        public bool Collapsed { get; set; }

        public bool ShowLabel { get; set; }

        public TabPanelContainerAttribute(string name, string title, int sortOrder)
            : base(name, sortOrder)
        {
            Title = title;
        }

        public override Control AddTo(Control container)
        {
            // If the current container doesn't already contain the tab control, create one now.
            CustomTabPanel tabControl = container.FindControl(Name) as CustomTabPanel;

            if (tabControl == null)
            {
                var controls = container is Component component ? component.ContentControls : container.Controls;
                tabControl = new CustomTabPanel { ID = Name, Title = Title };

                controls.Add(tabControl);
                controls.Add(new LiteralControl("<br />"));
            }

            tabControl.Title = Title;
            tabControl.Collapsible = Collapsible;
            tabControl.Collapsed = Collapsed;
            tabControl.FieldLabel = ShowLabel ? Title : "";

            return tabControl;
        }
    }

    public class CustomTabPanel : TabPanel, INamingContainer
	{
		
	}
}
