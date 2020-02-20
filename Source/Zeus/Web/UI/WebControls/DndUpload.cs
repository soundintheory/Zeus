using Ext.Net;
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
	public class DndUpload : BaseFileUpload
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

			var html = $@"
				<div style=""float:left;width:500px;margin-bottom:10px;"" id=""upload-div"">
					<div class=""dnd-upload"" id=""upload1""></div>
				</div>
			";

			Controls.Add(new LiteralControl(html));

			if (TypeFilter.Length > 1 && !TypeFilter.Contains("*.*"))
			{
				var typeHtml = $@"
					<br />
					<div style=""float:left;width:500px;margin-bottom:10px;"">Acceptable file types: {string.Join(", ", TypeFilter)}</div>
				";
				Controls.Add(new LiteralControl(typeHtml));
			}

			var filter = TypeFilter.Select(x => $"\"{x}\"");

			var startScript = $@"window.onload = () => {{
						var uploader = new DragAndDropUpload(""upload1"", ""{_hiddenIdentifierField.Value}"", ""{_hiddenFileNameField.ClientID}"", ""{VirtualPathUtility.ToAbsolute("~/PostedFileUpload.axd")}"", ""{_currentFileName}"", [{string.Join(",", filter)}]);
					}}
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
	}
}
