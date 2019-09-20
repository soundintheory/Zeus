using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Zeus.BaseLibrary.Reflection
{
	public class TypeFinder : ITypeFinder
	{
		private readonly IAssemblyFinder _assemblyFinder;

		public TypeFinder(IAssemblyFinder assemblyFinder)
		{
			_assemblyFinder = assemblyFinder;
		}

		/// <summary>Finds types assignable from of a certain type in the app domain.</summary>
		/// <param name="requestedType">The type to find.</param>
		/// <returns>A list of types found in the app domain.</returns>
		public IEnumerable<Type> Find(Type requestedType)
		{
			var types = new List<Type>();
			foreach (var assembly in _assemblyFinder.GetAssemblies())
			{
				try
				{
					foreach (var type in assembly.GetTypes())
					{
						if (requestedType.IsAssignableFrom(type))
						{
							types.Add(type);
						}
					}
				}
				catch (ReflectionTypeLoadException ex)
				{
					var loaderErrors = string.Empty;
					foreach (var loaderEx in ex.LoaderExceptions)
					{
						Trace.TraceError(loaderEx.ToString());
						loaderErrors += ", " + loaderEx.Message;
					}

					throw new Exception(string.Format("Error getting types from assembly " + assembly.FullName + loaderErrors, ex));
				}
			}
			return types;
		}
	}
}