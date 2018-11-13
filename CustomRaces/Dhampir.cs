using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
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
            newRace.SelectableRaceStat = false;
            SetRamps(newRace);
            newRace.Features = new BlueprintFeatureBase[]
            {
                (BlueprintFeatureBase)blueprints["8a75eb16bfff86949a4ddcb3dd2f83ae"], //UndeadImmunities
            };
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Dhampir"));
            Traverse.Create(newRace).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Description Goes Here"));
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
    }
}
