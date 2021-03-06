using Ext.Net;
using Zeus.Integrity;
using Zeus.Templates.ContentTypes;
using Zeus.Templates.ContentTypes.ReferenceData;

namespace Zeus.AddIns.ECommerce.ContentTypes.Data
{
	[ContentType]
	[RestrictParents(typeof(ShoppingBasket))]
	public class Address : BaseContentItem
	{
		protected override Icon Icon
		{
			get { return Icon.EmailEdit; }
		}

		[ContentProperty("Title", 200)]
		public string PersonTitle
		{
			get { return GetDetail("PersonTitle", string.Empty); }
			set { SetDetail("PersonTitle", value); }
		}

		[ContentProperty("First Name", 210)]
		public string FirstName
		{
			get { return GetDetail("FirstName", string.Empty); }
			set { SetDetail("FirstName", value); }
		}

		[ContentProperty("Surname", 220)]
		public string Surname
		{
			get { return GetDetail("Surname", string.Empty); }
			set { SetDetail("Surname", value); }
		}

		[ContentProperty("Address Line One", 230)]
		public string AddressLine1
		{
			get { return GetDetail("AddressLine1", string.Empty); }
			set { SetDetail("AddressLine1", value); }
		}

		[ContentProperty("Address Line 2", 240)]
		public string AddressLine2
		{
			get { return GetDetail("AddressLine2", string.Empty); }
			set { SetDetail("AddressLine2", value); }
		}

		[ContentProperty("Town / City", 250)]
		public string TownCity
		{
			get { return GetDetail("TownCity", string.Empty); }
			set { SetDetail("TownCity", value); }
		}

		[ContentProperty("State / Region", 260)]
		public string StateRegion
		{
			get { return GetDetail("StateRegion", string.Empty); }
			set { SetDetail("StateRegion", value); }
		}

		[ContentProperty("Postcode", 270)]
		public string Postcode
		{
			get { return GetDetail("Postcode", string.Empty); }
			set { SetDetail("Postcode", value); }
		}

		[ContentProperty("Country", 280)]
		public Country Country
		{
			get { return GetDetail<Country>("Country", null); }
			set { SetDetail("Country", value); }
		}

		public string PrintAddress
		{
			get {
				return AddressLine1 + "<br/>" +
					AddressLine2 + "<br/>" +
					TownCity + "<br/>" +
					StateRegion + "<br/>" +
					Postcode + "<br/>" +
					Country.Title;
			}
			
		}
	}
}