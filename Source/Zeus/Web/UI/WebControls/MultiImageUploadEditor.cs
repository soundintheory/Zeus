namespace Zeus.Web.UI.WebControls
{
	public class MultiImageUploadEditor : MultiFileUploadEditor
	{
		#region Properties

		protected override string ItemTitle
		{
			get { return "Image"; }
		}

		#endregion

		protected override BaseFileUpload CreateEditor()
		{
			return new DropzoneImageUpload();
		}
	}
}