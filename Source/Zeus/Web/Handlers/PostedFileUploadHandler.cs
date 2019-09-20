using System;
using System.IO;
using System.Web;

namespace Zeus.Web.Handlers
{
	public class PostedFileUploadHandler : BaseFileUploadHandler
	{
		public override void ProcessRequest(HttpContext context)
		{
			var postedFile = context.Request.Files["Filedata"];
			var identifier = new Guid(context.Request["identifier"]);
			var fileName = context.Server.UrlDecode(context.Request["Filename"]);

			// Work out (and create if necessary) the path to upload to.
			var uploadFolder = GetUploadFolder(identifier, true);
			var finalUploadPath = Path.Combine(uploadFolder, fileName);
			postedFile.SaveAs(finalUploadPath);

			context.Response.Write("1");
		}
	}
}