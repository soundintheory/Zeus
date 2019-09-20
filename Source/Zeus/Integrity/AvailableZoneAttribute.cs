﻿using System;
using System.Collections.Generic;
using Zeus.ContentTypes;

namespace Zeus.Integrity
{
	/// <summary>
	/// Class decoration that lets Zeus know that a class has zones where to 
	/// which data items can be bound (ZoneName).
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class AvailableZoneAttribute : AbstractContentTypeRefiner, IInheritableDefinitionRefiner
	{
		public AvailableZoneAttribute(string title, string zoneName)
		{
			this.Title = title;
			this.ZoneName = zoneName;
		}

#region Private Members

		#endregion

		#region Properties
		/// <summary>Gets or sets the name of the zone in question.</summary>
		public string ZoneName { get; set; }

		/// <summary>Gets or sets the title displayed to editors.</summary>
		public string Title { get; set; }

		#endregion

		#region Equals & GetHashCode

		public override bool Equals(object obj)
		{
			if (obj is AvailableZoneAttribute)
			{
				return ZoneName.Equals(((AvailableZoneAttribute) obj).ZoneName);
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return ZoneName.GetHashCode();
		}

		#endregion

		public override void Refine(ContentType definition, IList<ContentType> allDefinitions)
		{
			definition.AvailableZones.Add(this);
		}
	}
}
