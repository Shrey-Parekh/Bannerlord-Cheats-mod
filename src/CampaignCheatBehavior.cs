using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace CustomCheats
{
    /// <summary>One-shot campaign actions used by the MCM buttons and hotkeys. Each is a no-op outside a campaign.</summary>
    internal static class CampaignCheatBehavior
    {
        private static bool InCampaign()
        {
            if (Campaign.Current != null && Ext.MainHero != null && Clan.PlayerClan != null)
                return true;
            Log.Message("Custom Cheats: load a campaign first.", Colors.Yellow);
            return false;
        }

        private static void Run(string label, Action action)
        {
            try
            {
                if (InCampaign())
                    action();
            }
            catch (Exception e)
            {
                Patcher.Error(label, e);
            }
        }

        internal static void AddGold(int amount) => Run("Add Gold", () =>
        {
            Ext.MainHero.ChangeHeroGold(amount);
            Log.Message($"Added {amount:N0} gold.");
        });

        internal static void AddInfluence(float amount) => Run("Add Influence", () =>
        {
            Clan.PlayerClan.Influence += amount;
            Log.Message($"Added {amount:N0} influence.");
        });

        internal static void AddRenown(float amount) => Run("Add Renown", () =>
        {
            Clan.PlayerClan.AddRenown(amount, true);
            Log.Message($"Added {amount:N0} renown.");
        });

        internal static void AddPoints(int attributePoints, int focusPoints) => Run("Add Points", () =>
        {
            var dev = Ext.MainHero.HeroDeveloper;
            dev.UnspentAttributePoints += attributePoints;
            dev.UnspentFocusPoints += focusPoints;
            Log.Message($"Added {attributePoints} attribute and {focusPoints} focus points.");
        });

        internal static void MaxSkills(int level) => Run("Max Skills", () =>
        {
            foreach (var skill in Skills.All)
            {
                if (Ext.MainHero.GetSkillValue(skill) < level)
                    Ext.MainHero.HeroDeveloper.SetInitialSkillLevel(skill, level);
            }
            Log.Message($"All skills set to {level}.");
        });

        internal static void MaxAttributes() => MaxAttributes(Ext.MainHero);

        internal static void MaxAttributes(Hero hero) => Run("Max Attributes", () =>
        {
            hero ??= Ext.MainHero;
            int max = Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute;
            foreach (var attribute in new[] { DefaultCharacterAttributes.Vigor, DefaultCharacterAttributes.Control, DefaultCharacterAttributes.Endurance,
                                              DefaultCharacterAttributes.Cunning, DefaultCharacterAttributes.Social, DefaultCharacterAttributes.Intelligence })
            {
                int missing = max - hero.GetAttributeValue(attribute);
                if (missing > 0)
                    hero.HeroDeveloper.AddAttribute(attribute, missing, false);
            }
            Log.Message($"{hero.Name}: all attributes set to {max}.");
        });
    }
}
