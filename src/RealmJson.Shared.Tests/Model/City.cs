using Realms;

namespace RealmJson.Test
{
    [Preserve(AllMembers = true)]
    public class City : RealmObject
    {
        public string Abbreviation { get; set; }
        public string Name { get; set; }
    }

}
