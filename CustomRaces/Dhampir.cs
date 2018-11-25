using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.FactLogic;
using System.Collections.Generic;
using UnityEngine;

namespace CustomRaces
{
    public static class Dhampir
    {
        static AssetBundle bundle;
        public static BlueprintRace CreateRace()
        {
            if (bundle == null) bundle = AssetBundle.LoadFromFile("Mods/CustomRaces/AssetBundles/dhampir");
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var human = (BlueprintRace)blueprints["0a5d473ead98b0646b94495af250fdc4"];
            var newRace = RaceUtil.CopyRace(human, "7ef12cdd1464418d9f9547033bd9f77d");
            newRace.name = "DhampirRace";
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
            addCon.Stat = Kingmaker.EntitySystem.Stats.StatType.Dexterity;
            addCon.Value = -2;
            newRace.ComponentsArray = new BlueprintComponent[]
            {
                addDex,
                addCon,
                addCha
            };
            newRace.SelectableRaceStat = false;
            SetRamps(newRace);
            newRace.Features = new BlueprintFeatureBase[]
            {
                (BlueprintFeatureBase)blueprints["8a75eb16bfff86949a4ddcb3dd2f83ae"], //UndeadImmunities
                Manipulative()
            };
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Dhampir"));
            Traverse.Create(newRace).Field("m_Description").SetValue(RaceUtil.MakeLocalized("The half-living children of vampires birthed by human females, dhampirs are progenies of both horror and tragedy. The circumstances of a dhampir’s conception are often called into question but scarcely understood, as few mortal mothers survive the childbirth. Those who do often abandon their monstrous children and refuse to speak of the matter. While some speculate that dhampirs result when mortal women couple with vampires, others claim that they form when a pregnant woman suffers a vampire bite. Some particularly zealous scholars even contest dhampirs’ status as a unique race, instead viewing them as humans suffering from an unholy affliction. Indeed, this hypothesis is strengthened by dhampirs’ seeming inability to reproduce, their offspring inevitably humans (usually sorcerers with the undead bloodline)."));
            return newRace;
        }
        static void SetRamps(BlueprintRace newRace)
        {
            var ramps = new List<Texture2D>
            {
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_GrayDead_U_EL.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_Pale_U_HM.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_BlueLight_U_EL.png"),
                bundle.LoadAsset<Texture2D>("Assets/Texture2D/CR_Skin_Pale_U_EL.png"),
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
        static BlueprintFeature Manipulative()
        {
            var manipulative = ScriptableObject.CreateInstance<BlueprintFeature>();
            var addBluffStat = ScriptableObject.CreateInstance<AddStatBonus>();
            addBluffStat.name = "CustomRaceStat";
            addBluffStat.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
            addBluffStat.Stat = Kingmaker.EntitySystem.Stats.StatType.CheckBluff;
            addBluffStat.Value = 2;
            var addPerceptionStat = ScriptableObject.CreateInstance<AddStatBonus>();
            addPerceptionStat.name = "CustomRaceStat";
            addPerceptionStat.Descriptor = Kingmaker.Enums.ModifierDescriptor.Racial;
            addPerceptionStat.Stat = Kingmaker.EntitySystem.Stats.StatType.CheckBluff;
            addPerceptionStat.Value = 2;
            manipulative.ComponentsArray = new BlueprintComponent[]
            {

                addBluffStat,
                addPerceptionStat,
            };
            manipulative.name = "Manipulative";
            Traverse.Create(manipulative).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Manipulative"));
            Traverse.Create(manipulative).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Dhampir gain a +2 racial bonus on Bluff and Perception checks."));
            RaceUtil.AddBlueprint(manipulative, "6480bda61617490ca18e3ecb068e74bf");
            return manipulative;
        }

    }
}
