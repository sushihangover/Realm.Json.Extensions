using Realms;

namespace RealmJson.Test
{
    [Preserve(AllMembers = true)]
	public class State : RealmObject
	{
		public string Abbreviation { get; set; }
		public string Name { get; set; }
	}
}
