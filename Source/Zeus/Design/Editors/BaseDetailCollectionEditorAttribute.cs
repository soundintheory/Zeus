using System.Collections.Generic;
using System.Web.UI;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Web.UI.WebControls;

namespace Zeus.Design.Editors
{
	[System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = false)]
	public abstract class BaseDetailCollectionEditorAttribute : AbstractEditorAttribute
	{
		protected BaseDetailCollectionEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
			var detailCollection = item.GetDetailCollection(Name, true);
			var detailCollectionEditor = (BaseDetailCollectionEditor) editor;

			var propertyDataToDelete = new List<PropertyData>();

			// First pass saves or creates items.
			for (var i = 0; i < detailCollectionEditor.Editors.Count; i++)
			{
				if (!detailCollectionEditor.DeletedIndexes.Contains(i))
				{
					var existingDetail = (detailCollection.Count > i) ? detailCollection.Details[i] : null;
					object newDetail;
					CreateOrUpdateDetailCollectionItem((ContentItem) item, existingDetail, detailCollectionEditor.Editors[i], out newDetail);
					if (newDetail != null)
					{
						if (existingDetail != null)
						{
							existingDetail.Value = newDetail;
						}
						else
						{
							detailCollection.Add(newDetail);
						}
					}
				}
				else
				{
					propertyDataToDelete.Add(detailCollection.Details[i]);
				}
			}

			// Do a second pass to delete the items, this is so we don't mess with the indices on the first pass.
			foreach (var propertyData in propertyDataToDelete)
			{
				detailCollection.Remove(propertyData);
			}

			return detailCollectionEditor.DeletedIndexes.Count > 0 || detailCollectionEditor.AddedEditors;
		}

		protected abstract void CreateOrUpdateDetailCollectionItem(ContentItem item,PropertyData existingDetail, Control editor, out object newDetail);
		protected abstract BaseDetailCollectionEditor CreateEditor();

		protected override Control AddEditor(Control container)
		{
			var detailCollectionEditor = CreateEditor();
			detailCollectionEditor.ID = Name;
			container.Controls.Add(detailCollectionEditor);
			return detailCollectionEditor;
		}

		protected override void DisableEditor(Control editor)
		{
			((BaseDetailCollectionEditor) editor).Enabled = false;
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			var detailCollectionEditor = (BaseDetailCollectionEditor) editor;
			var detailCollection = item.GetDetailCollection(Name, true);
			var details = new PropertyData[detailCollection.Count];
			detailCollection.CopyTo(details, 0);
			detailCollectionEditor.Initialize(details);
		}
	}
}