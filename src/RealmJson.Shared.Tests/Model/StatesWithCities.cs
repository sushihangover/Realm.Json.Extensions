using System.Collections.Generic;
using Realms;

namespace RealmJson.Test
{
    [Preserve(AllMembers = true)]
    public class StatesWithCities : RealmObject
    {
        public string Abbreviation { get; set; }
        public string Name { get; set; }
        public IList<City> Cities { get; } 
    }
}
