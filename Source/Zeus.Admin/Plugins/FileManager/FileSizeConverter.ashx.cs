using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Linq;
using System.Web;
using Ext.Net;
using Zeus.Admin.Plugins.Tree;
using Zeus.ContentProperties;
using Zeus.FileSystem;
using Zeus.Linq;
using Zeus.Security;
using Zeus.Web;

namespace Zeus.Admin.Plugins.FileManager
{
	public class FileSizeConverter : IHttpHandler
	{
		private static readonly object _lockObject = new object();

		public void ProcessRequest(HttpContext context)
		{
			lock (_lockObject)
			{
				ConvertFileSizes(context);
			}
		}

		private void ConvertFileSizes(HttpContext context)
		{
			var allFiles = Context.Current.Finder.QueryItems().ToList().OfType<File>().ToList();
			var converted = 0;
			var skipped = 0;
			var already = 0;
			var batch = new List<File>();
			var batchSize = 100;

			context.Response.ContentType = "text/plain";
			context.Response.Output.WriteLine($"Found {allFiles.Count} files");

			foreach (var file in allFiles)
			{
                var newSize = file.Size;

				// This has already been converted
				if (newSize > 0)
				{
					already++;
					continue;
				}

				if (file.TryGetPropertyData("Size", out var prop) && prop is ObjectProperty oldSizeProperty)
				{
					var oldSizeValue = Utility.Convert<long?>(oldSizeProperty.Value);
					var newSizeValue = oldSizeValue ?? 0;

                    file.Size = Convert.ToInt32(newSizeValue);
					file.Details.Remove("Size");
                    oldSizeProperty.Blob = null;
					batch.Add(file);

                    context.Response.Output.WriteLine($"{file.Title}: {newSizeValue} bytes");
                    converted++;

                    if (batch.Count >= batchSize)
                    {
                        Context.Current.Persister.SaveFast(batch.ToArray());
						batch.Clear();
                    }
                }
				else
				{
                    skipped++;
				}
			}

            if (batch.Count > 0)
            {
                Context.Current.Persister.SaveFast(batch.ToArray());
            }

            context.Response.Output.WriteLine($"Converted {converted}, {already} already done, skipped {skipped}");
            context.Response.End();
        }

		public bool IsReusable
		{
			get { return false; }
		}
	}
}