using System;
using System.Collections.Generic;
using System.Linq;

namespace Isis.Collections.Generic
{
	public class EnumerableUtility
	{
		/// <summary>
		/// Only works if arrays contain only value types or strings
		/// </summary>
		/// <param name="array1"></param>
		/// <param name="array2"></param>
		/// <returns></returns>
		public static bool Equals<T>(IEnumerable<T> array1, IEnumerable<T> array2)
		{
			if (array1 == null && array2 == null)
				return true;

			if (array1 == null || array2 == null)
				return false;

			if (array1.Count() != array2.Count())
				return false;

			for (int i = 0, length = array1.Count(); i < length; ++i)
				if (!Equals(array1.ElementAt(i), array2.ElementAt(i)))
					return false;

			return true;
		}
	}
}