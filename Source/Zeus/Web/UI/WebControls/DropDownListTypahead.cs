using Ext.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

namespace Zeus.Web.UI.WebControls
{
	public class DropDownListTypahead : DropDownList
	{
		public string PlaceholderText { get; set; }

        public string SharedOptionsKey { get; set; }

		public ListItemStub[] SharedOptions { get; set; }

        public string ExcludeOption { get; set; }

        protected override void OnPreRender(EventArgs e)
		{
			CssClass += "select-2";
			Attributes.Add("data-placeholder", PlaceholderText ?? "Please select an option");

			if (!string.IsNullOrEmpty(SharedOptionsKey))
			{
                Attributes.Add("data-options-key", SharedOptionsKey);
            }

            if (!string.IsNullOrEmpty(ExcludeOption))
            {
                Attributes.Add("data-exclude-option", ExcludeOption);
            }

            base.OnPreRender(e);
		}

        public override string SelectedValue
        {
			get => base.SelectedValue;
            set
            {
				SetLazyValue(value);
				base.SelectedValue = value;
            }
        }

        protected override bool LoadPostData(string postDataKey, NameValueCollection postCollection)
        {
            string[] values = postCollection.GetValues(postDataKey);

            if (values != null)
            {
				SetLazyValue(values[0]);
            }

            return base.LoadPostData(postDataKey, postCollection);
        }

		protected virtual void SetLazyValue(string value)
		{
			if (value == null)
			{
				return;
			}

            var item = Items.FindByValue(value);

            if (item == null && SharedOptions != null)
            {
                var sharedItem = SharedOptions.FirstOrDefault(x => x.Id == value);

                if (sharedItem != null)
                {
                    var listItem = new System.Web.UI.WebControls.ListItem { Value = sharedItem.Id, Text = sharedItem.Text };
                    listItem.Attributes.Add("data-icon", sharedItem.Icon);
                    Items.Add(listItem);
                }
            }
        }

        protected override void CreateChildControls()
        {
			if (!ExtNet.IsAjaxRequest)
			{
                var js = $@"
					(function($)
					{{
						function renderOption(option) {{
							if (option.icon) {{
								return $('<span><img class=""icon"" src=""' + option.icon + '"" /> ' + option.text + '</span>');
							}}
							if (option.element && option.element.dataset.icon) {{
								return $('<span><img class=""icon"" src=""' + option.element.dataset.icon + '"" /> ' + option.text + '</span>');
							}}
							return option.text;
						}}
						$(document).ready(function(){{
							$('.select-2').each(function(i, el) {{
								let opts = {{
									pageSize: 5,
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
								}};
								if (!!el.dataset.optionsKey && !!window.sharedOptions[el.dataset.optionsKey]) {{
									opts.ajax = {{
										transport: function (params, success, failure) {{
											let results = [];
											let offset = opts.pageSize * ((params.data.page || 1) - 1);
											window.sharedOptions[el.dataset.optionsKey].forEach((option) => {{
												let result = opts.matcher(params.data, option);
												if (result) {{
													results.push(result);
												}}
											}});
											success({{
												results: results.slice(offset, offset + opts.pageSize),
												pagination: {{ more: results.length > (offset + opts.pageSize) }}
											}});
										}}
									}};
								}}
								$(el).select2(opts);
							}})
						}})
					}})(jQuery);
				";
                ExtNet.ResourceManager.RegisterClientScriptBlock("Select2", js);

                if (!string.IsNullOrEmpty(SharedOptionsKey) && !ExtNet.ResourceManager.IsClientScriptBlockRegistered(SharedOptionsKey))
                {
                    var sharedOptionsJs = $@"
						window.sharedOptions = window.sharedOptions || {{}};
						window.sharedOptions['{SharedOptionsKey}'] = {JsonConvert.SerializeObject(SharedOptions)};
					";
                    ExtNet.ResourceManager.RegisterClientScriptBlock(SharedOptionsKey, sharedOptionsJs);
                }
            }

			base.CreateChildControls();
        }
    }

	public class ListItemStub
	{
		[JsonProperty("id")]
		public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }
	}
}