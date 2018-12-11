using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Items.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlueprints
{
    class BlueprintInfo
    {
        public List<BlueprintCharacterClass> Classes = new List<BlueprintCharacterClass>();
        public List<BlueprintRace> Races = new List<BlueprintRace>();
        public List<BlueprintItemWeapon> Weapons = new List<BlueprintItemWeapon>();
        public List<BlueprintFeature> Feats = new List<BlueprintFeature>();
        public List<BlueprintScriptableObject> Register = new List<BlueprintScriptableObject>();
        public static BlueprintInfo Load()
        {
            return JsonBlueprints.Load<BlueprintInfo>("mods/customblueprints/data/BlueprintInfo.json");
        }
    }
}
