using System.Collections.Generic;
using System.Linq;

namespace Zeus.BaseLibrary.Collections.Generic
{
	public static class CartesianProductUtility
	{
		public static IEnumerable<IEnumerable<T>> Combinations<T>(params IEnumerable<T>[] input)
		{
			IEnumerable<IEnumerable<T>> result = new T[0][];
			foreach (var item in input)
				result = Combine(result, Combinations(item));
			return result;
		}

		private static IEnumerable<IEnumerable<T>> Combinations<T>(IEnumerable<T> input)
		{
			foreach (var item in input)
				yield return new [] { item };
		}

		public static IEnumerable<IEnumerable<T>> Combine<T>(IEnumerable<IEnumerable<T>> a, IEnumerable<IEnumerable<T>> b)
		{
			var found = false;
			foreach (var groupa in a)
			{
				found = true;
				foreach (var groupB in b)
					yield return groupa.Concat(groupB);
			}
			if (!found)
				foreach (var groupB in b)
					yield return groupB;
		}
	}
}