using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System.Collections.Generic;
using UnityEngine;

namespace CustomRaces
{
    public static class Drow
    {
        static AssetBundle bundle;
        public static BlueprintRace CreateRace()
        {
            if (bundle == null) bundle = AssetBundle.LoadFromFile("Mods/CustomRaces/AssetBundles/drow");

            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var elf = (BlueprintRace)blueprints["25a5878d125338244896ebd3238226c8"];
            var newRace = RaceUtil.CopyRace(elf, "bf5ee08cbc0a44b898a3f2a0ed158b97");
            newRace.name = "DrowRace";
            newRace.ComponentsArray = elf.ComponentsArray;
            SetRamps(newRace);
            newRace.Features = new BlueprintFeatureBase[]
            {
                (BlueprintFeatureBase)blueprints["9c747d24f6321f744aa1bb4bd343880d"], //Keen Senses
                (BlueprintFeatureBase)blueprints["55edf82380a1c8540af6c6037d34f322"], //ElvenMagic
                (BlueprintFeatureBase)blueprints["2483a523984f44944a7cf157b21bf79c"], //ElvenImmunity
                (BlueprintFeatureBase)blueprints["03fd1e043fc678a4baf73fe67c3780ce"]  //ElvenWeaponFamiliarity
                //SpellResistance()
            };
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Drow"));
            Traverse.Create(newRace).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Description Goes Here"));
            return newRace;
        }
        static void SetRamps(BlueprintRace newRace)
        {
            var ramps = new List<Texture2D>
            {
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Hair_VioletDark_U_HM.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_PinkSallow_U_HO.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_Black_U_HM.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_Black_U_GN.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_BrownDark_U_HO.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Hair_Gray_U_EL.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Hair_BrownDarkFaded_U_EL.png")
            };
            foreach (var head in newRace.MaleOptions.Heads)
            {
                head.Load().ColorsProfile.PrimaryRamps.Clear();
                head.Load().ColorsProfile.PrimaryRamps.AddRange(ramps);
            }
            foreach (var head in newRace.FemaleOptions.Heads)
            {
                head.Load().ColorsProfile.PrimaryRamps.Clear();
                head.Load().ColorsProfile.PrimaryRamps.AddRange(ramps);
            }

            for(int i = 0; i < newRace.Presets.Length; i++)
            {
                var maleSkin = newRace.Presets[i].Skin.GetLinks(Gender.Male, newRace.RaceId);
                var femaleSkin = newRace.Presets[i].Skin.GetLinks(Gender.Female, newRace.RaceId);
                foreach (var skin in maleSkin)
                {
                    skin.Load().ColorsProfile.PrimaryRamps.Clear();
                    skin.Load().ColorsProfile.PrimaryRamps.AddRange(ramps);
                }
                foreach (var skin in femaleSkin)
                {
                    skin.Load().ColorsProfile.PrimaryRamps.Clear();
                    skin.Load().ColorsProfile.PrimaryRamps.AddRange(ramps);
                }
            }
        }
        //TODO make spellResistance6plusCR
        static BlueprintFeatureBase SpellResistance()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var spellResistance5plusCR = (BlueprintFeature)blueprints["2378680aaca855840ba325c509f5d654"];
            Traverse.Create(spellResistance5plusCR).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Spell Resistance"));
            Traverse.Create(spellResistance5plusCR).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Drow possess spell resistance (SR) equal to 6 plus their total number of class levels."));
            return spellResistance5plusCR;
        }
    }


}
