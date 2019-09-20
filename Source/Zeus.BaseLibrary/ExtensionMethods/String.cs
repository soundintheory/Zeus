using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

namespace Zeus.BaseLibrary.ExtensionMethods
{
	public static class StringExtensionMethods
	{
		public static bool Contains(this string thisString, string value, StringComparison comparisonType)
		{
			return thisString.IndexOf(value, comparisonType) != -1;
		}

		public static T Deserialize<T>(this string serializedValue)
		{
			return (T)Deserialize(serializedValue, typeof(T));
		}

		public static object Deserialize(this string serializedValue, Type type)
		{
			var serialisedBytes = Convert.FromBase64String(serializedValue);
			using (var stream = new MemoryStream(serialisedBytes))
			{
				var binaryFormatter = new BinaryFormatter();
				return binaryFormatter.Deserialize(stream);
			}
		}

		public static string FromBase64String(this string value)
		{
			var encoding = new ASCIIEncoding();
			var decoder = encoding.GetDecoder();
			var toDecodeBytes = Convert.FromBase64String(value);
			var charCount = decoder.GetCharCount(toDecodeBytes, 0, toDecodeBytes.Length);
			var decodedChars = new char[charCount];
			decoder.GetChars(toDecodeBytes, 0, toDecodeBytes.Length, decodedChars, 0);
			var result = new string(decodedChars);
			return result;
		}

		public static T FromXml<T>(this string xml)
		{
			var stringReader = new StringReader(xml);

			var xmlSerializer = new XmlSerializer(typeof(T));
			var result = (T)xmlSerializer.Deserialize(stringReader);

			stringReader.Close();

			return result;
		}

		public static string Left(this string value, int length)
		{
			return (length >= value.Length) ? value : value.Substring(0, length);
		}

		public static string Paragraph(this string value, int paragraphIndex)
		{
			var paragraphs = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			return paragraphs.Length > paragraphIndex ? paragraphs[paragraphIndex] : string.Empty;
		}

		public static string Paragraphs(this string value, int paragraphIndex)
		{
			var paragraphs = value.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
			if (paragraphs.Length > paragraphIndex)
			{
				return string.Join(Environment.NewLine, paragraphs, paragraphIndex, paragraphs.Length - paragraphIndex);
			}
			else
			{
				return string.Empty;
			}
		}

		public static string Right(this string value, int length)
		{
			return (length >= value.Length) ? value : value.Substring(value.Length - length, length);
		}

		public static string RemoveHtmlTags(this string value)
		{
			const string expression = "<.*?>";
			return Regex.Replace(value, expression, string.Empty);
		}

		public static string RemoveWrappingHtmlTags(this string value)
		{
			return Regex.Replace(value, @"^<.*?>(.*)<.*?>?", "$1");
		}

		public static string ToBase64String(this string value)
		{
			var encodedDataBytes = Encoding.ASCII.GetBytes(value);
			var encodedData = Convert.ToBase64String(encodedDataBytes);
			return encodedData;
		}

		public static string ToPascalCase(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			return char.ToLower(value[0]) + value.Substring(1);
		}

		public static bool IsSafeUrl(this string value)
		{
			return value == value.ToSafeUrl();
		}

		public static string ToSafeUrl(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			var temp = value.ToLower().Trim();
            //temp = temp.Replace("  ", " ").Replace("  ", " ").Trim();
			temp = Regex.Replace(temp, "[ ]+", "-");

			var result = string.Empty;
			foreach (var c in temp)
			{
				switch (CharUnicodeInfo.GetUnicodeCategory(c))
				{
					case UnicodeCategory.DecimalDigitNumber :
					case UnicodeCategory.LowercaseLetter :
						result += HttpUtility.UrlEncode(c.ToString());
						break;
					default :
						switch (c)
						{
							case '-' :
							case '_' :
								result += c;
								break;
						}
						break;
				}
			}
			return result;
		}

		public static T ToEnum<T>(this string value)
		{
			return (T)Enum.Parse(typeof(T), value);
		}

		/// <summary>Gets the type from a string.</summary>
		/// <param name="typeName">The type name string.</param>
		/// <returns>The type.</returns>
		public static Type ToType(this string typeName)
		{
			if (typeName == null)
			{
				throw new ArgumentNullException("name");
			}

			var t = Type.GetType(typeName);
			if (t == null)
			{
				throw new Exception(string.Format("Couldn't find any type with the name '{0}'", typeName));
			}

			return t;
		}

