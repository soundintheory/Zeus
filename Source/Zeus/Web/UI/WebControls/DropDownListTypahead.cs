using Ext.Net;
using System;
using System.Web.UI.WebControls;

namespace Zeus.Web.UI.WebControls
{
	public class DropDownListTypahead : ListControl
	{

		public new bool RequiresDataBinding
		{
			get { return (bool) (ViewState["RequiresDataBinding"] ?? false); }
			set { ViewState["RequiresDataBinding"] = value; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (this.RequiresDataBinding && !this.Page.IsPostBack)
				this.DataBind();

			CssClass += "select-2";

			Attributes.Add("select-2-id", "select-2" + ID);

			base.OnPreRender(e);
		}

        protected override void CreateChildControls()
        {

			var js = $@"
				(function($)
				{{
					$(document).ready(function(){{
						$('.select-2[select-2-id=""select-2{ID}""]').select2({{
							dropdownParent: $('.select-2[select-2-id=""select-2{ID}""]').closest('div')
						}});
					}})
				}})(jQuery);
				
			";
			ExtNet.ResourceManager.RegisterClientScriptBlock("Select2", js);

			base.CreateChildControls();
        }
    }
}