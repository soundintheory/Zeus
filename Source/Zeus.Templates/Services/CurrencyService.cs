using System.Linq;
using Zeus.Templates.ContentTypes.ReferenceData;

namespace Zeus.Templates.Services
{
	public class CurrencyService : ICurrencyService
	{
		public decimal Convert(string toIsoCode, decimal amount)
		{
			var currencyList = (CurrencyList) Find.RootItem.GetChild("system").GetChild("reference-data").GetChild("currencies");

			var baseCurrency = currencyList.BaseCurrency;
			var toCurrency = currencyList.GetChildren<Currency>().Single(c => c.IsoCode == toIsoCode);

			if (baseCurrency == toCurrency)
			{
				return amount;
			}

			return toCurrency.ExchangeRate * amount;
		}
	}
}