using NUnit.Framework;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;
using Zeus.BaseLibrary.Reflection;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Design.Displayers;
using Zeus.Design.Editors;
using Zeus.Integrity;
using Zeus.Persistence;
using Zeus.Tests.Integrity.ContentTypes;
using Zeus.Web;
using System.Configuration;
using System.Web;
using System.IO;

namespace Zeus.Tests.Integrity
{
	[TestFixture]
	public class IntegrityTests : ItemTestsBase
	{
		private IPersister persister;
		private IContentTypeManager definitions;
		private IUrlParser parser;
		private IntegrityManager integrityManger;

		private IEventRaiser moving;
		private IEventRaiser copying;
		private IEventRaiser deleting;
		private IEventRaiser saving;

		#region SetUp

		[SetUp]
		public override void SetUp()
		{
			base.SetUp();

			CreatePersister();

			parser = mocks.StrictMock<IUrlParser>();

			var typeFinder = CreateTypeFinder();
			var builder = new ContentTypeBuilder(typeFinder, new EditableHierarchyBuilder<IEditor>(),
				new AttributeExplorer<IDisplayer>(), new AttributeExplorer<IEditor>(),
				new AttributeExplorer<IContentProperty>(), new AttributeExplorer<IEditorContainer>());
			var notifier = mocks.DynamicMock<IItemNotifier>();
			mocks.Replay(notifier);
			definitions = new ContentTypeManager(builder, notifier);

            // Language manager
            Globalization.ILanguageManager languageManager = new Globalization.LanguageManager(
                persister,
                new Host(new WebRequestContext(), (Zeus.Configuration.HostSection)ConfigurationManager.GetSection("zeus/host")),
                definitions
            );

            // Mock the request
            var httpRequest = new HttpRequest("", "http://zeus-test/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContextMock = new HttpContext(httpRequest, httpResponse);
            HttpContext.Current = httpContextMock;

            // Mock the context
            Context.Current.AddComponentInstance("LanguageManager", languageManager);

            integrityManger = new IntegrityManager(
                definitions,
                parser,
                languageManager,
                (Zeus.Configuration.AdminSection)ConfigurationManager.GetSection("zeus/admin")
            );
			var enforcer = new IntegrityEnforcer(persister, integrityManger);
			enforcer.Start();
		}

		private ITypeFinder CreateTypeFinder()
		{
			Expect.Call(AssemblyFinder.GetAssemblies())
				.Return(new[] {
                    typeof (AlternativePage).Assembly,
                    typeof (Globalization.ContentTypes.Language).Assembly
                })
				.Repeat.Any();

			var typeFinder = mocks.StrictMock<ITypeFinder>();
			Expect.On(typeFinder)
				.Call(typeFinder.Find(typeof (ContentItem)))
				.Return(new[]
				        	{
				        		typeof (AlternativePage),
				        		typeof (AlternativeStartPage),
				        		typeof (Page),
				        		typeof (Root),
				        		typeof (StartPage),
				        		typeof (SubPage),
                                typeof (Globalization.ContentTypes.Language)
                            });
			mocks.Replay(typeFinder);
			return typeFinder;
		}

        private void CreatePersister()
		{
			mocks.Record();
			persister = mocks.DynamicMock<IPersister>();

			persister.ItemMoving += null;
			moving = LastCall.IgnoreArguments().Repeat.Any().GetEventRaiser();

			persister.ItemCopying += null;
			copying = LastCall.IgnoreArguments().Repeat.Any().GetEventRaiser();

			persister.ItemDeleting += null;
			deleting = LastCall.IgnoreArguments().Repeat.Any().GetEventRaiser();

			persister.ItemSaving += null;
			saving = LastCall.IgnoreArguments().Repeat.Any().GetEventRaiser();

			mocks.Replay(persister);
		}

		#endregion

		#region Move

		[Test]
		public void CanMoveItem()
		{
			var startPage = new StartPage();
			var page = new Page();
			var canMove = integrityManger.CanMove(page, startPage);
			Assert.IsTrue(canMove, "The page couldn't be moved to the destination.");
		}

		[Test]
		public void CanMoveItemEvent()
		{
			var startPage = new StartPage();
			var page = new Page();

			moving.Raise(persister, new CancelDestinationEventArgs(page, startPage));
		}

		[Test]
		public void CannotMoveItemOntoItself()
		{
			var page = new Page();
			var canMove = integrityManger.CanMove(page, page);
			Assert.IsFalse(canMove, "The page could be moved onto itself.");
		}

		[Test]
		public void CannotMoveItemOntoItselfEvent()
		{
			var page = new Page();
            Assert.Throws(typeof(DestinationOnOrBelowItselfException), () => {
                moving.Raise(persister, new CancelDestinationEventArgs(page, page));
            });
        }

		[Test]
		public void CannotMoveItemBelowItself()
		{
			var page = new Page();
			var page2 = CreateOneItem<Page>(2, "Rutger", page);

			var canMove = integrityManger.CanMove(page, page2);
			Assert.IsFalse(canMove, "The page could be moved below itself.");
		}

		[Test]
		public void CannotMoveItemBelowItselfEvent()
		{
			var page = new Page();
			var page2 = CreateOneItem<Page>(2, "Rutger", page);

            Assert.Throws(typeof(DestinationOnOrBelowItselfException), () => {
                moving.Raise(persister, new CancelDestinationEventArgs(page, page2));
            });
		}

		[Test]
		public void CannotMoveIfNameIsOccupied()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);
			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", null);

