using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using Zeus.Linq;
using Zeus.Persistence;
using Zeus.Security;
using Zeus.Security.ContentTypes;
using Zeus.Web.Security.Items;

namespace Zeus.Web.Security
{
	public class CredentialStore : ICredentialStore, IWebSecurityManager
	{
		#region Fields

		private readonly IHost _host;
		private readonly IPersister _persister;

		#endregion

		#region Constructor

		public CredentialStore(IPersister persister, IHost host)
		{
			_persister = persister;
			_host = host;

			DefaultRoles = new[] {"Everyone", "Members", "Editors", "Administrators"};
		}

		#endregion

		#region Properties

		protected int RootItemID
		{
			get { return _host.CurrentSite.RootItemID; }
		}

		public string[] DefaultRoles { get; set; }

		#endregion

		#region Methods

		public Role GetRole(string roleName)
		{
			var roles = GetRoleContainer(false);
			return roles.GetChild(roleName) as Role;
		}

		public IEnumerable<Role> GetRoles(IPrincipal user)
		{
			var roleContainer = GetRoleContainer(true);
			return roleContainer.GetChildren().Authorized(user, Context.SecurityManager, Operations.Read).Cast<Role>();
		}

		void ICredentialStore.CreateUser(string username, string password, string[] roles, string email, bool verified)
		{
			var userContainer = GetUserContainer(true);
			var roleContainer = GetRoleContainer(true);

			var user = new User
			{
				Name = username,
				Password = password,
				Email = email,
				Verified = verified
			};
			foreach (var role in roles)
			{
				user.RolesInternal.Add(roleContainer.GetRole(role));
			}

			user.AddTo(userContainer);

			Context.Persister.Save(user);
		}

		User ICredentialStore.GetUser(string username)
		{
			return GetUser(username);
		}

		public User GetUserByNonce(string nonce)
		{
			var users = GetUserContainer(false);
			if (users == null)
			{
				return null;
			}

			return users.GetChildren<User>().SingleOrDefault(u => u.Nonce == nonce);
		}

		public PasswordResetRequest GetPasswordResetRequestByNonce(string nonce)
		{
			var users = GetUserContainer(false);
			if (users == null)
			{
				return null;
			}

			var user = users.GetChildren<User>().SingleOrDefault(u => u.GetChildren<PasswordResetRequest>().Any(prr => prr.Nonce == nonce));
			if (user == null)
			{
				return null;
			}

			return user.GetChildren<PasswordResetRequest>().First(prr => prr.Nonce == nonce);
		}

		public User GetUserByEmail(string email)
		{
			var users = GetUserContainer(false);
			if (users == null)
			{
				return null;
			}

			return users.GetChildren<User>().SingleOrDefault(u => u.Email == email);
		}

		public void SaveNonce(User user, string nonce)
		{
			var typedUser = (User) user;
			typedUser.Nonce = nonce;
			typedUser.Verified = false;
			_persister.Save(typedUser);
		}

		public void VerifyUser(User user)
		{
			var typedUser = (User) user;
			typedUser.Nonce = null;
			typedUser.Verified = true;
			_persister.Save(typedUser);
		}

		IEnumerable<string> ICredentialStore.GetAllRoles()
		{
			var roles = GetRoleContainer(false);
			return roles == null ? null : roles.GetRoleNames();
		}

		IEnumerable<User> ICredentialStore.GetAllUsers()
		{
			var users = GetUserContainer(false);
			return users == null ? null : users.GetChildren().Cast<User>();
		}

		/// <summary>
		/// A more efficient method to retrieve users
		/// </summary>
		/// <param name="username">Username to look up</param>
		/// <returns>User</returns>
		public User Get(string username)
		{
			if (string.IsNullOrWhiteSpace(username))
			{
				return null;
			}
			var user = _persister.Get<User>(x => string.Equals(x.Name, username, StringComparison.InvariantCultureIgnoreCase));
			return user;
		}

		private User GetUser(string username)
		{
			var users = GetUserContainer(false);
			if (users == null)
			{
				return null;
			}

			return users.GetChild(username) as User;
		}

		private UserContainer GetUserContainer(bool create)
		{
			var security = GetSecurityContainer(create);
			if (security != null)
			{
				return (UserContainer) security.GetChild(UserContainer.ContainerName);
			}

			return null;
		}

		private RoleContainer GetRoleContainer(bool create)
		{
			var security = GetSecurityContainer(create);
			if (security != null)
			{
				return (RoleContainer) security.GetChild(RoleContainer.ContainerName);
			}

			return null;
		}

		private SecurityContainer GetSecurityContainer(bool create)
		{
			var root = _persister.Get(RootItemID);
			var systemNode = root.GetChildren<SystemNode>().FirstOrDefault();
			if (systemNode == null && create)
			{
				systemNode = CreateSystemNode(root);
			}

			if (systemNode == null)
			{
				return null;
			}

			var m = systemNode.GetChild(SecurityContainer.ContainerName) as SecurityContainer;
			if (m == null && create)
			{
				m = CreateSecurityContainer(systemNode);
			}

			return m;
		}

		private SystemNode CreateSystemNode(ContentItem parent)
		{
			var systemNode = Context.ContentTypes.CreateInstance<SystemNode>(parent);
			_persister.Save(systemNode);
			return systemNode;
		}

		private SecurityContainer CreateSecurityContainer(ContentItem parent)
		{
			var security = Context.ContentTypes.CreateInstance<SecurityContainer>(parent);

			var roles = Context.ContentTypes.CreateInstance<RoleContainer>(security);
			roles.AddTo(security);

			foreach (var role in DefaultRoles)
			{
				roles.AddRole(role);
			}

			var users = Context.ContentTypes.CreateInstance<UserContainer>(security);
			users.AddTo(security);

			_persister.Save(security);
			return security;
		}

		#endregion
	}
}