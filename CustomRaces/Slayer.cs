using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.CharacterSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomRaces
{
    class Slayer
    {
        public static BlueprintCharacterClass CreateClass()
        {
            var ranger = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("cda0615668a6df14eb36ba19ee881af6");
            var rogue = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            var newClass = ScriptableObject.CreateInstance<BlueprintCharacterClass>();
            var preRequeNoClass = ScriptableObject.CreateInstance<PrerequisiteNoClassLevel>(); //Slayer can't be AnimalClass
            preRequeNoClass.CharacterClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("4cd1757a0eea7694ba5c933729a53920");
            newClass.ComponentsArray = new BlueprintComponent[] { preRequeNoClass };
            newClass.name = "SlayerClass";
            newClass.LocalizedName = RaceUtil.MakeLocalized("Slayer");
            newClass.LocalizedDescription = RaceUtil.MakeLocalized("Skilled at tracking down targets, slayers are consummate hunters, living for the chase and the deadly stroke that brings it to a close. Slayers spend most of their time honing their weapon skills, studying the habits and anatomy of foes, and practicing combat maneuvers.");
            newClass.HitDie = Kingmaker.RuleSystem.DiceType.D10;
            newClass.SkillPoints = 4;
            newClass.BaseAttackBonus = ranger.BaseAttackBonus;
            newClass.FortitudeSave = ranger.FortitudeSave;
            newClass.ReflexSave = ranger.ReflexSave;
            newClass.WillSave = ranger.WillSave;
            newClass.Progression = ScriptableObject.CreateInstance<BlueprintProgression>();
            newClass.Archetypes = new BlueprintArchetype[0]; //No Archtypes
            newClass.ClassSkills = new StatType[]
            {
                StatType.SkillMobility,
                StatType.SkillAthletics,
                StatType.SkillPersuasion,
                StatType.SkillPerception,
                StatType.SkillStealth,
                StatType.SkillKnowledgeWorld
            };
            SetupClassApperance(newClass, rogue, ranger);
            newClass.StartingGold = 411;
            newClass.StartingItems = new Kingmaker.Blueprints.Items.BlueprintItem[] { };
            newClass.RecommendedAttributes = new StatType[] { StatType.Dexterity };
            //newClass.DefaultBuild = rogue.DefaultBuild;
            RaceUtil.AddBlueprint(newClass, "38e7361d138f4a04bf03659e4204543e");
            RaceUtil.AddBlueprint(newClass.Progression, "d6bdd34d8f8f4295b6433a201c8e0605");
            return newClass;
        }
        static void SetupClassApperance(BlueprintCharacterClass newClass, BlueprintCharacterClass rogue, BlueprintCharacterClass ranger)
        {
            newClass.PrimaryColor = 31;
            newClass.SecondaryColor = 7;
            /*
             * Shoulder stuff, Belts and straps, keytools
             * "b1c62eff2287d9a4fbbf76c345d58840" EE_RogueAccesories_M
             * "345af8eabd450524ab364e7a7c6f1044" EE_RogueAccesories_F
             * 
             * Base Outfit
             * "d019e95d4a8a8474aa4e03489449d6ee" EE_Rogue_M_Any_Colorize
             * "c6757746d62b78f46a92020110dfe088" EE_Rogue_F_Any_Colorize
             * 
             * Nice Cape
             * "bba6c03b44e5a1c4dbfacf7eec6123dd" EE_Rogue_M_Cape
             * "b7613075291c79947a0cde8c7aec5926" EE_Rogue_F_Cape
             * 
             * Backpack
             * "e249678d823d00f4cb30d4d5c8ca1219" Ranger_M_Accessories
             * "e09cf61a567f2a84ea9a3b505f390a32" Ranger_F_Accessories
             * 
             * Belts and Straps
             * "0809ab3735b54874b965a09311f0c898" EE_RangerAccesories_M_Any_Colorize
             * "b6bca728c4ced324da7e8d0d01ad34bb" EE_RangerAccesories_F_Any_Colorize
             * 
             * Base Outfit
             * "ca71ad9178ecf6a4d942ce55d0c7857b" EE_Ranger_M_Any_Colorize
             * "bc6fb7e5c91de08418b81a397b20bb18" EE_Ranger_F_Any_Colorize
             * 
             * Ratty Cape
             * "fb0037ec1d96c8d418bc08d3e0bbf063" EE_Ranger_M_Cape
             * "52a0a0c7183957a4ea02301ce40b3e83" EE_Ranger_F_Cape
             */

            /*
             * Ranger contains: Boots, Forearms, Faulds, Hands, Torso, Upperarms, Upperlegs
             * Roue Contains: Boots, forearms, Faulds, Torso, Upperarms, Upperlegs
             */
            var rogueMaleOutfit = ResourcesLibrary.TryGetResource<EquipmentEntity>("d019e95d4a8a8474aa4e03489449d6ee");
            var rangerMaleOutfit = ResourcesLibrary.TryGetResource<EquipmentEntity>("ca71ad9178ecf6a4d942ce55d0c7857b");
            var maleOutfit = RaceUtil.CopyEquipmentEntity(rogueMaleOutfit);
            maleOutfit.BodyParts.Clear();
            var rangerBodyPartTypes = BodyPartType.Boots | BodyPartType.Hands | BodyPartType.Upperarms | BodyPartType.Forearms;
            var rogueBodyPartTypes = BodyPartType.Faulds | BodyPartType.Upperlegs |  BodyPartType.Torso;
            maleOutfit.BodyParts.AddRange(rogueMaleOutfit.BodyParts.Where(
                (bp) => (bp.Type & rogueBodyPartTypes) != 0));
            maleOutfit.BodyParts.AddRange(rangerMaleOutfit.BodyParts.Where(
                (bp) => (bp.Type & rangerBodyPartTypes) != 0));
            var maleOutfitLink = new StrongEquipmentEntityLink(maleOutfit, "7b8429914e404455b270835c20486322");
            var rogueFemaleOutfit = ResourcesLibrary.TryGetResource<EquipmentEntity>("c6757746d62b78f46a92020110dfe088");
            var rangerFemaleOutfit = ResourcesLibrary.TryGetResource<EquipmentEntity>("bc6fb7e5c91de08418b81a397b20bb18");
            var femaleOutfit = RaceUtil.CopyEquipmentEntity(rogueFemaleOutfit);
            femaleOutfit.BodyParts.Clear();
            femaleOutfit.BodyParts.AddRange(rogueFemaleOutfit.BodyParts.Where(
                (bp) => bp.Type == BodyPartType.Faulds || bp.Type == BodyPartType.Torso || bp.Type == BodyPartType.Upperarms || bp.Type == BodyPartType.Upperlegs));
            femaleOutfit.BodyParts.AddRange(rangerFemaleOutfit.BodyParts.Where(
                (bp) => bp.Type == BodyPartType.Boots || bp.Type == BodyPartType.Hands || bp.Type == BodyPartType.Forearms));

            var femaleOutfitLink = new StrongEquipmentEntityLink(femaleOutfit, "b23db2bf48b340b79e25039deb0c86dd");
            //EquipmentEntities contains race dependent equipment entities, specificly hoods because races have different heads
            newClass.EquipmentEntities = rogue.EquipmentEntities;
            newClass.MaleEquipmentEntities = new EquipmentEntityLink[] {
                new EquipmentEntityLink(){ AssetId = "0809ab3735b54874b965a09311f0c898" }, //EE_RangerAccesories_M_Any_Colorize
                maleOutfitLink, //CustomOutfit
                new EquipmentEntityLink(){ AssetId = "fb0037ec1d96c8d418bc08d3e0bbf063" }, //EE_Ranger_M_Cape
            };
            newClass.FemaleEquipmentEntities = new EquipmentEntityLink[] {
                new EquipmentEntityLink(){ AssetId = "b6bca728c4ced324da7e8d0d01ad34bb" }, //EE_RangerAccesories_F_Any_Colorize
                femaleOutfitLink, //CustomOutfit
                new EquipmentEntityLink(){ AssetId = "52a0a0c7183957a4ea02301ce40b3e83" }, //EE_Ranger_F_Cape
            };
        }
        static void SetupProgression(BlueprintProgression progression, BlueprintCharacterClass newClass)
        {
            progression.name = "SlayerProgression";
            progression.Archetypes = new BlueprintArchetype[0];
            progression.Classes = new BlueprintCharacterClass[] { newClass };
            progression.Groups = new FeatureGroup[0];
            progression.LevelEntries = new LevelEntry[20];
            //progression.Description
            //Progression.Name
            List<List<string>> features = new List<List<string>>(20);
            features[0].Add("c5e479367d07d62428f2fe92f39c0341"); //RangerProficiencies TODO: Clone and change name
            features[0].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            //features[0].Add("d3e6275cfa6e7a04b9213b7b292a011c"); //Ranger RayCalculateFeature is this for spells?
            //features[0].Add("62ef1cdb90f1d654d996556669caf7fa"); //Ranger TouchCalculateFeature is this for spells?
            features[1].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[2].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[3].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[4].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            features[4].Add("c1be13839472aad46b152cf10cf46179"); //FavoriteEnemyRankUp TODO: Make Studied Target
            features[5].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[5].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[6].Add("c7e1d5ef809325943af97f093e149c4f"); //Stealthy TODO: Make Stalker talent
            features[7].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[8].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[9].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            features[9].Add("c1be13839472aad46b152cf10cf46179"); //FavoriteEnemyRankUp TODO: Make Studied Target
            features[9].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[9].Add("a33b99f95322d6741af83e9381b2391c"); //AdvanceTalents TODO: Make Advanced Slayer Talent
            features[10].Add("97a6aa2b64dd21a4fac67658a91067d7"); //FastStealth TODO: Make Swift Tracker
            features[10].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[11].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            features[11].Add("c1be13839472aad46b152cf10cf46179"); //FavoriteEnemyRankUp TODO: Make Studied Target
            features[11].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[12].Add("7df32d4e9bd2cdc48b0f69b03a57754a"); //SwiftFootFeature TODO replace with Slayer’s Advance
            features[13].Add("385260ca07d5f1b4e907ba22a02944fc"); //Quarry
            features[13].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[14].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            features[14].Add("c1be13839472aad46b152cf10cf46179"); //FavoriteEnemyRankUp TODO: Make Studied Target
            features[14].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[15].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[16].Add("7df32d4e9bd2cdc48b0f69b03a57754a"); //SwiftFootFeature TODO replace with Slayer’s Advance
            features[17].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            features[17].Add("9b9eac6709e1c084cb18c3a366e0ec87"); //SneakAttack
            features[18].Add("25e009b7e53f86141adee3a1213af5af"); //Improved Quary
            features[19].Add("16cc2c937ea8d714193017780e7d4fc6"); //FavoriteEnemySelection TODO: Make Studied Target
            features[19].Add("c1be13839472aad46b152cf10cf46179"); //FavoriteEnemyRankUp TODO: Make Studied Target
            features[19].Add("9d53ef63441b5d84297587d75f72fc17"); //MasterHunter TODO: Make Master Slayer
            features[19].Add("72dcf1fb106d5054a81fd804fdc168d3"); //MasterStrike TODO: Make Master Slayer
            features[19].Add("c074a5d615200494b8f2a9c845799d93"); //RogueTalentSelection TODO: Make Slayer Talent
            for(int i = 0; i < features.Count; i++)
            {
                progression.LevelEntries[i] = new LevelEntry();
                progression.LevelEntries[i].Level = i + 1;
                foreach(var featureId in features[i])
                {
                    progression.LevelEntries[i].Features.Add(ResourcesLibrary.TryGetBlueprint<BlueprintFeatureBase>(featureId));
                }
            }
            progression.UIDeterminatorsGroup = progression.LevelEntries[0].Features.ToArray();
            progression.UIGroups = new UIGroup[1];
            progression.UIGroups[0] = new UIGroup();
            progression.UIGroups[0].Features.AddRange(progression.LevelEntries[19].Features);

            //TODO progression.UIDeterminatorGroup
            //TODO UIGroups
        }

    }
}
