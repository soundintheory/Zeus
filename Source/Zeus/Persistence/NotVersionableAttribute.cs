using System;

namespace Zeus.Persistence
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	/// <summary>
	/// When used to decorate a content class this attribute tells the edit 
	/// manager not to store versions of items of that class.
	/// </summary>
	public class NotVersionableAttribute : Attribute
	{
	}
}