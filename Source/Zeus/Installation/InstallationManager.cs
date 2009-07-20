﻿using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Isis.ApplicationBlocks.DataMigrations;
using Isis.Web.Security;
using Zeus.Configuration;
using Zeus.ContentProperties;
using Zeus.ContentTypes;
using Zeus.Installation.Migrations;
using Zeus.Persistence;
using Zeus.Serialization;
using Zeus.Web;
using AuthorizationRule=Zeus.Security.AuthorizationRule;
using PropertyCollection=System.Data.PropertyCollection;

namespace Zeus.Installation
{
	/// <summary>
	/// Wraps functionality to request database status and generate Zeus's 
	/// database schema on multiple database flavours.
	/// </summary>
	public class InstallationManager
	{
		#region Fields

		private readonly IContentTypeManager _contentTypeManager;
		private readonly Importer _importer;
		private readonly IPersister _persister;
		private readonly IFinder<ContentItem> _contentItemFinder;
		private readonly IFinder<PropertyData> _contentDetailFinder;
		private readonly IFinder<PropertyCollection> _detailCollectionFinder;
		private readonly IFinder<AuthorizationRule> _authorizationRuleFinder;
		private readonly IHost _host;
		private readonly ICredentialContextService _credentialContextService;
		private readonly AdminSection _adminConfig;

		#endregion

		#region Constructor

		public InstallationManager(IHost host, IContentTypeManager contentTypeManager, Importer importer, IPersister persister,
			IFinder<ContentItem> contentItemFinder, IFinder<PropertyData> contentDetailFinder,
			IFinder<PropertyCollection> detailCollectionFinder, IFinder<AuthorizationRule> authorizationRuleFinder,
			ICredentialContextService credentialContextService, AdminSection adminConfig)
		{
			_host = host;
			_contentTypeManager = contentTypeManager;
			_importer = importer;
			_persister = persister;
			_contentItemFinder = contentItemFinder;
			_contentDetailFinder = contentDetailFinder;
			_detailCollectionFinder = detailCollectionFinder;
			_authorizationRuleFinder = authorizationRuleFinder;
			_credentialContextService = credentialContextService;
			_adminConfig = adminConfig;
		}

		#endregion

		#region Methods

		/// <summary>Executes sql create database scripts.</summary>
		public void Install()
		{
			MigrationManager.Migrate(GetConnectionString(), typeof(Migration001).Assembly, "Zeus.Installation.Migrations");
		}

		public DatabaseStatus GetStatus()
		{
			DatabaseStatus status = new DatabaseStatus();

			UpdateConnection(status);
			UpdateSchema(status);
			UpdateItems(status);

			return status;
		}

		public void CreateAdministratorUser(string username, string password)
		{
			UserCreateStatus createStatus;
			_credentialContextService.GetCurrentService().CreateUser(username, password, new[] { _adminConfig.AdministratorRole }, out createStatus);
			if (createStatus != UserCreateStatus.Success)
				throw new ZeusException("Could not create user: " + createStatus);
		}

		public string CreateDatabase(string server, string name)
		{
			return MigrationManager.CreateDatabase(server, name);
		}

		private void UpdateItems(DatabaseStatus status)
		{
			try
			{
				status.StartPageID = _host.CurrentSite.StartPageID;
				status.RootItemID = _host.CurrentSite.RootItemID;
				status.StartPage = _persister.Get(status.StartPageID);
				status.RootItem = _persister.Get(status.RootItemID);
				status.IsInstalled = status.RootItem != null && status.StartPage != null;

				status.HasUsers = _credentialContextService.GetCurrentService().GetUser("administrator") != null;
			}
			catch (Exception ex)
			{
				status.IsInstalled = false;
				status.ItemsError = ex.Message;
			}
		}

		private void UpdateSchema(DatabaseStatus status)
		{
			try
			{
				status.Items = _contentItemFinder.Items().Count();
				status.Details = _contentDetailFinder.Items().Count();
				status.DetailCollections = _detailCollectionFinder.Items().Count();
				status.AuthorizedRoles = _authorizationRuleFinder.Items().Count();
				status.HasSchema = true;
			}
			catch (Exception ex)
			{
				status.HasSchema = false;
				status.SchemaError = ex.Message;
				status.SchemaException = ex;
			}
		}

		private void UpdateConnection(DatabaseStatus status)
		{
			try
			{
				using (IDbConnection conn = GetConnection())
				{
					conn.Open();
					conn.Close();
					status.ConnectionType = conn.GetType().Name;
				}
				status.IsConnected = true;
				status.ConnectionError = null;
			}
			catch (Exception ex)
			{
				status.IsConnected = false;
				status.ConnectionError = ex.Message;
				status.ConnectionException = ex;
			}
		}

