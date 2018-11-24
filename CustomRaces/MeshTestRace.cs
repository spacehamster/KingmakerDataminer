using Harmony12;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Linq;
using UnityEngine;

namespace CustomRaces
{
    static public class MeshTestRace
    {
        public static AssetBundle bundle;
        public static BlueprintRace race;
        public static string[] testAssets = new string[]
        {
            "TS_0.prefab", //Bad
            "TS_1.prefab", //Bad
        };
        public static BlueprintRace CreateRace()
        {
            if (bundle == null) bundle = AssetBundle.LoadFromFile("Mods/CustomRaces/AssetBundles/customrace");
            var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
            var human = (BlueprintRace)blueprints["0a5d473ead98b0646b94495af250fdc4"];
            var newRace = RaceUtil.CopyRace(human, "96f0c206fb134674a0c0bbbfcb39803c");
            newRace.Features = new BlueprintFeatureBase[]
            {
                (BlueprintFeatureBase)blueprints["03fd1e043fc678a4baf73fe67c3780ce"] //ElvenWeaponFamiliarity
            };
            newRace.SelectableRaceStat = false;
            newRace.name = "MeshTestRace";
            foreach (var preset in newRace.Presets) preset.name += "MeshTestRace";
            //SetSMR(newRace);
            SetSMR2(newRace);
            Traverse.Create(newRace).Field("m_DisplayName").SetValue(RaceUtil.MakeLocalized("MeshTest"));
            Traverse.Create(newRace).Field("m_Description").SetValue(RaceUtil.MakeLocalized("Description Goes Here"));
            race = newRace;
            return newRace;
        }
        static void SetSMR2(BlueprintRace newRace)
        {
            var oldSkin = newRace.Presets[0].Skin.Load(Gender.Male, newRace.RaceId).First();
            var skinPrefab = bundle.LoadAsset<GameObject>("Assets/Preview_Blender/Skin_Male2.prefab");
            foreach (var smr in skinPrefab.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var smrType = Traverse.Create(typeof(EquipmentEntity)).Method("GetBodyPartType", new object[] { smr.name }).GetValue<BodyPartType>();
                var bp = oldSkin.BodyParts.Find((_bp) => _bp.Type == smrType);
                var oldSMR = bp.RendererPrefab.GetComponentInChildren<SkinnedMeshRenderer>();
                Main.DebugLog($"Old SMR Name {oldSMR.name}");
                bp.RendererPrefab = smr.gameObject;
                Traverse.Create(bp).Field("m_SkinnedRenderer").SetValue(smr);
            }
           
            var oldHead = newRace.MaleOptions.Heads[0].Load();
            var headPrefab = bundle.LoadAsset<GameObject>("Assets/Preview_Blender/Head_Male1.prefab");
            foreach (var smr in headPrefab.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var smrType = Traverse.Create(typeof(EquipmentEntity)).Method("GetBodyPartType", new object[] { smr.name }).GetValue<BodyPartType>();
                var bp = oldHead.BodyParts.Find((_bp) => _bp.Type == smrType);
                bp.RendererPrefab = smr.gameObject;
                Traverse.Create(bp).Field("m_SkinnedRenderer").SetValue(smr);
            }
        }
        static void SetSMR(BlueprintRace newRace)
        {
            var humanTorso = newRace.Presets[0].Skin.Load(Gender.Male, newRace.RaceId).First().BodyParts.Find((bp) => bp.Type == BodyPartType.Torso);
            var oldSkin = newRace.Presets[0].Skin.Load(Gender.Male, newRace.RaceId).First();
            var bodypart = oldSkin.BodyParts[0];
            bodypart.RendererPrefab = bundle.LoadAsset<GameObject>(testAssets[0]);
            Traverse.Create(bodypart).Field("m_SkinnedRenderer").SetValue(bodypart.SkinnedRenderer);
            Main.DebugLog("Missing skinned renderer!!!!");
            bodypart.SkinnedRenderer.sharedMesh.bindposes = humanTorso.SkinnedRenderer.sharedMesh.bindposes;
            /*var newHead = bundle.LoadAsset<EquipmentEntity>("Assets/Race/EE_Head_Face01_M_HM.asset");
            var oldHead = newRace.MaleOptions.Heads[0].Load();
            for(int i = 0; i < newHead.BodyParts.Count && i < oldHead.BodyParts.Count; i++)
            {
                break;
                var oldBP = oldHead.BodyParts[i];
                var newBP = newHead.BodyParts[i];
                oldBP.RendererPrefab = newBP.RendererPrefab;
            }
            var newSkin = bundle.LoadAsset<EquipmentEntity>("Assets/Race/EE_Naked_M_HM.asset");
            var oldSkin = newRace.Presets[0].Skin.Load(Gender.Male, newRace.RaceId).First();
            for (int i = 0; i < oldSkin.BodyParts.Count && i < newSkin.BodyParts.Count; i++)
            {
                var oldBP = oldSkin.BodyParts[i];
                var newBP = newSkin.BodyParts[i];
                oldBP.RendererPrefab = newBP.RendererPrefab;
                break;
            }*/
        }
        static void AddOptions(CustomizationOptions options, string oldAssetID)
        {
            var hair = bundle.LoadAsset<EquipmentEntity>("Assets/Race/EE_Hair_HairLongBangs_M_AS.asset");
            var head = bundle.LoadAsset<EquipmentEntity>("Assets/Race/EE_Head_Face01_M_HM.asset");
            options.Hair = new EquipmentEntityLink[] { new StrongEquipmentEntityLink(hair, new Guid(oldAssetID + "Hair").ToString()) };
            options.Heads = new EquipmentEntityLink[] { new StrongEquipmentEntityLink(head, new Guid(oldAssetID + "Head").ToString()) };
            Main.DebugLog("Added Hair");
        }
        public static void ChooseTorso(int index)
        {

            Main.DebugLog("Changed Toros");
            GameObject prefab = null;

            if (index == -1)
            {
                var blueprints = ResourcesLibrary.LibraryObject.BlueprintsByAssetId;
                var human = (BlueprintRace)blueprints["0a5d473ead98b0646b94495af250fdc4"];
                foreach (var bodypart in human.Presets[0].Skin.Load(Gender.Female, human.RaceId).First().BodyParts) {
                    if(bodypart.Type == BodyPartType.Torso) prefab = bodypart.RendererPrefab;
                }
            } else
            {
                prefab = bundle.LoadAsset<GameObject>("Assets/Model/" + testAssets[index]);
            }
            //var maleSkin = race.Presets[0].Skin.Load(Gender.Male, race.RaceId).First();
            //maleSkin.BodyParts[0].RendererPrefab = prefab;
            if (Game.Instance.Player.MainCharacter != null)
            {
                Character character = Game.Instance.Player.MainCharacter.Value.View.CharacterAvatar;
                Main.DebugLog("Changing Torso for Character " + character.name);
                bool swapped = false;
                foreach(var ee in character.EquipmentEntities.ToArray())
                {
                    if (ee.name != "Skin" && ee.name != "EE_Naked_M_HM")
                    {
                        character.RemoveEquipmentEntity(ee);
                        continue;
                    }
                    foreach (var bodypart in ee.BodyParts)
                    {
                        if (bodypart.Type != BodyPartType.Torso) continue;                        
                        var smr = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
                        bodypart.RendererPrefab = prefab;
                        Traverse.Create(bodypart).Field("m_SkinnedRenderer").SetValue(smr);
                        Main.DebugLog("Replacing bodypart with " + index + " " + prefab.name + " " + smr?.name);
                        Main.DebugLog("RootBone " + prefab.name);
                        Main.DebugLog("SharedMesh " + smr?.sharedMesh?.name);
                        Main.DebugLog("Bodypart " + index + " " + bodypart.RendererPrefab.name + " " + bodypart.SkinnedRenderer.name);
                        swapped = true;
                    }
                }

                character.IsDirty = true;
                Traverse.Create(character).Method("Update").GetValue();
                if (!swapped) Main.DebugLog("Couldn't find torso");
            } else
            {
                Main.DebugLog("Can't find main Character");
            }
        }
    }
}
