﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Zeus.BaseLibrary.ExtensionMethods.Web.UI;
using Zeus.Globalization;
using Zeus.Globalization.ContentTypes;

namespace Zeus.Admin.Plugins.Globalization.LanguageSettings
{
	public partial class Default : PreviewFrameAdminPage
	{
		#region Methods

		protected override void OnLoad(EventArgs e)
		{
			Title = "Language Settings for '" + SelectedItem.Title + "'";

			if (!IsPostBack)
			{
				chkInheritSettings.Checked = SelectedItem.LanguageSettings.Count == 0;
				chkInheritSettings.Visible = (SelectedItem.Parent != null);
				if (SelectedItem.Parent != null)
				{
					chkInheritSettings.Text = " Inherit settings from parent: '" + SelectedItem.Parent.Title + "'";
				}
			}

			hlCancel.NavigateUrl = CancelUrl();

			CreateFallbackLanguagesTable();

			base.OnLoad(e);
		}

		private void CreateFallbackLanguagesTable()
		{
			CreateHeaderRow();
			CreateRows();
		}

		private void CreateHeaderRow()
		{
			var headerRow = new TableHeaderRow { CssClass = "titles" };
			headerRow.Cells.Add(new TableHeaderCell { Text = "Visitor Language" });
			headerRow.Cells.Add(new TableHeaderCell { Text = "Fallback Language" });
			tblFallbackLanguages.Rows.Add(headerRow);
		}

		private void CreateRows()
		{
			var availableLanguages = Engine.Resolve<ILanguageManager>().GetAvailableLanguages();
			foreach (var language in availableLanguages)
			{
				CreateRow(language, availableLanguages);
			}
		}

		private void CreateRow(Language language, IEnumerable<Language> availableLanguages)
		{
			var row = new TableRow();
			row.Cells.Add(new TableCell { Text = language.Title });

			var ddl = new DropDownList { ID = "ddlFallbackLanguage" + language.ID, Width = Unit.Pixel(200) };
			ddl.Items.Add(string.Empty);
			foreach (var availableLanguage in availableLanguages.Where(l => l != language))
			{
				ddl.Items.Add(new ListItem(availableLanguage.Title, availableLanguage.Name));
			}

			var languageSetting = SelectedItem.LanguageSettings.SingleOrDefault(ls => ls.Language == language.Name);
			if (languageSetting != null && !string.IsNullOrEmpty(languageSetting.FallbackLanguage))
			{
				ddl.SelectedValue = languageSetting.FallbackLanguage;
			}

			var cell = new TableCell();
			cell.Controls.Add(ddl);
			row.Cells.Add(cell);

			tblFallbackLanguages.Rows.Add(row);
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			if (chkInheritSettings.Checked)
			{
				SelectedItem.LanguageSettings.Clear();
				Engine.Persister.Save(SelectedItem);
			}
			else
			{
				var availableLanguages = Engine.Resolve<ILanguageManager>().GetAvailableLanguages();
				foreach (var language in availableLanguages)
				{
					var ddl = (DropDownList) tblFallbackLanguages.FindControl("ddlFallbackLanguage" + language.ID);
					var languageSetting = SelectedItem.LanguageSettings.SingleOrDefault(ls => ls.Language == language.Name);
					if (languageSetting == null)
					{
						languageSetting = new LanguageSetting(SelectedItem, language.Name);
						SelectedItem.LanguageSettings.Add(languageSetting);
					}
					languageSetting.FallbackLanguage = !string.IsNullOrEmpty(ddl.SelectedValue) ? ddl.SelectedValue : null;
				}

				Engine.Persister.Save(SelectedItem);
			}

			Refresh(SelectedItem, AdminFrame.Both, false);
		}

		protected override void OnPreRender(EventArgs e)
		{
			Page.ClientScript.RegisterCssResource(typeof(Default), "Zeus.Admin.Assets.Css.edit.css");
			Page.ClientScript.RegisterCssResource(typeof(Default), "Zeus.Admin.Assets.Css.view.css");
			Page.ClientScript.RegisterCssResource(typeof(Default), "Zeus.Admin.Assets.Css.globalization.css");
			base.OnPreRender(e);
		}

		#endregion
	}
}