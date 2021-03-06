﻿using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using Ext.Net;
using Zeus.BaseLibrary.ExtensionMethods;
using Zeus.ContentTypes;

namespace Zeus.Design.Editors
{
	/// <summary>
	/// Attribute used to mark properties as editable. This attribute is predefined to use 
	/// the <see cref="System.Web.UI.WebControls.TextBox"/> web control as editor.</summary>
	/// <example>
	/// [Zeus.Details.EditableTextBox("Published", 80)]
	/// public override DateTime Published
	/// {
	///     get { return base.Published; } 
	///     set { base.Published = value; }
	/// }
	/// </example>
	[AttributeUsage(AttributeTargets.Property)]
	public class TextFieldEditorAttribute : AbstractEditorAttribute
	{
		private string _dataTypeText, _dataTypeErrorMessage;

		public TextFieldEditorAttribute()
		{
		}

		/// <summary>Initializes a new instance of the EditableTextBoxAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		public TextFieldEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		/// <summary>Initializes a new instance of the EditableTextBoxAttribute class.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		/// <param name="maxLength">The max length of the text box.</param>
		public TextFieldEditorAttribute(string title, int sortOrder, int maxLength)
			: this(title, sortOrder)
		{
			MaxLength = maxLength;
		}

		#region Properties

		/// <summary>Gets or sets columns on the text box.</summary>
		public int Columns { get; set; }

		/// <summary>Gets or sets rows on the text box.</summary>
		public int Rows { get; set; }

		/// <summary>Gets or sets the text box mode.</summary>
		public TextBoxMode TextMode { get; set; }

		/// <summary>Gets or sets the max length of the text box.</summary>
		public int MaxLength { get; set; }

		/// <summary>Gets or sets the default value. When the editor's value equals this value then null is saved instead.</summary>
		public string DefaultValue { get; set; }

		public string DataTypeText
		{
			get { return _dataTypeText ?? "&nbsp;*"; }
			set { _dataTypeText = value; }
		}

		public string DataTypeErrorMessage
		{
			get { return _dataTypeErrorMessage ?? string.Format("{0} must be a valid {1}", Title, GetDataTypeName(true)); }
			set { _dataTypeErrorMessage = value; }
		}

		public string TextBoxCssClass
		{
			get;
			set;
		}

		public bool ReadOnly { get; set; }

		#endregion

		protected override void DisableEditor(Control editor)
		{
			((TextFieldBase)editor).Enabled = false;
			((TextFieldBase)editor).ReadOnly = true;
		}

		private string GetDataTypeName(bool throwException)
		{
			Type propertyType = PropertyType.GetTypeOrUnderlyingType();
			if (propertyType == typeof(int))
				return "integer";
			if (propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float))
				return "number";
			if (propertyType == typeof(DateTime))
				return "date";
			if (throwException)
				throw new NotSupportedException();
			return string.Empty;
		}

		protected override void AddValidators(Control panel, Control editor)
		{
			base.AddValidators(panel, editor);

			// If data type is not string, we need to add a validator for data type
			Type propertyType = PropertyType.GetTypeOrUnderlyingType();
			if (propertyType == typeof(int) || propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float) || propertyType == typeof(DateTime))
				AddCompareValidator(panel, editor);
		}

		protected virtual IValidator AddCompareValidator(Control container, Control editor)
		{
			CompareValidator cmv = new CompareValidator
			{
				ID = "cmv" + Name,
				ControlToValidate = editor.ID,
				Display = ValidatorDisplay.Dynamic,
				Text = DataTypeText,
				ErrorMessage = DataTypeErrorMessage,
				Type = GetValidationDataType(),
				Operator = ValidationCompareOperator.DataTypeCheck
			};
			container.Controls.Add(cmv);

			return cmv;
		}

		private ValidationDataType GetValidationDataType()
		{
			Type propertyType = PropertyType.GetTypeOrUnderlyingType();
			if (propertyType == typeof(int))
				return ValidationDataType.Integer;
			if (propertyType == typeof(decimal) || propertyType == typeof(double) || propertyType == typeof(float))
				return ValidationDataType.Double;
			if (propertyType == typeof(DateTime))
				return ValidationDataType.Date;
			throw new NotSupportedException();
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
			TextFieldBase tb = editor as TextFieldBase;
			string value = (tb.Text == DefaultValue) ? null : tb.Text;
			if (!AreEqual(value, item[Name]))
			{
				item[Name] = value;
				return true;
			}
			return false;
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			TextFieldBase tb = editor as TextFieldBase;
			tb.Text = Utility.Convert<string>(item[Name]) ?? DefaultValue;
		}

		public override Control AddTo(Control container)
		{
			TextFieldBase textField = CreateEditor();
			textField.FieldLabel = Title;
			textField.ID = Name;
			textField.AllowBlank = !Required;
			textField.ReadOnly = ReadOnly;
			textField.Width = 500;

			if (MaxLength > 0)
				textField.MaxLength = MaxLength;

			//ModifyEditor(textField);

			if (container is FormPanel)
				((FormPanel)container).Items.Add(textField);
			else
				container.Controls.Add(textField);
			return textField;
		}

		/// <summary>Creates a text box editor.</summary>
		/// <param name="container">The container control the tetx box will be placed in.</param>
		/// <returns>A text box control.</returns>
		protected override Control AddEditor(Control container)
		{
			TextFieldBase tb = CreateEditor();
			tb.ID = Name;
			tb.CssClass += " textEditor " + TextBoxCssClass;
			if (Required)
				tb.CssClass += " required";
			tb.CssClass += " " + GetDataTypeName(false);
			if (ReadOnly)
				tb.ReadOnly = true;
			//ModifyEditor(tb);
			container.Controls.Add(tb);

			return tb;
		}

		/// <summary>Instantiates the text box control.</summary>
		/// <returns>A text box.</returns>
		protected virtual TextFieldBase CreateEditor()
		{
			if (TextMode == TextBoxMode.MultiLine)
			{
				TextArea textArea = new TextArea();
				//if (Columns > 0) textArea.Columns = Columns;
				//if (Rows > 0) textArea.Rows = Rows;
				return textArea;
			}
			return new TextField();
		}

		/*protected virtual void ModifyEditor(TextBox tb)
		{
			if (MaxLength > 0) tb.MaxLength = MaxLength;
			if (Columns > 0) tb.Columns = Columns;
			if (Rows > 0) tb.Rows = Rows;
			if (Columns > 0) tb.Rows = Rows;
			tb.TextMode = TextMode;
		}*/
	}
}