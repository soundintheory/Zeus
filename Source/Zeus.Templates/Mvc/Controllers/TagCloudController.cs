using System;
using System.Linq;
using System.Web.Mvc;
using Zeus.Templates.ContentTypes.Widgets;
using Zeus.Templates.Mvc.ViewModels;
using Zeus.Templates.Services;
using Zeus.Web;

namespace Zeus.Templates.Mvc.Controllers
{
	[Controls(typeof(TagCloud), AreaName = TemplatesAreaRegistration.AREA_NAME)]
	public class TagCloudController : WidgetController<TagCloud>
	{
		private readonly ITagService _tagService;

		public TagCloudController(ITagService tagService)
		{
			_tagService = tagService;
		}

		public override ActionResult Index()
		{
			// Get active tags with their reference counts.
			var activeTagsCounts = _tagService.GetActiveTags(_tagService.GetCurrentTagGroup(CurrentItem))
				.Select(t => new { Tag = t, ReferenceCount = _tagService.GetReferenceCount(t) });

			// Get the min and max reference counts.
			var minReferenceCount = activeTagsCounts.Min(atc => atc.ReferenceCount);
			var maxReferenceCount = activeTagsCounts.Max(atc => atc.ReferenceCount);
			var logMin = Math.Log(minReferenceCount);
			var logDiff = Math.Log(maxReferenceCount) - logMin;
			var diffFontSize = CurrentItem.MaxFontSize - CurrentItem.MinFontSize;

			var tagCloudEntries = activeTagsCounts.Select(atc =>
			{
				var weight = (Math.Log(atc.ReferenceCount) - logMin)/logDiff;
				var fontSize = CurrentItem.MinFontSize + (int) Math.Round(diffFontSize*weight);
				return new TagCloudEntry(atc.Tag, fontSize);
			});

			return PartialView(new TagCloudViewModel(CurrentItem, tagCloudEntries));
		}
	}
}