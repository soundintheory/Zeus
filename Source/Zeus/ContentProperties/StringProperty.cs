using System;
using Zeus.Design.Editors;

namespace Zeus.ContentProperties
{
	[PropertyDataType(typeof(string))]
	public class StringProperty : PropertyData
	{
		#region Constuctors

		public StringProperty()
		{
		}

		public StringProperty(ContentItem containerItem, string name, string value)
			: base(containerItem, name)
		{
			StringValue = value;
		}

		#endregion

		public virtual string StringValue { get; set; }

		public override PropertyDataType Type
		{
			get { return PropertyDataType.String; }
		}

		public override object Value
		{
			get { return StringValue; }
			set { StringValue = (string) value; }
		}

		public override Type ValueType
		{
			get { return typeof(string); }
		}

		public override IEditor GetDefaultEditor(string title, int sortOrder, Type propertyType, string containerName)
		{
			return new TextBoxEditorAttribute(title, sortOrder) { ContainerName = containerName };
		}

		public override string GetXhtmlValue()
		{
			return (StringValue != null) ? StringValue.Replace(Environment.NewLine, "<br />") : null;
		}
	}
}