		public static string Truncate(this string value, int length)
		{
			return Truncate(value, length, true);
		}

		public static string Truncate(this string value, int length, bool preserveWordBoundaries)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}

			if (value.Length <= length)
			{
				return value;
			}

			var words = value.Split(' ');
			var currentLength = 0;
			var sb = new StringBuilder();
			for (int i = 0, arrayLength = words.Length; i < arrayLength && currentLength < length - 3; ++i)
			{
				var newEntry = words[i] + " ";
				sb.Append(newEntry);
				currentLength += newEntry.Length;
			}

			return sb.ToString() + "...";
		}

		public static int LuhnChecksum(this string value)
		{
			return value
			       	.Where(Char.IsDigit)
			       	.Reverse()
			       	.SelectMany((c, i) => ((c - '0') << (i & 1)).ToString())
			       	.Sum(c => c - '0') % 10;
		}

		/// <summary>
		/// Returns a string containing every character within a string before the 
		/// first occurrence of another string.
		/// </summary>
		/// <param name="str">Required. String expression from which the leftmost characters are returned.</param>
		/// <param name="search">The string where the beginning of it marks the 
		/// characters to return.  If the string is not found, the whole string is 
		/// returned.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string LeftBefore(this string str, string search)
		{
			return LeftBefore(str, search, StringComparison.InvariantCulture);
		}

		/// <summary>
		/// Returns a string containing every character within a string before the 
		/// first occurrence of another string.
		/// </summary>
		/// <param name="original">Required. String expression from which the leftmost characters are returned.</param>
		/// <param name="search">The string where the beginning of it marks the 
		/// characters to return.  If the string is not found, the whole string is 
		/// returned.</param>
		/// <param name="comparisonType">Determines whether or not to use case sensitive search.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string LeftBefore(this string original, string search, StringComparison comparisonType)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original), "The original string may not be null.");
			}

			if (search == null)
			{
				throw new ArgumentNullException(nameof(search), "Search string may not be null.");
			}

			//Shortcut.
			if (search.Length > original.Length || search.Length == 0)
			{
				return original;
			}

			var searchIndex = original.IndexOf(search, 0, comparisonType);

			if (searchIndex < 0)
			{
				return original;
			}

			return Left(original, searchIndex);
		}

		/// <summary>
		/// Returns a string containing every character within a string after the 
		/// first occurrence of another string.
		/// </summary>
		/// <param name="original">Required. String expression from which the rightmost characters are returned.</param>
		/// <param name="search">The string where the end of it marks the 
		/// characters to return.  If the string is not found, the whole string is 
		/// returned.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string RightAfter(this string original, string search)
		{
			return RightAfter(original, search, StringComparison.InvariantCulture);
		}

		/// <summary>
		/// Returns a string containing every character within a string after the 
		/// first occurrence of another string.
		/// </summary>
		/// <param name="original">Required. String expression from which the rightmost characters are returned.</param>
		/// <param name="search">The string where the end of it marks the 
		/// characters to return.  If the string is not found, the whole string is 
		/// returned.</param>
		/// <param name="comparisonType">Determines whether or not to use case sensitive search.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string RightAfter(this string original, string search, StringComparison comparisonType)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original), "The original string may not be null.");
			}

			if (search == null)
			{
				throw new ArgumentNullException(nameof(search), "The searchString string may not be null.");
			}

			//Shortcut.
			if (search.Length > original.Length || search.Length == 0)
			{
				return original;
			}

			var searchIndex = original.IndexOf(search, 0, comparisonType);

			if (searchIndex < 0)
			{
				return original;
			}

			return Right(original, original.Length - (searchIndex + search.Length));
		}

		/// <summary>
		/// Returns a string containing every character within a string after the 
		/// last occurrence of another string.
		/// </summary>
		/// <param name="original">Required. String expression from which the rightmost characters are returned.</param>
		/// <param name="search">The string where the end of it marks the 
		/// characters to return.  If the string is not found, the whole string is 
		/// returned.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string RightAfterLast(this string original, string search)
		{
			return RightAfterLast(original, search, original.Length - 1, StringComparison.InvariantCulture);
		}

		/// <summary>
		/// Returns a string containing every character within a string after the
		/// last occurrence of another string.
		/// </summary>
		/// <param name="original">Required. String expression from which the rightmost characters are returned.</param>
		/// <param name="search">The string where the end of it marks the
		/// characters to return.  If the string is not found, the whole string is
		/// returned.</param>
		/// <param name="startIndex">The start index.</param>
		/// <param name="comparisonType">Determines whether or not to use case sensitive search.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">Thrown if str or searchstring is null.</exception>
		public static string RightAfterLast(this string original, string search, int startIndex, StringComparison comparisonType)
		{
			if (original == null)
			{
				throw new ArgumentNullException(nameof(original), "The original string may not be null.");
			}

			if (search == null)
			{
				throw new ArgumentNullException(nameof(search), "The searchString string may not be null.");
			}

			//Shortcut.
			if (search.Length > original.Length || search.Length == 0)
			{
				return original;
			}

			var searchIndex = original.LastIndexOf(search, startIndex, comparisonType);

			if (searchIndex < 0)
			{
				return original;
			}

			return Right(original, original.Length - (searchIndex + search.Length));
		}

		public static bool IsValidEmail(this string original)
		{
			if (string.IsNullOrEmpty(original))
			{
				return false;
			}

			try
            {
                var match = Regex.Match(original, @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
                return match.Success && (match.Index == 0) && (match.Length == original.Length);
            }
            catch
            {
                return false;
            }
		}

		public static bool IsValidUKTelephoneNumber(this string original)
		{
			if (string.IsNullOrEmpty(original))
			{
				return false;
			}

			return Regex.IsMatch(original, @"^\s*\(?(020[7,8]{1}\)?[ ]?[1-9]{1}[0-9{2}[ ]?[0-9]{4})|(0[1-8]{1}[0-9]{3}\)?[ ]?[1-9]{1}[0-9]{2}[ ]?[0-9]{3})\s*$");
		}

        public static string MultilineTextToHTML(this string text)
        {
            return System.Web.HttpContext.Current.Server.HtmlEncode(text).Replace("\n", "<br/>");
        }

        /// <summary>
        /// Content cut for summaries in the lists
        /// </summary>
        /// <param name="html"></param>
        /// <param name="theString"></param>
        /// <param name="theLength"></param>
        /// <returns></returns>
        public static string SafeTruncate(this string theString, int theLength)
        {
            if (theString.Length < theLength)
            {
                return theString;
            }
            else
            {
				var newText = theString.Substring(0, theLength);

				// test whether the truncate has cut into an existing HTML tag. If it has, remove a character to newText and test again. Do this until false. 
				var isItCutXP = new Regex(@"<[^>]*$");
                while (isItCutXP.IsMatch(newText))
                {
                    theLength--;
                    newText = theString.Substring(0, theLength);
                }

                //remove images from newText
                var imagesRGX = new Regex(@"<img[^>]+>", RegexOptions.None);
                newText = imagesRGX.Replace(newText, "");

                // match all opening HTML tags (avoiding <br> tags) in newText and put in an array called 'theMatches'
                var openTagsRGX = new Regex(@"<(?!\/)(?!br)[^>]+>", RegexOptions.IgnoreCase);
                var theMatches = openTagsRGX.Matches(newText);

                // for each opening tag, create a close tag
                var theCloses = new ArrayList();
                var inTagRGX = new Regex(@"\w+");
                foreach (Match m in theMatches)
                {
                    var theTag = inTagRGX.Match(m.ToString());
                    var toAdd = "</" + theTag.ToString() + ">";
                    theCloses.Add(toAdd);
                }

                //find all currently existing close tags
                var closeTagsRGX = new Regex(@"<\/[^>]+>", RegexOptions.IgnoreCase);
                var existingCloseTags = closeTagsRGX.Matches(newText);

                //if there are any, delete matches entries in theCloses in the order in which they appear

                foreach (Match m in existingCloseTags)
                {
                    foreach (string exC in theCloses)
                    {
                        if (m.ToString() == exC)
                        {
                            theCloses.Remove(exC);
                            break;
                        }
                    }
                }
                //reverse it
                theCloses.Reverse();

                //concatentate theCloses into a string and tack it to the end of the truncated text.
                var theCloseString = new StringBuilder();
                foreach (string m in theCloses)
                {
                    theCloseString.Append(m);
                }

				return newText + "..." + theCloseString;
			}
        }
	}
}