using System;
using Realms;

namespace Nuget.Test
{
	public class State : RealmObject
	{
		[PrimaryKey]
		public string abbreviation { get; set; }
		public string name { get; set; }
	}
}
