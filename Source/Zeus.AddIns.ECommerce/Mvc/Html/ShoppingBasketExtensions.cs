using System.Linq;
using System.Web.Mvc;
using Zeus.AddIns.ECommerce.ContentTypes.Pages;
using Zeus.AddIns.ECommerce.Services;

namespace Zeus.AddIns.ECommerce.Mvc.Html
{
	public static class ShoppingBasketExtensions
	{
		public static string ShoppingBasketSummary(this HtmlHelper html, Shop shop)
		{
			if (shop == null)
			{
				return "[[Shop page not found]]";
			}

			var shoppingBasketPage = shop.GetChild("shopping-basket") as ShoppingBasketPage;
			if (shoppingBasketPage == null)
			{
				return "[[Shopping basket page not found]]";
			}

			var shoppingBasketService = Context.Current.Resolve<IShoppingBasketService>();
			IShoppingBasket shoppingBasket = shoppingBasketService.GetBasket(shop);
			var innerText = string.Format("You have <span>{0}</span> items in your shopping basket (<span>{1:C2}</span>)",
				shoppingBasket.TotalItemCount, shoppingBasket.SubTotalPrice);

			var linkTag = new TagBuilder("a");
			linkTag.MergeAttribute("href", shoppingBasketPage.Url, true);
			linkTag.MergeAttribute("id", "basket", true);
			linkTag.SetInnerText(innerText);
			return linkTag.ToString();
		}
	}
}