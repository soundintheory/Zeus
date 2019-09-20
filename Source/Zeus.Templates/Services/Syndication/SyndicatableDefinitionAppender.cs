using Ninject;
using Zeus.ContentTypes;
using Zeus.Design.Editors;
using Zeus.Web.UI;

namespace Zeus.Templates.Services.Syndication
{
	/// <summary>
	/// Examines existing item definitions and add an editable checkbox detail 
	/// to the items implementing the <see cref="ISyndicatable" />
	/// interface.
	/// </summary>
	public class SyndicatableDefinitionAppender : IInitializable
	{
		private readonly IContentTypeManager _contentTypeManager;
		public static readonly string SyndicatableDetailName = "Syndicate";

		public SyndicatableDefinitionAppender(IContentTypeManager definitions)
		{
			_contentTypeManager = definitions;
		}

		public int SortOrder { get; set; } = 30;

		public string ContainerName { get; set; } = "Syndication";

		public string CheckBoxText { get; set; } = "Make available for syndication.";

		public void Initialize()
		{
			foreach (var contentType in _contentTypeManager.GetContentTypes())
			{
				if (typeof(ISyndicatable).IsAssignableFrom(contentType.ItemType))
				{
					var seoTab = new FieldSetAttribute("Syndication", "Syndication", 30);
					contentType.Add(seoTab);

					var ecb = new CheckBoxEditorAttribute(CheckBoxText, string.Empty, 10)
					{
						Name = SyndicatableDetailName,
						ContainerName = ContainerName,
						SortOrder = SortOrder,
						DefaultValue = true
					};

					contentType.Add(ecb);
				}
			}
		}
	}
}