using Realms;

namespace RealmJson.Test
{
	public class StateUnique : RealmObject
	{
		[PrimaryKey]
		public string abbreviation { get; set; }
		public string name { get; set; }
	}

	public class State : RealmObject
	{
		public string abbreviation { get; set; }
		public string name { get; set; }
	}
}
