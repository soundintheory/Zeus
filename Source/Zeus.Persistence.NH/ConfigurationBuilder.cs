using System;
using System.Reflection;
using FluentNHibernate.Cfg.Db;
using Zeus.ContentTypes;
using System.Text;
using System.Collections.Generic;
using Zeus.Configuration;
using Zeus.Web;
using FluentNHibernate.Cfg;

namespace Zeus.Persistence.NH
{
	public class ConfigurationBuilder : IConfigurationBuilder
	{
		private readonly IContentTypeManager _definitions;

		private const string _mappingFormat = @"<?xml version=""1.0"" encoding=""utf-16""?>
<hibernate-mapping xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:nhibernate-mapping-2.2""
	auto-import=""false"">
{0}
</hibernate-mapping>
";

		private const string _classFormat = @"<subclass name=""{0}"" extends=""{1}"" discriminator-value=""{2}"" lazy=""false""/>";

		private const string _widgetClassFormat = @"<subclass name=""{0}"" extends=""{1}"" discriminator-value=""{2}"" lazy=""false"">
				<property name=""ZoneName"" access=""property"" length=""50"" />
			</subclass>";

		public NHibernate.Cfg.Configuration Configuration { get; }

		public ConfigurationBuilder(IContentTypeManager definitions, DatabaseSection databaseSectionConfig)
		{
			_definitions = definitions;

			if (databaseSectionConfig == null)
			{
				databaseSectionConfig = new DatabaseSection();
			}

			var configuration = MsSqlConfiguration.MsSql2008
                .ConnectionString(c => c.FromConnectionStringWithKey(databaseSectionConfig.ConnectionStringName));

			var properties = configuration.ToProperties();
			properties[NHibernate.Cfg.Environment.ConnectionStringName] = databaseSectionConfig.ConnectionStringName;
            properties[NHibernate.Cfg.Environment.CacheProvider] = databaseSectionConfig.CacheProviderClass;

            if (databaseSectionConfig.CacheEnabled)
            {
                properties[NHibernate.Cfg.Environment.UseQueryCache] = "true";
                properties[NHibernate.Cfg.Environment.UseSecondLevelCache] = "true";
            }

            //ZeusPersistenceModel persistenceModel = new ZeusPersistenceModel();
            //persistenceModel.Mappings.(DefaultLazy.AlwaysTrue());

            Configuration = new NHibernate.Cfg.Configuration().AddProperties(properties);

            AddMapping(Configuration, "Zeus.Persistence.NH.Mappings.Default.hbm.xml");
			//_configuration.AddAssembly(Assembly.GetExecutingAssembly());

			//persistenceModel.Configure(_configuration);

			//persistenceModel.WriteMappingsTo(@"C:\mappings");

			// For each definition, add a <subclass> element to mapping file.
			var mappings = new StringBuilder();
			foreach (var type in EnumerateDefinedTypes())
			{
				var format = (typeof(WidgetContentItem) == type) ? _widgetClassFormat : _classFormat;
				mappings.AppendFormat(format, GetName(type), GetName(type.BaseType), GetDiscriminator(type));
			}
			var configurationXml = string.Format(_mappingFormat, mappings);
			Configuration.AddXml(configurationXml);
		}

		/// <summary>Adds mappings to the configuration.</summary>
		/// <param name="cfg">The configuration to add the mappings to.</param>
		protected virtual void AddMapping(NHibernate.Cfg.Configuration cfg, string name)
		{
			if (!string.IsNullOrEmpty(name))
			{
				cfg.AddInputStream(GetType().Assembly.GetManifestResourceStream(name));
			}
		}

		private IEnumerable<Type> EnumerateDefinedTypes()
		{
			var types = new List<Type>();
			foreach (var definition in _definitions.GetContentTypes())
			{
				foreach (var t in EnumerateBaseTypes(definition.ItemType))
					{
						if (t.IsSubclassOf(typeof(ContentItem)) && !types.Contains(t))
						{
							var index = types.IndexOf(t.BaseType);
							types.Insert(index + 1, t);
						}
					}
			}

			return types;
		}

		/// <summary>Enumerates base type chain of the supplied type.</summary>
		/// <param name="t">The type whose base types will be enumerated.</param>
		/// <returns>An enumerator of the supplied item and all it's base types.</returns>
		private static IEnumerable<Type> EnumerateBaseTypes(Type t)
		{
			if (t != null)
			{
				foreach (var baseType in EnumerateBaseTypes(t.BaseType))
				{
					yield return baseType;
				}

				yield return t;
			}
		}

		private static string GetName(Type t)
		{
			return t.FullName + ", " + t.Assembly.FullName.Split(',')[0];
		}

		private string GetDiscriminator(Type itemType)
		{
			var definition = _definitions[itemType];
			if (definition != null)
			{
				return definition.Discriminator;
			}

			return itemType.Name;
		}
	}
}
