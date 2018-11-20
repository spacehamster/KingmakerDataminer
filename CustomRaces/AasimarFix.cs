using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRaces
{
    class AasimarFix
    {
        public static void Apply()
        {
            var aasimar = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("b7f02ba92b363064fb873963bec275ee");
            var human = ResourcesLibrary.TryGetBlueprint<BlueprintRace>("0a5d473ead98b0646b94495af250fdc4");
            var humanSkin = human.Presets[0].Skin.Load(Gender.Male, human.RaceId).First();
            var humanHair = human.MaleOptions.Hair[0].Load();
            var genders = new Gender[] { Gender.Male, Gender.Female };
            foreach(var gender in genders)
            {
                foreach(var preset in aasimar.Presets)
                {
                    foreach(var ee in preset.Skin.Load(gender, aasimar.RaceId)){
                        ee.PrimaryRamps.AddRange(humanSkin.PrimaryRamps);
                        ee.SecondaryRamps.AddRange(humanSkin.PrimaryRamps);
                        ee.PrimaryRamps.Distinct();
                        ee.PrimaryRamps.Distinct();
                    }
                }
                var faces = gender == Gender.Male ? aasimar.MaleOptions.Heads : aasimar.FemaleOptions.Heads;
                foreach(var face in faces)
                {
                    var ee = face.Load();
                    ee.PrimaryRamps.AddRange(humanSkin.PrimaryRamps);
                    ee.SecondaryRamps.AddRange(humanSkin.PrimaryRamps);
                    ee.PrimaryRamps.Distinct();
                    ee.PrimaryRamps.Distinct();
                }
                var hairs = gender == Gender.Male ? aasimar.MaleOptions.Hair : aasimar.FemaleOptions.Hair;
                foreach (var hair in hairs)
                {
                    var ee = hair.Load();
                    ee.PrimaryRamps.AddRange(humanHair.PrimaryRamps);
                    ee.SecondaryRamps.AddRange(humanHair.PrimaryRamps);
                }
                var beards = gender == Gender.Male ? aasimar.MaleOptions.Beards : aasimar.FemaleOptions.Beards;
                foreach (var beard in beards)
                {
                    var ee = beard.Load();
                    ee.PrimaryRamps.AddRange(humanHair.PrimaryRamps);
                    ee.SecondaryRamps.AddRange(humanHair.PrimaryRamps);
                }
            }
            {
                var hair = human.MaleOptions.Hair.ToList();
                hair.AddRange(aasimar.MaleOptions.Hair);
                aasimar.MaleOptions.Hair = hair.ToArray();

                hair = human.FemaleOptions.Hair.ToList();
                hair.AddRange(aasimar.FemaleOptions.Hair);
                aasimar.FemaleOptions.Hair = hair.ToArray();
            }


        }
    }
}
