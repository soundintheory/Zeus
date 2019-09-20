using System;
using System.Web.UI;
using Ext.Net;
using Zeus.ContentTypes;

namespace Zeus.Design.Editors
{
	public class DateEditorAttribute : AbstractEditorAttribute
	{
		public bool IncludeTime { get; set; }

		public DateEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		public DateEditorAttribute()
		{
			
		}

		protected override void DisableEditor(Control editor)
		{
			var placeHolder = (CompositeField)editor;
			placeHolder.Items[0].Enabled = false;
			((DateField)placeHolder.Items[0]).ReadOnly = true;
			if (IncludeTime)
			{
				placeHolder.Items[1].Enabled = false;
				((TimeField)placeHolder.Items[1]).ReadOnly = true;
			}
		}

		protected override Control AddEditor(Control container)
		{
			var placeHolder = new CompositeField();

			var tb = new DateField();
			tb.ID = Name;
			if (Required)
			{
				tb.AllowBlank = false;
				tb.Cls = "required";
			}
			placeHolder.Items.Add(tb);

			if (IncludeTime)
			{
				var timeField = new TimeField();
				timeField.ID = Name + "Time";
				timeField.Width = 70;
				if (Required)
				{
					timeField.AllowBlank = false;
					timeField.Cls += " required";
				}
				placeHolder.Items.Add(timeField);
			}

			container.Controls.Add(placeHolder);
			container.Controls.Add(new LiteralControl("<br />"));

			return placeHolder;
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			var placeHolder = (CompositeField)editor;
			var tb = (DateField)placeHolder.Items[0];
			if (item[Name] != null)
			{
				tb.SelectedDate = (DateTime) item[Name];
				if (IncludeTime)
				{
					((TimeField)placeHolder.Items[1]).SelectedTime = ((DateTime)item[Name]).TimeOfDay;
				}
			}
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
			var placeHolder = (CompositeField)editor;
			var tb = (DateField)placeHolder.Items[0];
			var result = false;
			var currentDate = item[Name] as DateTime?;
			//this line was not storing a date if set to the original value - in the case of a date that original value might 
			//be DateTime.Now!!!  So put in a check 
			if ((currentDate != null && (tb.SelectedDate.Date != currentDate.Value.Date || currentDate == DateTime.Now.Date)) || currentDate == null)
			{
				currentDate = tb.SelectedDate.Date.Add((currentDate ?? DateTime.Now).TimeOfDay);
				item[Name] = currentDate;
				result = true;
			}
			if (IncludeTime)
			{
				var timeField = (TimeField) placeHolder.Items[1];
				if ((currentDate != null && timeField.SelectedTime != currentDate.Value.TimeOfDay) || currentDate == null)
				{
					var newDate = (currentDate ?? DateTime.Now).Date.Add(timeField.SelectedTime);
					item[Name] = newDate;
					result = true;
				}
			}
			return result;
		}
	}
}