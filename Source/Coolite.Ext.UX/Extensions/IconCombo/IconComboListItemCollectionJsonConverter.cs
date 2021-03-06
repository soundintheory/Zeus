using System;
using System.Text;
using Ext.Net;
using Newtonsoft.Json;

namespace Coolite.Ext.UX
{
	public class IconComboListItemCollectionJsonConverter : ListItemCollectionJsonConverter
	{
		private readonly string _iconUrlField;

		public IconComboListItemCollectionJsonConverter(string iconUrlField)
		{
			_iconUrlField = iconUrlField;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			ListItemCollection<ListItem> items = value as ListItemCollection<ListItem>;

			StringBuilder sb = new StringBuilder("new Ext.data.SimpleStore({fields:[\"text\",\"value\",\"" + _iconUrlField + "\"],data :[");
			if (items != null && items.Count > 0)
			{
				foreach (IconComboListItem item in items)
				{
					sb.Append("[");
					sb.Append(JSON.Serialize(item.Text));
					sb.Append(",");
					sb.Append(JSON.Serialize(item.Value));
					sb.Append(",");
					sb.Append(JSON.Serialize(item.IconUrl));
					sb.Append("],");
				}
				sb.Remove(sb.Length - 1, 1);
			}

			sb.Append("]})");

			writer.WriteRawValue(sb.ToString());
		}
	}
}