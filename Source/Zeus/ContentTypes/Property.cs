using System.Reflection;

namespace Zeus.ContentTypes
{
	public class Property
	{
		#region Constructor

		public Property(PropertyInfo underlyingProperty)
		{
			Name = underlyingProperty.Name;
			UnderlyingProperty = underlyingProperty;
		}

		#endregion

		#region Properties

		public string Name
		{
			get;
		}

		public PropertyInfo UnderlyingProperty
		{
			get;
		}

		#endregion
	}
}
