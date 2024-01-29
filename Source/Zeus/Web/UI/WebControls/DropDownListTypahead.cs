using Ext.Net;
using System;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using System.Web;

namespace Zeus.Web.UI.WebControls
{
	public class DropDownListTypahead : DropDownList
	{
		public string PlaceholderText { get; set; }

		protected override void OnPreRender(EventArgs e)
		{
			CssClass += "select-2";
			Attributes.Add("data-placeholder", PlaceholderText ?? "Please select an option");

			base.OnPreRender(e);
		}

        protected override void CreateChildControls()
        {
			var js = $@"
				(function($)
				{{
					$(document).ready(function(){{
						$('.select-2').select2({{
							allowClear: true,
							matcher: function(params, data) {{
								if (!params.term || $.trim(params.term) === '') {{
								  return data;
								}}
								if (!data.text) {{
								  return null;
								}}

								if (!params.splitTerms) {{
									params.splitTerms = params.term.toLowerCase().split(' ').filter(function(term) {{ return !!$.trim(term); }});
								}}
								
								for (var i = 0; i < params.splitTerms.length; i++) {{
									if (data.text.toLowerCase().indexOf(params.splitTerms[i]) > -1) {{
										return data;
									}}
								}}
								
								return null;
							}}
						}});
					}})
				}})(jQuery);
			";
			ExtNet.ResourceManager.RegisterClientScriptBlock("Select2", js);

			base.CreateChildControls();
        }
    }
}