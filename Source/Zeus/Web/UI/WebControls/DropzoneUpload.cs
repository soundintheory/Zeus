using Ext.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Zeus.Web.UI.WebControls
{
    public class DropzoneUpload : BaseFileUpload
	{
        protected override void CreateChildControls()
		{
			_hiddenFileNameField = new HiddenField { ID = ID + "hdnFileName" };
			Controls.Add(_hiddenFileNameField);

			_hiddenIdentifierField = new HiddenField { ID = ID + "hdnIdentifier" };
			Controls.Add(_hiddenIdentifierField);

			base.CreateChildControls();
		}

		protected override void OnPreRender(EventArgs e)
		{
			var justCreated = false;

			if (string.IsNullOrEmpty(_hiddenIdentifierField.Value))
			{
				_hiddenIdentifierField.Value = Guid.NewGuid().ToString();
				justCreated = true;
			}

            var fieldId = $"dropzone-{Identifier}";
            var note = !string.IsNullOrEmpty(TypeFilterDescription) ? $@"<span class=""note"">{TypeFilterDescription}</span>" : "";

            var html = $@"
				<div class=""dropzone dropzone-single"" id=""{fieldId}"">
                    <div class=""dz-default dz-message"">
                        <button class=""dz-button"" type=""button"">Drop files or click here to upload</button>
                        {note}
                    </div>
                </div>
			";

			Controls.Add(new LiteralControl(html));

            var config = GetConfig();
            var currentFile = GetCurrentFile();
            var extraJs = "";

            if (currentFile != null)
            {
                extraJs += $@"dz.displayExistingFile({JsonConvert.SerializeObject(currentFile)}, {JsonConvert.SerializeObject(PreviewUrl)}, null, null, false);";
            }

			var startScript = $@"
                (function() {{
                    Dropzone.autoDiscover = false;
                    var config = {JsonConvert.SerializeObject(config)};
                    config.thumbnail = function(file, dataUrl) {{
                        if (dataUrl) {{
                            Dropzone.prototype.defaultOptions.thumbnail.call(this, file, dataUrl);
                        }}
                    }}

                    var dz = new Dropzone('#{fieldId}', config);
                    dz.on('addedfile', function(file) {{
                        while (this.files.length > this.options.maxFiles) {{
                            this.removeFile(this.files[0]);
                        }}
                    }});
                    dz.on('removedfile', function(file) {{
                        (document.getElementById('{_hiddenFileNameField.ClientID}') || {{}}).value = '-1';
                    }});
                    dz.on('complete', function(file) {{
                        if (!file.existing) {{
                            (document.getElementById('{_hiddenFileNameField.ClientID}') || {{}}).value = file.name;
                        }}
                    }});
                    {GetExtraJS() ?? ""}
                    {extraJs}
                    
                }})();
            ";

			if (ExtNet.IsAjaxRequest && justCreated)
			{
				ExtNet.ResourceManager.RegisterOnReadyScript(startScript);
			}
			else
			{
				ScriptManager.RegisterStartupScript(this, GetType(), ClientID + "DnDUpload", startScript, true);
			}

			base.OnPreRender(e);
		}

        protected virtual string GetExtraJS()
        {
            return "";
        }

        protected virtual Dictionary<string, object> GetConfig()
        {
            return new Dictionary<string, object>
            {
                { "url", VirtualPathUtility.ToAbsolute("~/PostedFileUpload.axd") },
                { "thumbnailMethod", "contain" },
                { "maxFiles", 1 },
                { "addRemoveLinks", true },
                { "thumbnailWidth", THUMBNAIL_WIDTH },
                { "thumbnailHeight", THUMBNAIL_HEIGHT },
                { "acceptedFiles", TypeFilter.Length > 0 ? string.Join(",", TypeFilter) : null },
                { "params", new { identifier = Identifier } },
            };
        }

        protected virtual Dictionary<string, object> GetCurrentFile()
        {
            if (string.IsNullOrEmpty(_currentFileName))
            {
                return null;
            }

            return new Dictionary<string, object>
            {
                { "name", _currentFileName },
                { "existing", true }
            };
        }
    }
}
