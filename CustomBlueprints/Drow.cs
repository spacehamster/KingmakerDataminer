using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using System.Collections.Generic;
using UnityEngine;

namespace CustomBlueprints
{
    public static class Drow
    {
        static AssetBundle bundle;
        public static BlueprintRace CreateRace()
        {
            if (bundle == null) bundle = AssetBundle.LoadFromFile("Mods/CustomRaces/AssetBundles/drow");

            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var elf = (BlueprintRace)blueprints["25a5878d125338244896ebd3238226c8"];
            var newRace = BlueprintUtil.CopyRace(elf, "42a7466432fd4db4870363ffa1a9eaba");
            newRace.name = "DrowRace";
            var addDex = ScriptableObject.CreateInstance<AddStatBonus>();
            addDex.name = "CustomRaceStat";
            addDex.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
            addDex.Stat = Kingmaker.EntitySystem.Stats.StatType.Dexterity;
            addDex.Value = 2;
            var addCha = ScriptableObject.CreateInstance<AddStatBonus>();
            addCha.name = "CustomRaceStat";
            addCha.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
            addCha.Stat = Kingmaker.EntitySystem.Stats.StatType.Charisma;
            addCha.Value = 2;
            var addCon = ScriptableObject.CreateInstance<AddStatBonus>();
            addCon.name = "CustomRaceStat";
            addCon.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
            addCon.Stat = Kingmaker.EntitySystem.Stats.StatType.Constitution;
            addCon.Value = -2;
            newRace.ComponentsArray = new BlueprintComponent[]
            {
                addDex,
                addCon,
                addCha
            };
            SetRamps(newRace);
            newRace.Features = new BlueprintFeatureBase[]
            {
                (BlueprintFeatureBase)blueprints["9c747d24f6321f744aa1bb4bd343880d"], //Keen Senses
                (BlueprintFeatureBase)blueprints["55edf82380a1c8540af6c6037d34f322"], //ElvenMagic
                (BlueprintFeatureBase)blueprints["2483a523984f44944a7cf157b21bf79c"], //ElvenImmunity
                (BlueprintFeatureBase)blueprints["03fd1e043fc678a4baf73fe67c3780ce"],  //ElvenWeaponFamiliarity
                SpellResistance()
            };
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(BlueprintUtil.MakeLocalized("Drow"));
            Traverse.Create(newRace).Field("m_Description").SetValue(BlueprintUtil.MakeLocalized("Cruel and cunning, drow are a dark reflection of the elven race. Also called dark elves, they dwell deep underground in elaborate cities shaped from the rock of cyclopean caverns. Drow seldom make themselves known to surface folk, preferring to remain legends while advancing their sinister agendas through proxies and agents. Drow have no love for anyone but themselves, and are adept at manipulating other creatures. While they are not born evil, malignancy is deep-rooted in their culture and society, and nonconformists rarely survive for long. Some stories tell that given the right circumstances, a particularly hateful elf might turn into a drow, though such a transformation would require a truly heinous individual."));
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
            var spellResistance6plusCR = ScriptableObject.CreateInstance<BlueprintFeature>();
            var addSpellResistance = ScriptableObject.CreateInstance<AddSpellResistance>();
            addSpellResistance.AddCR = true;
            addSpellResistance.Value = new Kingmaker.UnitLogic.Mechanics.ContextValue()
            {
                Value = 6
            };
            spellResistance6plusCR.ComponentsArray = new BlueprintComponent[]
            {
                addSpellResistance
            };
            spellResistance6plusCR.name = "SpellResistance6plusCR";
            Traverse.Create(spellResistance6plusCR).Field("m_DisplayName").SetValue(BlueprintUtil.MakeLocalized("Spell Resistance"));
            Traverse.Create(spellResistance6plusCR).Field("m_Description").SetValue(BlueprintUtil.MakeLocalized("Drow possess spell resistance (SR) equal to 6 plus their total number of class levels."));
            BlueprintUtil.AddBlueprint(spellResistance6plusCR, "94cb101bb5e944bea2e1777e6627dc5c");
            return spellResistance6plusCR;
        }
    }


}
