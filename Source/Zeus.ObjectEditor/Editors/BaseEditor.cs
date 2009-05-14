using System;
using System.Reflection;
using System.Security.Principal;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace Zeus.ObjectEditor.Editors
{
	public abstract class BaseEditor : IEditor, ISecurable
	{
		#region Fields

		private Label _label;
		private string _requiredText, _requiredErrorMessage;
		private string _validationText, _validationErrorMessage;

		#endregion

		#region Constructors

		/// <summary>Default/empty constructor.</summary>
		protected BaseEditor()
		{
		}

		/// <summary>Initializes a new instance of the AbstractEditableAttribute.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="sortOrder">The order of this editor</param>
		protected BaseEditor(string title, int sortOrder)
		{
			Title = title;
			SortOrder = sortOrder;
		}

		/// <summary>Initializes a new instance of the AbstractEditableAttribute.</summary>
		/// <param name="title">The label displayed to editors</param>
		/// <param name="name">The name used for equality comparison and reference.</param>
		/// <param name="sortOrder">The order of this editor</param>
		protected BaseEditor(string title, string name, int sortOrder)
		{
			Title = title;
			Name = name;
			SortOrder = sortOrder;
		}

		#endregion

		#region Properties

		/// <summary>Gets or sets roles allowed to edit this detail. This property can be set by the DetailAuthorizedRolesAttribute.</summary>
		public string[] AuthorizedRoles { get; set; }

		/// <summary>Gets or sets the name of the detail (property) on the content item's object.</summary>
		public string Name { get; set; }

		public bool Shared { get; set; }

		/// <summary>Gets or sets the order of the associated control</summary>
		public int SortOrder { get; set; }

		/// <summary>Gets or sets the label used for presentation.</summary>
		public string Title { get; set; }

		public string ContainerName { get; set; }

		public bool Required { get; set; }

		public string Description { get; set; }

		public string RequiredText
		{
			get { return _requiredText ?? "&nbsp;*"; }
			set { _requiredText = value; }
		}

		public string RequiredErrorMessage
		{
			get { return _requiredErrorMessage ?? string.Format("{0} is required", Title); }
			set { _requiredErrorMessage = value; }
		}

		/// <summary>Gets or sets whether a regular expression validator should be added.</summary>
		public bool ValidateRegularExpression { get; set; }

		/// <summary>Gets or sets the validation expression for a regular expression validator.</summary>
		public string ValidationExpression { get; set; }

		/// <summary>Gets or sets the message for the regular expression validator.</summary>
		public string ValidationMessage
		{
			get { return _validationErrorMessage ?? string.Format("{0} is invalid.", Title); }
			set { _validationErrorMessage = value; }
		}

		/// <summary>Gets or sets the text for the regular expression validator.</summary>
		public string ValidationText
		{
			get { return _validationText ?? "&nbsp;*"; }
			set { _validationText = value; }
		}

		public string EditorPrefixText { get; set; }
		public string EditorPostfixText { get; set; }

		public Type PropertyType { get; set; }

		#endregion

		#region Methods

		/// <summary>Find out whether a user has permission to edit this detail.</summary>
		/// <param name="user">The user to check.</param>
		/// <returns>True if the user has the required permissions.</returns>
		public virtual bool IsAuthorized(IPrincipal user)
		{
			if (AuthorizedRoles == null)
				return true;
			if (user == null)
				return false;

			foreach (string role in AuthorizedRoles)
				if (string.Equals(user.Identity.Name, role, StringComparison.OrdinalIgnoreCase) || user.IsInRole(role))
					return true;

			return false;
		}

		public virtual Control AddTo(Control container)
		{
			Control panel = AddPanel(container);
			_label = AddLabel(panel);
			if (!string.IsNullOrEmpty(EditorPrefixText))
				panel.Controls.Add(new LiteralControl("<span class=\"prefix\">" + EditorPrefixText + "</span>"));
			Control editor = AddEditor(panel);
			if (!string.IsNullOrEmpty(EditorPostfixText))
				panel.Controls.Add(new LiteralControl("<span class=\"postfix\">" + EditorPostfixText + "</span>"));
			if (_label != null && editor != null && !string.IsNullOrEmpty(editor.ID))
				_label.AssociatedControlID = editor.ID;
			AddValidators(panel, editor);
			if (!string.IsNullOrEmpty(Description))
				panel.Controls.Add(new LiteralControl("<span class=\"description\">" + Description.Replace("\n", "<br />") + "</span><br style=\"clear:both\" />"));

			return editor;
		}

		protected abstract void DisableEditor(Control editor);

		protected virtual void AddValidators(Control panel, Control editor)
		{
			if (Required)
				AddRequiredFieldValidator(panel, editor);
			if (ValidateRegularExpression)
				AddRegularExpressionValidator(panel, editor);
		}

		/// <summary>Adds the panel to the container. Creating this panel and adding labels and editors to it will help to avoid web controls from interfering with each other.</summary>
		/// <param name="container">The container onto which add the panel.</param>
		/// <returns>A panel that can be used to add editor and label.</returns>
		protected virtual Control AddPanel(Control container)
		{
			HtmlGenericControl detailContainer = new HtmlGenericControl("div");
			detailContainer.Attributes["class"] = "editDetail";
			container.Controls.Add(detailContainer);
			return detailContainer;
		}

		/// <summary>Adds a label with the text set to the current Title to the container.</summary>
		/// <param name="container">The container control for the label.</param>
		protected virtual Label AddLabel(Control container)
		{
			Label label = new Label { ID = "lbl" + Name, Text = Title, CssClass = "editorLabel" };
			container.Controls.Add(label);
			return label;
		}

		/// <summary>Adds the editor control to the edit panel. This method is invoked by <see cref="AddTo"/> and the editor is prepended a label and wrapped in a panel. To remove these controls also override the <see cref="AddTo"/> method.</summary>
		/// <param name="container">The container onto which to add the editor.</param>
		/// <returns>A reference to the added editor.</returns>
		protected abstract Control AddEditor(Control panel);

		protected virtual IValidator AddRequiredFieldValidator(Control panel, Control editor)
		{
			RequiredFieldValidator rfv = new RequiredFieldValidator
			{
				ID = "rfv" + Name,
				ControlToValidate = editor.ID,
				Display = ValidatorDisplay.Dynamic,
				Text = RequiredText,
				ErrorMessage = RequiredErrorMessage
			};
			panel.Controls.Add(rfv);

			return rfv;
		}

		/// <summary>Adds a regular expression validator.</summary>
		/// <param name="container">The container control for this validator.</param>
		/// <param name="editor">The editor control to validate.</param>
		protected virtual Control AddRegularExpressionValidator(Control panel, Control editor)
		{
			RegularExpressionValidator rev = new RegularExpressionValidator
     	{
     		ID = Name + "_rev",
     		ControlToValidate = editor.ID,
     		ValidationExpression = ValidationExpression,
     		Display = ValidatorDisplay.Dynamic,
     		Text = ValidationText,
     		ErrorMessage = ValidationMessage
     	};
			panel.Controls.Add(rev);

			return rev;
		}

		/// <summary>Compares two values regarding null values as equal.</summary>
		protected bool AreEqual(object editorValue, object itemValue)
		{
			return (editorValue == null && itemValue == null)
			       || (editorValue != null && editorValue.Equals(itemValue))
			       || (itemValue != null && itemValue.Equals(editorValue));
		}

		/// <summary>Updates the object with the values from the editor.</summary>
		/// <param name="object">The object to update.</param>
		/// <param name="editor">The editor control which contains values to update the object with.</param>
		/// <returns>True if the item was changed (and needs to be saved).</returns>
		public abstract bool UpdateObject(IEditableObject @object, Control editor);

		/// <summary>Updates the editor with the values from the item.</summary>
		/// <param name="item">The item that contains values to assign to the editor.</param>
		/// <param name="editor">The editor to load with a value.</param>
		public void UpdateEditor(IEditableObject item, Control editor)
		{
			UpdateEditorInternal(item, editor);
		}

		protected abstract void UpdateEditorInternal(IEditableObject item, Control editor);

		int IComparable<IContainable>.CompareTo(IContainable other)
		{
			int delta = SortOrder - other.SortOrder;
			return delta != 0 ? delta : Name.CompareTo(other.Name);
		}

		/// <summary>Compares the sort order of editable attributes.</summary>
		public int CompareTo(IEditor other)
		{
			if (SortOrder != other.SortOrder)
				return SortOrder - other.SortOrder;
			if (Title != null && other.Title != null)
				return Title.CompareTo(other.Title);
			if (Title != null)
				return -1;
			if (other.Title != null)
				return 1;
			return 0;
		}

		#endregion

		#region Equals & GetHashCode

		/// <summary>Checks another object for equality.</summary>
		/// <param name="obj">The other object to check.</param>
		/// <returns>True if the items are of the same type and have the same name.</returns>
		public override bool Equals(object obj)
		{
			IUniquelyNamed other = obj as IUniquelyNamed;
			if (other == null)
				return false;
			return (Name == other.Name);
		}

		/// <summary>Gets a hash code based on the attribute's name.</summary>
		/// <returns>A hash code.</returns>
		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		#endregion
	}
}