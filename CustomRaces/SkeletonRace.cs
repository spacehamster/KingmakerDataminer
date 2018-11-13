using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;

namespace CustomRaces
{
    public static class SkeletonRace
    {
        public static BlueprintRace CreateRace()
        {
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var halfling = (BlueprintRace)blueprints["b0c3ef2729c498f47970bb50fa1acd30"];
            var halforc = (BlueprintRace)blueprints["1dc20e195581a804890ddc74218bfd8e"];
            var newRace = RaceUtil.CopyRace(halforc, "c4f6a707e3ba4495a5b5693b42b20840");
            newRace.RaceId = Race.Halfling;
            for(int i = 0; i < newRace.Presets.Length && i < halfling.Presets.Length; i++)
            {
                newRace.Presets[i].MaleSkeleton = halfling.Presets[i].MaleSkeleton;
                newRace.Presets[i].FemaleSkeleton = halfling.Presets[i].FemaleSkeleton;
            }
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("Skeleton"));
            Traverse.Create(newRace).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Description Goes Here"));
            return newRace;
        }
    }
}
