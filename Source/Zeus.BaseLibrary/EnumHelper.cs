using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Zeus.BaseLibrary.ExtensionMethods.Reflection;

namespace Zeus.BaseLibrary
{
	public static class EnumHelper
	{
		private static readonly ConcurrentDictionary<string, string> _cachedEnumDescriptions = new ConcurrentDictionary<string, string>();

		public static string GetEnumValueDescription(Type enumType, string name)
		{
            return (string)_cachedEnumDescriptions.GetOrAdd(enumType.FullName + "." + name, key => {

                var memberInfo = enumType.GetMember(name);

                string description = name;
                if (memberInfo != null && memberInfo.Length > 0)
                {
                    var attribute = memberInfo[0].GetCustomAttribute<DescriptionAttribute>(false, false);
                    description = attribute?.Description ?? name;
                }

                return description;
            });
		}

		public static IEnumerable<string> GetDescriptions(Type enumType)
		{
			string[] names = Enum.GetNames(enumType);
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