using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
    class BlueprintInfo
    {
        public List<BlueprintCharacterClass> Classes = new List<BlueprintCharacterClass>();
        public List<BlueprintRace> Races = new List<BlueprintRace>();
        public List<BlueprintItemWeapon> Weapons = new List<BlueprintItemWeapon>();
        public static BlueprintInfo Load()
        {
            return JsonBlueprints.Load<BlueprintInfo>("mods/customraces/data/BlueprintInfo.json");
        }
    }
}
