using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Collections;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace Zeus.BaseLibrary.ExtensionMethods.Linq
{
	public static class IEnumerableExtensionMethods
	{
		#region Distinct 1

		public static IEnumerable<TSource> Distinct<TSource, TResult>(
			this IEnumerable<TSource> source, Func<TSource, TResult> comparer)
		{
			return source.Distinct(new DynamicComparer<TSource, TResult>(comparer));
		}

		private class DynamicComparer<T, TResult> : IEqualityComparer<T>
		{
			private readonly Func<T, TResult> _selector;

			public DynamicComparer(Func<T, TResult> selector)
			{
				_selector = selector;
			}

			public bool Equals(T x, T y)
			{
				var result1 = _selector(x);
				var result2 = _selector(y);
				return result1.Equals(result2);
			}

			public int GetHashCode(T obj)
			{
				var result = _selector(obj);
				return result.GetHashCode();
			}
		}

		#endregion

		#region Distinct 2

		private class EqualityComparer<T> : IEqualityComparer<T>
		{
			public Func<T, T, bool> Comparer { get; internal set; }
			public Func<T, int> Hasher { get; internal set; }

			bool IEqualityComparer<T>.Equals(T x, T y)
			{
				return this.Comparer(x, y);
			}

			int IEqualityComparer<T>.GetHashCode(T obj)
			{
				// No hashing capabilities. Default to Equals(x, y).
				if (this.Hasher == null)
				{
					return 0;
				}

				return this.Hasher(obj);
			}
		}

		/// <summary>
		/// Gets distinct items by a comparer delegate.
		/// </summary>
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumeration, Func<T, T, bool> comparer)
		{
			return Distinct(enumeration, comparer, null);
		}

		/// <summary>
		/// Gets distinct items by comparer and hasher delegates (faster than only comparer).
		/// </summary>
		public static IEnumerable<T> Distinct<T>(this IEnumerable<T> enumeration, Func<T, T, bool> comparer, Func<T, int> hasher)
		{
			// Check to see that enumeration is not null
			if (enumeration == null)
			{
				throw new ArgumentNullException(nameof(enumeration));
			}

			// Check to see that comparer is not null
			if (comparer == null)
			{
				throw new ArgumentNullException(nameof(comparer));
			}

			return enumeration.Distinct(new EqualityComparer<T> { Comparer = comparer, Hasher = hasher });
		}

		#endregion

		public static bool Contains<TSource, TResult>(
			this IEnumerable<TSource> source, TResult value, Func<TSource, TResult> selector)
		{
			foreach (var sourceItem in source)
			{
				var sourceValue = selector(sourceItem);
				if (sourceValue.Equals(value))
				{
					return true;
				}
			}
			return false;
		}

		public static DataTable ToDataTable(this IEnumerable enumeration)
		{
			var dataTable = new DataTable();

			// Base the properties on the first item in the list.
			var value = enumeration.Cast<object>().FirstOrDefault();
			if (value != null)
			{
				var properties = value.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

				// Create the columns in the DataTable.
				foreach (var pi in properties)
				{
					var underlyingType = Nullable.GetUnderlyingType(pi.PropertyType);
					var columnType = underlyingType ?? pi.PropertyType;
					dataTable.Columns.Add(pi.Name, columnType);
				}

				// Populate the table.
				foreach (var item in enumeration)
				{
					var dataRow = dataTable.NewRow();
					dataRow.BeginEdit();
					foreach (var pi in properties)
					{
						if (pi.GetIndexParameters() == null || pi.GetIndexParameters().Length == 0) // exclude indexers
						{
							var propertyValue = pi.GetValue(item, null);
							dataRow[pi.Name] = propertyValue ?? DBNull.Value;
						}
					}

					dataRow.EndEdit();
					dataTable.Rows.Add(dataRow);
				}
			}

			return dataTable;
		}

		public static string Join(this IEnumerable<string> source, string separator)
		{
			return string.Join(separator, source.ToArray());
		}

		public static string Join(this IEnumerable<string> source, string separator, string prefix, string suffix)
		{
			var values = source.ToArray();
			for (int i = 0, length = values.Length; i < length; i++)
			{
				values[i] = prefix + values[i] + suffix;
			}

			return string.Join(separator, values);
		}

		public static string Join(this IEnumerable<string> source, string separator, string format)
		{
			var values = source.ToArray();
			for (int i = 0, length = values.Length; i < length; i++)
			{
				values[i] = string.Format(format, values[i]);
			}

			return string.Join(separator, values);
		}

		public static string Join<T>(this IEnumerable<T> source, Func<T, string> valueCallback, string separator)
		{
			var values = source.ToArray();
			var sb = new StringBuilder();
			for (int i = 0, length = values.Length; i < length; i++)
			{
				sb.Append(valueCallback(values[i]));
				if (i < length - 1)
				{
					sb.Append(separator);
				}
			}
			return sb.ToString();
		}

		public static IEnumerable<T> OfType<T>(this IEnumerable<T> source, Type type)
		{
			foreach (var element in source)
			{
				if (element != null && type.IsAssignableFrom(element.GetType()))
				{
					yield return element;
				}
			}
		}

		public static IEnumerable<TSource> Alternate<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
		{
			using (var e1 = first.GetEnumerator())
			using (var e2 = second.GetEnumerator())
			{
				while (e1.MoveNext() && e2.MoveNext())
				{
					yield return e1.Current;
					yield return e2.Current;
				}
			}
		}

		public static IEnumerable<SelectListItem> ToSelectListItems<TSource>(this IEnumerable<TSource> source, object defaultValue)
			where TSource : IEquatable<TSource>
		{
			return source.Select(e => new SelectListItem
			{
				Text = e.ToString(),
				Value = e.ToString(),
				Selected = e.Equals(defaultValue)
			});
		}

		public static IEnumerable<SelectListItem> ToSelectListItems<TSource>(this IEnumerable<TSource> source)
			where TSource : IEquatable<TSource>
		{
			return ToSelectListItems(source, null);
		}

		public static TSource Next<TSource>(this IEnumerable<TSource> source, TSource currentItem)
			where TSource : class
		{
			var found = false;

			foreach (var item in source)
			{
				if (found)
				{
					return item;
				}

				if (item.Equals(currentItem))
				{
					found = true;
				}
			}

			return null;
		}

		public static TSource Previous<TSource>(this IEnumerable<TSource> source, TSource currentItem)
			where TSource : class
		{
			return Previous(source, currentItem, i => true);
		}

		public static TSource Previous<TSource>(this IEnumerable<TSource> source, TSource currentItem, Func<TSource, bool> filter)
			where TSource : class
		{
			TSource holder = null;

			foreach (var item in source)
			{
				if (item.Equals(currentItem))
				{
					return holder;
				}

				holder = item;
			}

			return null;
		}
	}
}