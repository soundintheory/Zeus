using System;
using Zeus.ContentTypes;

namespace Zeus.Integrity
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
	/// <summary>
	/// Base class for attributes used to restrict which types can be created below which.
	/// </summary>
	public abstract class TypeIntegrityAttribute : AbstractContentTypeRefiner
	{
		/// <summary>Gets or sets the types needed by this attribute.</summary>
		public Type[] Types { get; set; } = new Type[0];

		/// <summary>Tells wether any of the types defined by the <see cref="Types"/> property are assignable by the supplied type.</summary>
		/// <param name="type">The type to check.</param>
		/// <returns>True of any of the types was assignable from the supplied type.</returns>
		protected virtual bool IsAssignable(Type type)
		{
			if (Types == null)
			{
				return false;
			}

			foreach (var t in Types)
			{
				if (t.IsAssignableFrom(type))
				{
					return true;
				}
			}

			return false;
		}
	}
}
