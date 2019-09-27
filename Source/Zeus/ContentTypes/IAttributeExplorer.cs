using System;
using System.Collections.Generic;

namespace Zeus.ContentTypes
{
	public interface IAttributeExplorer<T> where T : IUniquelyNamed
	{
		IList<T> Find(Type typeToExplore);
		IDictionary<string, T> Map(Type typeToExplore);
	}
}