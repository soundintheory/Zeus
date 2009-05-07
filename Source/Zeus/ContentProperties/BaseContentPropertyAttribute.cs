using System;
using Zeus.Design.Displayers;
using Zeus.Design.Editors;

namespace Zeus.ContentProperties
{
	[AttributeUsage(AttributeTargets.Property)]
	public abstract class BaseContentPropertyAttribute : Attribute, IContentProperty
	{
		protected BaseContentPropertyAttribute(string title, int sortOrder)
		{
			Title = title;
			SortOrder = sortOrder;

			Shared = true;
		}

		public string Description { get; set; }
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this property is shared among all translations of a page.
		/// True if the property is shared, or false if the property is unique for each translation.
		/// </summary>
		public bool Shared { get; set; }

		public int SortOrder { get; set; }
		public string Title { get; set; }

		public virtual IDisplayer GetDefaultDisplayer()
		{
			return new LiteralDisplayerAttribute { Title = Title, Name = Name };
		}

		public virtual IEditor GetDefaultEditor()
		{
			PropertyData propertyData = (PropertyData) Activator.CreateInstance(GetPropertyDataType());
			Type propertyType = GetPropertyType();
			IEditor editor = GetDefaultEditorInternal(propertyType);
			editor.Name = Name;
			editor.PropertyType = propertyType;
			editor.Shared = Shared;

			// TODO - clean this up.
			if (editor is AbstractEditorAttribute)
				((AbstractEditorAttribute) editor).Description = Description;

			return editor;
		}

		protected abstract IEditor GetDefaultEditorInternal(Type propertyType);

		public abstract Type GetPropertyDataType();
		protected abstract Type GetPropertyType();

		public PropertyData CreatePropertyData(ContentItem enclosingItem, object value)
		{
			PropertyData propertyData = (PropertyData) Activator.CreateInstance(GetPropertyDataType());
			propertyData.Name = Name;
			propertyData.EnclosingItem = enclosingItem;
			propertyData.Value = value;
			return propertyData;
		}
	}
}