using Harmony12;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Localization;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;

namespace CustomRaces
{
    static public class RaceUtil
    {
        public static readonly string AssetSuffix = "#CustomFeature";
        public static Dictionary<Type, string> FallbackTable = new Dictionary<Type, string>()
        {
            { typeof(BlueprintRace),  "0a5d473ead98b0646b94495af250fdc4" }, //Human
            { typeof(BlueprintRaceVisualPreset),  "58181bf151eb0c0408f82546541dcc03" }, //Human_Standard_VisualPreset
            { typeof(BlueprintCharacterClass), "299aa766dee3cbf4790da4efb8c72484" }, //Rogue
            { typeof(BlueprintProgression), "b57b2a75a5abcaf47a01cf84672b50e9" }, //RogueProgression
            { typeof(BlueprintArchetype), "9e94d1847e6f331478e5a714659220ce"}, //KnifeMaster
            { typeof(BlueprintFeature),  "9b9eac6709e1c084cb18c3a366e0ec87" }, //SneakAttack
            { typeof(BlueprintFeatureSelection), "c074a5d615200494b8f2a9c845799d93" }, //RogueTalent
            { typeof(BlueprintSpellbook), "5a38c9ac8607890409fcb8f6342da6f4" }, //WizardSpellbook
            { typeof(BlueprintSpellList), "ba0401fdeb4062f40a7aa95b6f07fe89" }, //WizardSpelllist
            { typeof(BlueprintSpellsTable), "78bb94ed2e75122428232950bb09e97b" }, //WizardSpellLevels
            { typeof(BlueprintStatProgression), "4c936de4249b61e419a3fb775b9f2581" }, //BABMedium
            { typeof(BlueprintItemWeapon), "20f03323262f8604f8b8e4affe7dc3c8" }, //LongswordFrostPlus2
            { typeof(EquipmentEntity), "d019e95d4a8a8474aa4e03489449d6ee" } //RogueOutfit

        };
        public static string AddResource(UnityEngine.Object obj, string newAssetId, Type type)
        {
            string fallbackId = null;
            FallbackTable.TryGetValue(type, out fallbackId);
            if (fallbackId == null)
            {
                //throw new Exception($"No fallback for typeof {type}");
                fallbackId = "NULL";
            }
            string assetId = string.Format("{0}:{1}{2}", newAssetId, fallbackId, AssetSuffix);
            var resourceType = Traverse.CreateWithType("Kingmaker.Blueprints.ResourcesLibrary+LoadedResource").GetValue<Type>();
            object resource = Activator.CreateInstance(resourceType);
            Traverse.Create(resource).Field("Resource").SetValue(obj);
            Traverse.Create(resource).Field("RequestCounter").SetValue(1);
            var list = Traverse.Create(typeof(ResourcesLibrary)).Field("s_LoadedResources").GetValue<object>();
            Traverse.Create(list).Method("Add", new object[] { assetId, resource }).GetValue();
            return assetId;
        }
        public static string AddResource<T>(T obj, string newAssetId) where T : UnityEngine.Object
        {
            return AddResource(obj, newAssetId, typeof(T));
        }
        public static void AddBlueprint<T>(T blueprint, string newAssetId) where T : BlueprintScriptableObject
        {
            string fallbackId = null; 
            FallbackTable.TryGetValue(typeof(T), out fallbackId);
            if(fallbackId == null)
            {
                throw new Exception($"No fallback for typeof {typeof(T)}");
            }
            string assetId = string.Format("{0}:{1}{2}", newAssetId, fallbackId, AssetSuffix);
            Traverse.Create(blueprint).Field("m_AssetGuid").SetValue(assetId);
            ResourcesLibrary.LibraryObject.BlueprintsByAssetId[assetId] = blueprint;
            //This is not required, only used for debugging and ResourcesLibrary.GetBlueprints<BlueprintAreaPreset>() on gameload
            Traverse.Create(ResourcesLibrary.LibraryObject)
            .Field("m_AllBlueprints")
            .Method("Add", new object[] { blueprint })
            .GetValue();
        }
        public static EquipmentEntityLink MakeEquipmentEntityLink(EquipmentEntity ee, string assetId)
        {
            var newAssetId = AddResource(ee, assetId);
            var eel = new EquipmentEntityLink();
            eel.AssetId = newAssetId;
            return eel;
        }
        public static string GetDeterministicAssetID(string input)
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            byte[] inputBytes = Encoding.Default.GetBytes(input);
            byte[] hashBytes = provider.ComputeHash(inputBytes);
            Guid hashGuid = new Guid(hashBytes);
            return hashGuid.ToString("N");
        }
        public static EquipmentEntity CopyEquipmentEntity(EquipmentEntity oldEE)
        {
            var newEE = ScriptableObject.CreateInstance<EquipmentEntity>();
            newEE.name = oldEE.name;
            if (oldEE.ColorsProfile != null)
            {
                newEE.ColorsProfile = ScriptableObject.CreateInstance<CharacterColorsProfile>();
                newEE.ColorsProfile.PrimaryRamps = new List<Texture2D>(oldEE.ColorsProfile.PrimaryRamps);
                newEE.ColorsProfile.SecondaryRamps = new List<Texture2D>(oldEE.ColorsProfile.PrimaryRamps);
                newEE.ColorsProfile.name = oldEE.ColorsProfile.name;
            }
            newEE.HideBodyParts = oldEE.HideBodyParts;
            newEE.ShowLowerMaterials = oldEE.ShowLowerMaterials;
            newEE.Layer = oldEE.Layer;
            var primaryRamps = Traverse.Create(oldEE).Field("m_PrimaryRamps").GetValue<List<Texture2D>>();
            //Outfit parts are static meshes
            newEE.OutfitParts = new List<EquipmentEntity.OutfitPart>(oldEE.OutfitParts);

            foreach (var oldBP in oldEE.BodyParts)
            {
                var newBP = new BodyPart();
                newBP.RendererPrefab = oldBP.RendererPrefab;
                newBP.Material = oldBP.Material;
                newBP.Textures = oldBP.Textures;
                newBP.Type = oldBP.Type;
                newEE.BodyParts.Add(newBP);
            }
            Traverse.Create(newEE).Field("m_BonesByName").SetValue(new Dictionary<string, Skeleton.Bone>(oldEE.BonesByName));
            return newEE;
        }
        static EquipmentEntityLink[] CopyLinks(EquipmentEntityLink[] links, string oldGUID)
        {
            var eels = new EquipmentEntityLink[links.Length];
            for (int i = 0; i < links.Length; i++)
            {
                var oldEE = links[i].Load();
                var newEE = CopyEquipmentEntity(oldEE);
                var assetID = GetDeterministicAssetID(oldGUID);
                eels[i] = RaceUtil.MakeEquipmentEntityLink(newEE, assetID);
                oldGUID = assetID;
            }
            return eels;
        }
        static BlueprintRaceVisualPreset[] CopyPresets(BlueprintRace original, string prevGUID)
        {
            var newPresets = new BlueprintRaceVisualPreset[original.Presets.Length];
            for (int i = 0; i < original.Presets.Length; i++)
            {
                newPresets[i] = ScriptableObject.CreateInstance<BlueprintRaceVisualPreset>();
                var presetAssetGUID = GetDeterministicAssetID(prevGUID);
                RaceUtil.AddBlueprint(newPresets[i], presetAssetGUID);
                //RaceManager.assets[presetAssetGUID] = newPresets[i];
                newPresets[i].RaceId = original.Presets[i].RaceId;
                newPresets[i].MaleSkeleton = original.Presets[i].MaleSkeleton;
                newPresets[i].FemaleSkeleton = original.Presets[i].FemaleSkeleton;
                newPresets[i].Skin = ScriptableObject.CreateInstance<KingmakerEquipmentEntity>();
                var maleSkin = CopyLinks(original.Presets[i].Skin.GetLinks(Gender.Male, original.RaceId), presetAssetGUID + "MaleSalt");
                var femaleSkin = CopyLinks(original.Presets[i].Skin.GetLinks(Gender.Female, original.RaceId), presetAssetGUID + "FemaleSalt");
                Traverse.Create(newPresets[i].Skin).Field("m_RaceDependent").SetValue(false);
                Traverse.Create(newPresets[i].Skin).Field("m_MaleArray").SetValue(maleSkin);
                Traverse.Create(newPresets[i].Skin).Field("m_FemaleArray").SetValue(femaleSkin);
                prevGUID = presetAssetGUID;
            }
            return newPresets;
        }
        static CustomizationOptions CopyCustomisationOptions(CustomizationOptions orignal, string prevGUID)
        {
            var newOptions = new CustomizationOptions();
            newOptions.Beards = CopyLinks(orignal.Beards, prevGUID + "BeardSalt");
            newOptions.Heads = CopyLinks(orignal.Heads, prevGUID + "HeadSalt");
            newOptions.Eyebrows = CopyLinks(orignal.Eyebrows, prevGUID + "EyebrowSalt");
            newOptions.Hair = CopyLinks(orignal.Hair, prevGUID + "HairSalt");
            return newOptions;            
        }
        static public BlueprintRace CopyRace(BlueprintRace original, string newID = null)
        {
            var newRace = ScriptableObject.CreateInstance<BlueprintRace>();
            if (newID == null) newID = original.AssetGuid;
            RaceUtil.AddBlueprint(newRace, newID);
            newRace.RaceId = original.RaceId;
            newRace.SoundKey = original.SoundKey;
            newRace.SelectableRaceStat = original.SelectableRaceStat;
            newRace.MaleSpeedSettings = original.MaleSpeedSettings;
            newRace.FemaleSpeedSettings = original.FemaleSpeedSettings;
            newRace.MaleOptions = CopyCustomisationOptions(original.MaleOptions, newID + "MaleOptions");
            newRace.FemaleOptions = CopyCustomisationOptions(original.FemaleOptions, newID + "FemaleOptions");
            newRace.Presets = CopyPresets(original, newID + "Presets");
            newRace.Features = (BlueprintFeatureBase[]) original.Features.Clone();
            return newRace;
        }
        public static UnityEngine.Object FindObjectByInstanceId<T>(int instanceId)
        {
            return FindObjectByInstanceId(instanceId, typeof(T));
        }
        public static UnityEngine.Object FindObjectByInstanceId(int instanceId, Type type)
        {
            foreach (UnityEngine.Object go in Resources.FindObjectsOfTypeAll(type))
            {
                if (go.GetInstanceID() == instanceId) return go;
            }
            return null;
        }
        /*
         * Huge hack. TODO fix
         */
        public static LocalizedString MakeLocalized(string text)
        {
            var key = Guid.NewGuid().ToString();
            LocalizationManager.CurrentPack.Strings[key] = text;
            var localized = new LocalizedString();
            Traverse.Create(localized).Field("m_Key").SetValue(key);
            return localized;
        }
    }

}
