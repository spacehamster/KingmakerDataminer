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
using System.Linq;
using UnityEngine;

namespace CustomBlueprints
{
    static public class BlueprintUtil
    {
        public static readonly string AssetSuffix = "#CustomFeature";
        public static string AddResource(UnityEngine.Object obj, string newAssetId, Type type)
        {
            string fallbackId = obj.GetType().Name;
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
        public static void AddBlueprint(BlueprintScriptableObject blueprint, string newAssetId)
        {
            var type = blueprint.GetType();
            string fallbackId = blueprint.GetType().Name;
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
                eels[i] = BlueprintUtil.MakeEquipmentEntityLink(newEE, assetID);
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
                BlueprintUtil.AddBlueprint(newPresets[i], presetAssetGUID);
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
            BlueprintUtil.AddBlueprint(newRace, newID);
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
            var obj = Traverse.Create<UnityEngine.Object>().Method("FindObjectFromInstanceID", new object[] { instanceId }).GetValue<UnityEngine.Object>();
            return obj;
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
        public static object ShallowClone(object obj)
        {
            return Traverse.Create(obj).Method("MemberwiseClone").GetValue();
        }
    }

}
