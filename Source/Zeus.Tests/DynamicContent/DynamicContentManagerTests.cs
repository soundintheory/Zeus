using System.Configuration;
using NUnit.Framework;
using Zeus.Configuration;
using Zeus.DynamicContent;

namespace Zeus.Tests.DynamicContent
{
	[TestFixture]
	public class DynamicContentManagerTests
	{
		[Test]
		public void CanRenderDynamicContent()
		{
			var configSection = ConfigurationManager.GetSection("zeus/dynamicContent") as DynamicContentSection;
			var manager = new DynamicContentManager(configSection);
			const string testString = @"Hello blah
				<span class=""mceNonEditable"" state=""3,MyPropName"">{DynamicContent:DynamicPageProperty}</span>
				Some more text";
			var result = manager.RenderDynamicContent(testString);
		}
	}
}