			var canMove = integrityManger.CanMove(page3, startPage);
			Assert.IsFalse(canMove, "The page could be moved even though the name was occupied.");
		}

		[Test]
		public void CannotMoveIfNameIsOccupiedEvent()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);
			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", null);

            Assert.Throws(typeof(NameOccupiedException), () => {
                moving.Raise(persister, new CancelDestinationEventArgs(page3, startPage));
            });
		}

		[Test]
		public void CannotMoveIfTypeIsntAllowed()
		{
			var startPage = new StartPage();
			var page = new Page();

			var canMove = integrityManger.CanMove(startPage, page);
			Assert.IsFalse(canMove, "The start page could be moved even though a page isn't an allowed destination.");
		}

		[Test]
		public void CannotMoveIfTypeIsntAllowedEvent()
		{
			var startPage = new StartPage();
			var page = new Page();

            Assert.Throws(typeof(NotAllowedParentException), () => {
                moving.Raise(persister, new CancelDestinationEventArgs(startPage, page));
            });
        }

		#endregion

		#region Copy

		[Test]
		public void CanCopyItem()
		{
			var startPage = new StartPage();
			var page = new Page();
			var canCopy = integrityManger.CanCopy(page, startPage);
			Assert.IsTrue(canCopy, "The page couldn't be copied to the destination.");
		}

		[Test]
		public void CanCopyItemEvent()
		{
			var startPage = new StartPage();
			var page = new Page();

			copying.Raise(persister, new CancelDestinationEventArgs(page, startPage));
		}

		[Test]
		public void CannotCopyIfNameIsOccupied()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);
			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", null);

			var canCopy = integrityManger.CanCopy(page3, startPage);
			Assert.IsFalse(canCopy, "The page could be copied even though the name was occupied.");
		}

		[Test]
		public void CannotCopyIfNameIsOccupiedEvent()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);
			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", null);

            Assert.Throws(typeof(NameOccupiedException), () => {
                copying.Raise(persister, new CancelDestinationEventArgs(page3, startPage));
            });
		}

		[Test]
		public void CannotCopyIfTypeIsntAllowed()
		{
			var startPage = new StartPage();
			var page = new Page();

			var canCopy = integrityManger.CanCopy(startPage, page);
			Assert.IsFalse(canCopy, "The start page could be copied even though a page isn't an allowed destination.");
		}

		[Test]
		public void CannotCopyIfTypeIsntAllowedEvent()
		{
			var startPage = new StartPage();
			var page = new Page();

            Assert.Throws(typeof(NotAllowedParentException), () => {
                copying.Raise(persister, new CancelDestinationEventArgs(startPage, page));
            });
		}

		#endregion

		#region Delete

		[Test]
		public void CanDelete()
		{
			var page = new Page();

			mocks.Record();
			Expect.On(parser).Call(parser.IsRootOrStartPage(page)).Return(false);
			mocks.Replay(parser);

			var canDelete = integrityManger.CanDelete(page);
			Assert.IsTrue(canDelete, "Page couldn't be deleted");

			mocks.Verify(parser);
		}

		[Test]
		public void CanDeleteEvent()
		{
			var page = new Page();

			mocks.Record();
			Expect.On(parser).Call(parser.IsRootOrStartPage(page)).Return(false);
			mocks.Replay(parser);

			deleting.Raise(persister, new CancelItemEventArgs(page));

			mocks.Verify(parser);
		}

		[Test]
		public void CannotDeleteStartPage()
		{
			var startPage = new StartPage();

			mocks.Record();
			Expect.On(parser).Call(parser.IsRootOrStartPage(startPage)).Return(true);
			mocks.Replay(parser);

			var canDelete = integrityManger.CanDelete(startPage);
			Assert.IsFalse(canDelete, "Start page could be deleted");

			mocks.Verify(parser);
		}

		[Test]
		public void CannotDeleteStartPageEvent()
		{
			var startPage = new StartPage();

			mocks.Record();
			Expect.On(parser).Call(parser.IsRootOrStartPage(startPage)).Return(true);
			mocks.Replay(parser);

            Assert.Throws(typeof(CannotDeleteRootException), () => {
                deleting.Raise(persister, new CancelItemEventArgs(startPage));
            });
			mocks.Verify(parser);
		}

		#endregion

		#region Save

		[Test]
		public void CanSave()
		{
			var startPage = new StartPage();

			var canSave = integrityManger.CanSave(startPage);
			Assert.IsTrue(canSave, "Couldn't save");
		}

		[Test]
		public void CanSaveEvent()
		{
			var startPage = new StartPage();

			saving.Raise(persister, new CancelItemEventArgs(startPage));
		}

		[Test]
		public void CannotSaveNotLocallyUniqueItem()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);

			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", startPage);

			var canSave = integrityManger.CanSave(page3);
			Assert.IsFalse(canSave, "Could save even though the item isn't the only sibling with the same name.");
		}

		[Test]
		public void LocallyUniqueItemThatWithoutNameYet()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);

			var page2 = CreateOneItem<Page>(2, null, startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", startPage);

			var isUnique = integrityManger.IsLocallyUnique("Sasha", page2);
			Assert.IsFalse(isUnique, "Shouldn't have been locally unique.");
		}

		[Test]
		public void CannotSaveNotLocallyUniqueItemEvent()
		{
			var startPage = CreateOneItem<StartPage>(1, "start", null);

			var page2 = CreateOneItem<Page>(2, "Sasha", startPage);
			var page3 = CreateOneItem<Page>(3, "Sasha", startPage);

            Assert.Throws(typeof(NameOccupiedException), () => {
                saving.Raise(persister, new CancelItemEventArgs(page3));
            });
		}

		[Test]
		public void CannotSaveUnallowedItem()
		{
			var page = CreateOneItem<Page>(1, "John", null);
			var startPage = CreateOneItem<StartPage>(2, "Leonidas", page);

			var canSave = integrityManger.CanSave(startPage);
			Assert.IsFalse(canSave, "Could save even though the start page isn't below a page.");
		}

		[Test]
		public void CannotSaveUnallowedItemEvent()
		{
			var page = CreateOneItem<Page>(1, "John", null);
			var startPage = CreateOneItem<StartPage>(2, "Leonidas", page);

            Assert.Throws(typeof(NotAllowedParentException), () => {
                saving.Raise(persister, new CancelItemEventArgs(startPage));
            });
		}

		#endregion

		#region Security

		[Test]
		public void UserCanEditAccessibleDetail()
		{
			var definition = definitions.GetContentType(typeof(Page));
			Assert.AreEqual(1,
				definition.GetEditors(SecurityUtilities.CreatePrincipal("UserNotInTheGroup", "ACertainGroup")).
				Count);
		}

		[Test]
		public void UserCannotEditInaccessibleDetail()
		{
			var definition = definitions.GetContentType(typeof(Page));
			Assert.AreEqual(0,
				definition.GetEditors(SecurityUtilities.CreatePrincipal("UserNotInTheGroup", "Administrator")).
				Count);
		}

		#endregion
	}
}