		/// <summary>Method that will checks the database. If something goes wrong an exception is thrown.</summary>
		/// <returns>A string with diagnostic information about the database.</returns>
		public string CheckDatabase()
		{
			int itemCount = _contentItemFinder.Items().Count();
			int detailCount = _contentDetailFinder.Items().Count();
			int detailCollectionCount = _detailCollectionFinder.Items().Count();
			int authorizationRuleCount = _authorizationRuleFinder.Items().Count();

			return string.Format("Database OK, items: {0}, details: {1}, authorization rules: {2}, detail collections: {3}",
													 itemCount, detailCount, authorizationRuleCount, detailCollectionCount);
		}

		/// <summary>Checks the root node in the database. Throws an exception if there is something really wrong with it.</summary>
		/// <returns>A diagnostic string about the root node.</returns>
		public string CheckRootItem()
		{
			int rootID = _host.CurrentSite.RootItemID;
			ContentItem rootItem = _persister.Get(rootID);
			if (rootItem != null)
				return string.Format("Root node OK, id: {0}, name: {1}, type: {2}, discriminator: {3}, published: {4} - {5}",
					rootItem.ID, rootItem.Name, rootItem.GetType(),
					_contentTypeManager.GetContentType(rootItem.GetType()), rootItem.Published, rootItem.Expires);
			return "No root item found with the id: " + rootID;
		}

		/// <summary>Checks the root node in the database. Throws an exception if there is something really wrong with it.</summary>
		/// <returns>A diagnostic string about the root node.</returns>
		public string CheckStartPage()
		{
			int startID = _host.CurrentSite.StartPageID;
			ContentItem startPage = _persister.Get(startID);
			if (startPage != null)
				return string.Format("Start page OK, id: {0}, name: {1}, type: {2}, discriminator: {3}, published: {4} - {5}",
					startPage.ID, startPage.Name, startPage.GetType(),
					_contentTypeManager.GetContentType(startPage.GetType()), startPage.Published, startPage.Expires);
			return "No start page found with the id: " + startID;
		}

		public ContentItem InsertRootNode(Type type, string name, string title)
		{
			ContentItem item = _contentTypeManager.CreateInstance(type, null);
			item.Name = name;
			item.Title = title;
			_persister.Save(item);
			return item;
		}

		public ContentItem InsertStartPage(Type type, ContentItem root, string name, string title, string languageCode)
		{
			ContentItem item = _contentTypeManager.CreateInstance(type, root);
			item.Name = name;
			item.Title = title;
			item.Language = languageCode;
			_persister.Save(item);
			return item;
		}

		#region Helper Methods

		public string GetConnectionString()
		{
			return ConfigurationManager.ConnectionStrings[GetConnectionStringName()].ConnectionString;
		}

		public string GetConnectionStringName()
		{
			DatabaseSection configSection = ConfigurationManager.GetSection("zeus/database") as DatabaseSection;
			if (configSection == null)
				throw new ZeusException("Missing <zeus/database> configuration section");
			ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings[configSection.ConnectionStringName];
			if (connectionString == null)
				throw new ZeusException("Missing connection string '" + configSection.ConnectionStringName + "'");
			return configSection.ConnectionStringName;
		}

		public IDbConnection GetConnection()
		{
			return new SqlConnection(GetConnectionString());
		}

		/*public IDbConnection GetConnection()
		{
			IDriver driver = GetDriver();

			IDbConnection conn = driver.CreateConnection();
			if (Cfg.Properties.ContainsKey(Environment.ConnectionString))
				conn.ConnectionString = (string) Cfg.Properties[Environment.ConnectionString];
			else if (Cfg.Properties.ContainsKey(Environment.ConnectionStringName))
				conn.ConnectionString = ConfigurationManager.ConnectionStrings[(string) Cfg.Properties[Environment.ConnectionStringName]].ConnectionString;
			else
				throw new Exception("Didn't find a confgiured connection string or connection string name in the nhibernate configuration.");
			return conn;
		}

		public IDbCommand GenerateCommand(CommandType type, string sqlString)
		{
			IDriver driver = GetDriver();
			return driver.GenerateCommand(type, new NHibernate.SqlCommand.SqlString(sqlString), new SqlType[0]);
		}

		private IDriver GetDriver()
		{
			string driverName = (string) Cfg.Properties[Environment.ConnectionDriver];
			Type driverType = NHibernate.Util.ReflectHelper.ClassForName(driverName);
			return (IDriver) Activator.CreateInstance(driverType);
		}*/

		#endregion

		public ContentItem InsertExportFile(Stream stream, string filename)
		{
			IImportRecord record = _importer.Read(stream, filename);
			_importer.Import(record, null, ImportOptions.AllItems);

			return record.RootItem;
		}

		#endregion
	}
}
