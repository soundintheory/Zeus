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
					function renderOption(option) {{
						if (option.element && option.element.dataset.icon) {{
							return $('<span><img class=""icon"" src=""' + option.element.dataset.icon + '"" /> ' + option.text + '</span>');
						}}
						return option.text;
					}}
					$(document).ready(function(){{
						$('.select-2').select2({{
							allowClear: true,
							templateResult: renderOption,
							templateSelection: renderOption,
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

								var matched = 0;
								
								for (var i = 0; i < params.splitTerms.length; i++) {{
									if (data.text.toLowerCase().indexOf(params.splitTerms[i]) > -1) {{
										matched++;
									}}
								}}

								if (matched > 0 && matched === params.splitTerms.length) {{
									return data;
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