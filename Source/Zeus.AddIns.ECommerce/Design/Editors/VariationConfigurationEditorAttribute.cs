using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Zeus.AddIns.ECommerce.ContentTypes.Data;
using Zeus.AddIns.ECommerce.ContentTypes.Pages;
using Zeus.BaseLibrary.Collections.Generic;
using Zeus.BaseLibrary.ExtensionMethods.Linq;
using Zeus.ContentTypes;
using Zeus.Design.Editors;

namespace Zeus.AddIns.ECommerce.Design.Editors
{
	public class VariationConfigurationEditorAttribute : AbstractEditorAttribute
	{
		public VariationConfigurationEditorAttribute(string title, int sortOrder)
			: base(title, sortOrder)
		{

		}

		protected override void DisableEditor(Control editor)
		{
			
		}

		protected override Control AddEditor(Control container)
		{
			var checkBoxList = new CheckBoxList();
			checkBoxList.CssClass += " checkBoxList";
			checkBoxList.RepeatLayout = RepeatLayout.Flow;

			container.Controls.Add(checkBoxList);
			container.Controls.Add(new LiteralControl("<br style=\"clear:both\" />"));

			return checkBoxList;
		}

		public override bool UpdateItem(IEditableObject item, Control editor)
		{
			var checkBoxList = (CheckBoxList)editor;
			var product = (Product)item;

			// Clear any existing variation configurations.
			foreach (var variationConfiguration in product.GetChildren<VariationConfiguration>().ToArray())
				Context.Persister.Delete(variationConfiguration);

			foreach (var listItem in checkBoxList.Items.Cast<ListItem>().Where(li => li.Selected))
			{
				var variationPermutation = new VariationPermutation();
				var variationIDs = listItem.Value.Split(',').Select(s => Convert.ToInt32(s)).ToArray();
				foreach (var variationID in variationIDs)
					variationPermutation.Variations.Add(Context.Persister.Get(variationID));

				var variationConfiguration = new VariationConfiguration
				{
					Permutation = variationPermutation,
					Available = true
				};
				variationConfiguration.AddTo(product);
			}

			return true;
		}

		protected override void UpdateEditorInternal(IEditableObject item, Control editor)
		{
			// Create editors for each of the possible permutations of variations.
			var checkBoxList = (CheckBoxList) editor;
			var product = (Product) item;

			// Build possible permutations.
			var permutations = GetPermutations(product);
			if (permutations != null)
				foreach (var permutation in permutations)
				{
					var listItem = new ListItem(permutation.Join(v => v.Title, ", "), permutation.Join(v => v.ID.ToString(), ","));
					listItem.Selected = HasPermutation(product, permutation);
					checkBoxList.Items.Add(listItem);
				}
		}

		private static bool HasPermutation(Product product, IEnumerable<Variation> permutation)
		{
			return product.VariationConfigurations.Any(vc => vc.Available
				&& EnumerableUtility.Equals(vc.Permutation.Variations.Cast<Variation>(), permutation));
		}

		private static IEnumerable<IEnumerable<Variation>> GetPermutations(Product product)
		{
			var variationSets = product.CurrentCategory.Shop.VariationsSet;
			if (variationSets == null)
				return null;
			var inputVariationSets = new List<List<Variation>>();
			foreach (var set in variationSets.Sets)
			{
				var inputVariations = new List<Variation>();
				foreach (var variation in set.Variations)
					inputVariations.Add(variation);
				inputVariationSets.Add(inputVariations);
			}
			return CartesianProductUtility.Combinations(inputVariationSets[0], inputVariationSets[1]);
		}
	}
}