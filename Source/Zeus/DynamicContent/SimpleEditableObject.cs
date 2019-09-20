using System;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.ContentProperties;
using Zeus.ContentTypes;

namespace Zeus.DynamicContent
{
	public class SimpleEditableObject : IEditableObject
	{
		public SimpleEditableObject(object objectToWrap)
		{
			WrappedObject = objectToWrap;
		}

		public object this[string detailName]
		{
			get { return WrappedObject.GetValue(detailName); }
			set { WrappedObject.SetValue(detailName, value, true); }
		}

		public object WrappedObject { get; }

		public PropertyCollection GetDetailCollection(string name, bool create)
		{
			throw new NotSupportedException();
		}

		public object GetDetail(string name)
		{
			return this[name];
		}

		public void SetDetail(string name, object value)
		{
			this[name] = value;
		}
	}
}