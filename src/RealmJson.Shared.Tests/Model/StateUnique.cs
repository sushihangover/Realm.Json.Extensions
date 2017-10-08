using Realms;

namespace RealmJson.Test
{
    [Preserve(AllMembers = true)]
    public class StateUnique : RealmObject
    {
        [PrimaryKey]
        public string Abbreviation { get; set; }
        public string Name { get; set; }
    }

}
