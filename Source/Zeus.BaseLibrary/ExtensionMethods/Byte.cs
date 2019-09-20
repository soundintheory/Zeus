using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zeus.BaseLibrary.ExtensionMethods
{
	public static class ByteExtensionMethods
	{
		public static T ToDeserializedObject<T>(this byte[] array)
		{
			using (var stream = new MemoryStream(array))
			{
				var binaryFormatter = new BinaryFormatter();
				return (T) binaryFormatter.Deserialize(stream);
			}
		}
	}
}