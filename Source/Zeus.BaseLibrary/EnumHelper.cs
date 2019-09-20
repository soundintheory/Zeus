using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Zeus.BaseLibrary.ExtensionMethods.Reflection;

namespace Zeus.BaseLibrary
{
	public static class EnumHelper
	{
		private static readonly Dictionary<string, string> _cachedEnumDescriptions = new Dictionary<string, string>();

		public static string GetEnumValueDescription(Type enumType, string name)
		{
			var cacheKey = enumType.FullName + "." + name;
			if (!_cachedEnumDescriptions.ContainsKey(cacheKey))
			{
				var memberInfo = enumType.GetMember(name);

				var description = name;
				if (memberInfo != null && memberInfo.Length > 0)
				{
					var attribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>(false, false);
					if (attribute != null)
					{
						description = attribute.Description;
					}
				}

				_cachedEnumDescriptions.Add(cacheKey, description);
			}
			return _cachedEnumDescriptions[cacheKey];
		}

		public static IEnumerable<string> GetDescriptions(Type enumType)
		{
			var names = Enum.GetNames(enumType);
			return names.Select(s => GetEnumValueDescription(enumType, s));
		}

		public static bool TryParse<T>(string stringValue, out T value)
		{
			try
			{
				value = (T) Enum.Parse(typeof (T), stringValue);
				return true;
			}
			catch (ArgumentException)
			{
				value = default(T);
				return false;
			}
		}
	}
}