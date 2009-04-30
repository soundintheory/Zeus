﻿using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Zeus.Security
{
	/// <summary>
	/// Classes implementing this interface are responsible for maintaining 
	/// security by monitoring things like page access and permissions to save.
	/// </summary>
	public interface ISecurityManager
	{
		IEnumerable<string> GetAuthorizedRoles(ContentItem item);
		IEnumerable<string> GetAuthorizedUsers(ContentItem item);

		IEnumerable<string> GetAvailableOperations();

		/// <summary>Checks whether a user is authorized to access a certain item.</summary>
		/// <param name="item">The item to check for access.</param>
		/// <param name="principal">The user whose permissions to check.</param>
		/// <param name="operation"></param>
		/// <returns>True if the user is authorized.</returns>
		bool IsAuthorized(ContentItem item, IPrincipal principal, string operation);
	}
}
