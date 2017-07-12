using Realms;

namespace RealmJson.Test
{
	[Preserve(AllMembers = true)]
	public class StateUnique : RealmObject
	{
		[PrimaryKey]
		public string abbreviation { get; set; }
		public string name { get; set; }
	}

	[Preserve(AllMembers = true)]
	public class State : RealmObject
	{
		public string abbreviation { get; set; }
		public string name { get; set; }
	}
}
