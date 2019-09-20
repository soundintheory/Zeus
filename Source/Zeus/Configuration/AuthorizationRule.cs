using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;

namespace Zeus.Configuration
{
	public class AuthorizationRule : ConfigurationElement
	{
		private CommaDelimitedStringCollection _roles, _users;

		public AuthorizationRuleAction Action { get; set; }

		[TypeConverter(typeof(CommaDelimitedStringCollectionConverter)), ConfigurationProperty("roles")]
		public StringCollection Roles
		{
			get
			{
				if (_roles == null)
				{
					var strings = (CommaDelimitedStringCollection) base["roles"];
					_roles = strings == null ? new CommaDelimitedStringCollection() : strings.Clone();
				}
				return _roles;
			}
		}

		[TypeConverter(typeof(CommaDelimitedStringCollectionConverter)), ConfigurationProperty("users")]
		public StringCollection Users
		{
			get
			{
				if (_users == null)
				{
					var strings = (CommaDelimitedStringCollection) base["users"];
					_users = strings == null ? new CommaDelimitedStringCollection() : strings.Clone();
				}
				return _users;
			}
		}
	}
}