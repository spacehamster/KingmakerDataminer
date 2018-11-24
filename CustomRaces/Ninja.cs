using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomRaces
{
    class Ninja
    {
        public static BlueprintCharacterClass CreateClass()
        {
            var newClass = ScriptableObject.CreateInstance<BlueprintCharacterClass>();
            var rogue = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>("299aa766dee3cbf4790da4efb8c72484");
            newClass.LocalizedName = RaceUtil.MakeLocalized("Ninja");
            newClass.LocalizedDescription = RaceUtil.MakeLocalized("When the wealthy and the powerful need an enemy eliminated quietly and without fail, they call upon the ninja. When a general needs to sabotage the siege engines of his foes before they can reach the castle walls, he calls upon the ninja. And when fools dare to move against a ninja or her companions, they will find the ninja waiting for them while they sleep, ready to strike. These shadowy killers are masters of infiltration, sabotage, and assassination, using a wide variety of weapons, practiced skills, and mystical powers to achieve their goals.");
            //Archtypes cannot be duplicated accross classes because breats BlueprintArchetype.GetParentClass()
            //newClass.Archetypes = rogue.Archetypes;
            newClass.Archetypes = new BlueprintArchetype[0];
            newClass.BaseAttackBonus = rogue.BaseAttackBonus;
            newClass.ClassSkills = rogue.ClassSkills;
            newClass.ComponentsArray = rogue.ComponentsArray;
            newClass.DefaultBuild = rogue.DefaultBuild;
            newClass.EquipmentEntities = rogue.EquipmentEntities;
            newClass.FemaleEquipmentEntities = rogue.FemaleEquipmentEntities;
            newClass.FortitudeSave = rogue.FortitudeSave;
            newClass.HideIfRestricted = rogue.HideIfRestricted;
            newClass.HitDie = rogue.HitDie;
            newClass.IsArcaneCaster = rogue.IsArcaneCaster;
            newClass.IsArcaneCaster = rogue.IsDivineCaster;
            newClass.MaleEquipmentEntities = rogue.MaleEquipmentEntities;
            newClass.m_Icon = rogue.m_Icon;
            newClass.name = "NinjaClass";
            newClass.NotRecommendedAttributes = rogue.NotRecommendedAttributes;
            newClass.PrestigeClass = rogue.PrestigeClass;
            newClass.PrimaryColor = rogue.PrimaryColor;
            newClass.Progression = rogue.Progression;
            newClass.RecommendedAttributes = rogue.RecommendedAttributes;
            newClass.ReflexSave = rogue.ReflexSave;
            newClass.SecondaryColor = rogue.SecondaryColor;
            newClass.SkillPoints = rogue.SkillPoints;
            newClass.Spellbook = rogue.Spellbook;
            newClass.StartingGold = rogue.StartingGold;
            newClass.StartingItems = rogue.StartingItems;
            newClass.WillSave = rogue.WillSave;
            RaceUtil.AddBlueprint(newClass, "7b9c8a62205d44cf8d1021ed0f4bf2da");
            //Note set progression parent ot newClass
            CopyProgression(newClass, rogue.Progression);
            //BlueprintPgoression.ExclusiveProgression = newClass
            //Check for Prerequisites that contain classblueprints
            return newClass;
        }
        public static void CopyProgression(BlueprintCharacterClass newClass, BlueprintProgression oldProgression)
        {
            BlueprintProgression newProgression = ScriptableObject.CreateInstance<BlueprintProgression>();
            newProgression.Classes = new BlueprintCharacterClass[] { newClass };
            newProgression.ComponentsArray = oldProgression.ComponentsArray;
            newProgression.ExclusiveProgression = newClass;
            newProgression.Groups = oldProgression.Groups;
            newProgression.LevelEntries = oldProgression.LevelEntries;
            newProgression.name = "Custom_" + oldProgression.name;
            newProgression.Ranks = oldProgression.Ranks;
            newProgression.UIDeterminatorsGroup = newClass.Progression.LevelEntries[0].Features.ToArray();
            newProgression.UIGroups = new UIGroup[0];
            newClass.Progression = newProgression;
            RaceUtil.AddBlueprint(newProgression, "376e3fa3a4994cb5a6626d93e63d57e6");
        }
    }
}
