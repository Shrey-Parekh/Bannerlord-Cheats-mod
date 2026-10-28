using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using SandBox.Tournaments.MissionLogics;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.BarterSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static CustomCheats.Ext;

namespace CustomCheats
{
    /// <summary>
    /// Campaign cheats. Model methods are patched on the registered model's runtime type (see <see cref="Patcher.Impl"/>),
    /// and postfixes take positional arguments (__N) so they survive parameter renames in overriding mods.
    /// </summary>
    internal static class CampaignPatches
    {
        private static readonly TextObject Label = new TextObject("{=!}Custom Cheats");

        internal static void Apply(GameModels m)
        {
            if (m == null)
                return;

            // ----- Map -----
            Model("Map Speed", m.PartySpeedCalculatingModel, typeof(PartySpeedModel), nameof(PartySpeedModel.CalculateFinalSpeed), nameof(CalculateFinalSpeed));
            Model("Map Visibility", m.MapVisibilityModel, typeof(MapVisibilityModel), nameof(MapVisibilityModel.GetPartySpottingRange), nameof(GetPartySpottingRange));
            Patcher.Postfix("Invisible on Map", Patcher.Getter(typeof(MobileParty), nameof(MobileParty.ShouldBeIgnored)), typeof(CampaignPatches), nameof(ShouldBeIgnored));

            // ----- Party -----
            Model("Party Size", m.PartySizeLimitModel, typeof(PartySizeLimitModel), nameof(PartySizeLimitModel.GetPartyMemberSizeLimit), nameof(GetPartyMemberSizeLimit));
            Model("Prisoner Size", m.PartySizeLimitModel, typeof(PartySizeLimitModel), nameof(PartySizeLimitModel.GetPartyPrisonerSizeLimit), nameof(GetPartyPrisonerSizeLimit));
            Model("Party Morale", m.PartyMoraleModel, typeof(PartyMoraleModel), nameof(PartyMoraleModel.GetEffectivePartyMorale), nameof(GetEffectivePartyMorale));
            Model("Food Consumption", m.MobilePartyFoodConsumptionModel, typeof(MobilePartyFoodConsumptionModel), nameof(MobilePartyFoodConsumptionModel.CalculateDailyFoodConsumptionf), nameof(CalculateDailyFoodConsumption));
            Model("Wages", m.PartyWageModel, typeof(PartyWageModel), nameof(PartyWageModel.GetTotalWage), nameof(GetTotalWage));
            Model("Free Troop Recruitment", m.PartyWageModel, typeof(PartyWageModel), nameof(PartyWageModel.GetTroopRecruitmentCost), nameof(GetTroopRecruitmentCost));
            Model("Free Troop Upgrades", m.PartyTroopUpgradeModel, typeof(PartyTroopUpgradeModel), nameof(PartyTroopUpgradeModel.GetGoldCostForUpgrade), nameof(GetGoldCostForUpgrade));
            Model("Upgrade Without Items", m.PartyTroopUpgradeModel, typeof(PartyTroopUpgradeModel), nameof(PartyTroopUpgradeModel.DoesPartyHaveRequiredItemsForUpgrade), nameof(DoesPartyHaveRequiredItemsForUpgrade));
            Patcher.Postfix("Upgrade Without Items (party screen)", Patcher.Method(typeof(PartyScreenLogic), nameof(PartyScreenLogic.ValidateCommand)), typeof(CampaignPatches), nameof(ValidateCommand));
            Model("Free Companion Hiring", m.CompanionHiringPriceCalculationModel, typeof(CompanionHiringPriceCalculationModel), nameof(CompanionHiringPriceCalculationModel.GetCompanionHiringPrice), nameof(GetCompanionHiringPrice));
            Model("Instant Prisoner Recruitment", m.PrisonerRecruitmentCalculationModel, typeof(PrisonerRecruitmentCalculationModel), nameof(PrisonerRecruitmentCalculationModel.CalculateRecruitableNumber), nameof(CalculateRecruitableNumber));
            Model("Party Healing (troops)", m.PartyHealingModel, typeof(PartyHealingModel), nameof(PartyHealingModel.GetDailyHealingForRegulars), nameof(PartyHealing));
            Model("Party Healing (heroes)", m.PartyHealingModel, typeof(PartyHealingModel), nameof(PartyHealingModel.GetDailyHealingHpForHeroes), nameof(PartyHealing));
            Model("Carrying Capacity", m.InventoryCapacityModel, typeof(InventoryCapacityModel), nameof(InventoryCapacityModel.CalculateInventoryCapacity), nameof(CalculateInventoryCapacity));
            Patcher.Prefix("No Prisoner Escape", Patcher.Method(typeof(EndCaptivityAction), nameof(EndCaptivityAction.ApplyByEscape)), typeof(CampaignPatches), nameof(ApplyByEscape));
            Patcher.Postfix("Instant Escape", Patcher.Method(typeof(PlayerCaptivityCampaignBehavior), nameof(PlayerCaptivityCampaignBehavior.CheckCaptivityChange)), typeof(CampaignPatches), nameof(CheckCaptivityChange));
            Patcher.Prefix("Item Spawning", typeof(InventoryLogic).GetMethods().FirstOrDefault(x =>
                x.Name == nameof(InventoryLogic.Initialize) && x.GetParameters().Length > 1 && x.GetParameters()[1].ParameterType == typeof(MobileParty)), typeof(CampaignPatches), nameof(InventoryInitialize));

            // ----- Clan -----
            Model("Companion Limit", m.ClanTierModel, typeof(ClanTierModel), nameof(ClanTierModel.GetCompanionLimit), nameof(GetCompanionLimit));
            Model("Clan Party Limit", m.ClanTierModel, typeof(ClanTierModel), nameof(ClanTierModel.GetPartyLimitForTier), nameof(GetPartyLimitForTier));

            // ----- Characters -----
            Model("Relation After Battle", m.BattleRewardModel, typeof(BattleRewardModel), nameof(BattleRewardModel.GetPlayerGainedRelationAmount), nameof(GetPlayerGainedRelationAmount));
            Patcher.Postfix("Perfect Relationships", Patcher.Method(typeof(Hero), nameof(Hero.GetRelation)), typeof(CampaignPatches), nameof(GetRelation));
            Patcher.Prefix("Never Die of Old Age", Patcher.Method(typeof(KillCharacterAction), nameof(KillCharacterAction.ApplyByOldAge)), typeof(CampaignPatches), nameof(ApplyByOldAge));
            Patcher.Postfix("Barter Always Accepted", Patcher.Method(typeof(BarterManager), nameof(BarterManager.IsOfferAcceptable), new[] { typeof(BarterData), typeof(Hero), typeof(PartyBase) }), typeof(CampaignPatches), nameof(IsOfferAcceptable));
            Patcher.Postfix("No Barter Cooldown", Patcher.Method(typeof(BarterManager), nameof(BarterManager.CanPlayerBarterWithHero)), typeof(CampaignPatches), nameof(CanPlayerBarterWithHero));
            Model("Persuasion", m.PersuasionModel, typeof(PersuasionModel), nameof(PersuasionModel.GetChances), nameof(GetChances));
            Model("Perfect Attraction", m.RomanceModel, typeof(RomanceModel), nameof(RomanceModel.GetAttractionValuePercentage), nameof(GetAttractionValuePercentage));
            Model("Same-Sex Marriage", m.MarriageModel, typeof(MarriageModel), nameof(MarriageModel.IsCoupleSuitableForMarriage), nameof(IsCoupleSuitableForMarriage));
            Model("Pregnancy Chance", m.PregnancyModel, typeof(PregnancyModel), nameof(PregnancyModel.GetDailyChanceOfPregnancyForHero), nameof(GetDailyChanceOfPregnancyForHero));
            Patcher.Postfix("Pregnancy Duration", m.PregnancyModel == null ? null : Patcher.Getter(m.PregnancyModel.GetType(), nameof(PregnancyModel.PregnancyDurationInDays)), typeof(CampaignPatches), nameof(PregnancyDurationInDays));

            // ----- Kingdom -----
            Patcher.Postfix("Kingdom Decision Weight", Patcher.Getter(typeof(DecisionOutcome), nameof(DecisionOutcome.TotalSupportPoints)), typeof(CampaignPatches), nameof(TotalSupportPoints));
            Model("No Relation Loss (army)", m.DiplomacyModel, typeof(DiplomacyModel), nameof(DiplomacyModel.GetRelationCostOfDisbandingArmy), nameof(NoRelationLoss));
            Model("No Relation Loss (expel)", m.DiplomacyModel, typeof(DiplomacyModel), nameof(DiplomacyModel.GetRelationCostOfExpellingClanFromKingdom), nameof(NoRelationLoss));
            Patcher.Postfix("No Relation Loss (election)", Patcher.Method(typeof(KingdomElection), nameof(KingdomElection.GetRelationChangeWithSponsor)), typeof(CampaignPatches), nameof(GetRelationChangeWithSponsor));
            Patcher.Prefix("No Crime Rating", Patcher.Method(typeof(ChangeCrimeRatingAction), nameof(ChangeCrimeRatingAction.Apply)), typeof(CampaignPatches), nameof(ChangeCrimeRating));
            Model("Decision Override Cost", m.ClanPoliticsModel, typeof(ClanPoliticsModel), nameof(ClanPoliticsModel.GetInfluenceRequiredToOverrideKingdomDecision), nameof(GetInfluenceRequiredToOverride));

            // ----- Experience -----
            Patcher.Prefix("Skill XP", Patcher.Method(typeof(HeroDeveloper), nameof(HeroDeveloper.AddSkillXp)), typeof(CampaignPatches), nameof(AddSkillXp));
            Patcher.Postfix("Free Focus Points", Patcher.Method(typeof(HeroDeveloper), nameof(HeroDeveloper.GetRequiredFocusPointsToAddFocus)), typeof(CampaignPatches), nameof(GetRequiredFocusPointsToAddFocus));
            Model("Learning Rate", m.CharacterDevelopmentModel, typeof(CharacterDevelopmentModel), nameof(CharacterDevelopmentModel.CalculateLearningRate), nameof(CalculateLearningRate));
            Model("Learning Limit", m.CharacterDevelopmentModel, typeof(CharacterDevelopmentModel), nameof(CharacterDevelopmentModel.CalculateLearningLimit), nameof(CalculateLearningLimit));
            Model("Troop XP", m.CombatXpModel, typeof(CombatXpModel), nameof(CombatXpModel.GetXpFromHit), nameof(GetXpFromHit));

            // ----- Battle rewards / auto-resolve -----
            Model("Renown Reward", m.BattleRewardModel, typeof(BattleRewardModel), nameof(BattleRewardModel.CalculateRenownGain), nameof(CalculateRenownGain));
            Model("Influence Reward", m.BattleRewardModel, typeof(BattleRewardModel), nameof(BattleRewardModel.CalculateInfluenceGain), nameof(CalculateInfluenceGain));
            Model("Tournament Renown", m.TournamentModel, typeof(TournamentModel), nameof(TournamentModel.GetRenownReward), nameof(GetTournamentRenownReward));
            Model("Always Win Simulation", m.CombatSimulationModel, typeof(CombatSimulationModel), nameof(CombatSimulationModel.SimulateHit), nameof(SimulateHit),
                new[] { typeof(CharacterObject), typeof(CharacterObject), typeof(PartyBase), typeof(PartyBase), typeof(float), typeof(MapEvent), typeof(float), typeof(float) });
            Model("No Troop Sacrifice (break in)", m.TroopSacrificeModel, typeof(TroopSacrificeModel), nameof(TroopSacrificeModel.GetLostTroopCountForBreakingInBesiegedSettlement), nameof(LostTroopCount));
            Model("No Troop Sacrifice (break out)", m.TroopSacrificeModel, typeof(TroopSacrificeModel), nameof(TroopSacrificeModel.GetLostTroopCountForBreakingOutOfBesiegedSettlement), nameof(LostTroopCount));
            Model("No Troop Sacrifice (escape)", m.TroopSacrificeModel, typeof(TroopSacrificeModel), nameof(TroopSacrificeModel.GetNumberOfTroopsSacrificedForTryingToGetAway), nameof(TroopsSacrificedToGetAway));
            Model("Hideout Troop Limit", m.BanditDensityModel, typeof(BanditDensityModel), nameof(BanditDensityModel.GetMaximumTroopCountForHideoutMission), nameof(GetMaximumTroopCountForHideoutMission));

            // ----- Sieges & armies -----
            Model("Siege Construction", m.SiegeEventModel, typeof(SiegeEventModel), nameof(SiegeEventModel.GetConstructionProgressPerHour), nameof(GetConstructionProgressPerHour));
            Model("Army Cohesion", m.ArmyManagementCalculationModel, typeof(ArmyManagementCalculationModel), nameof(ArmyManagementCalculationModel.CalculateDailyCohesionChange), nameof(CalculateDailyCohesionChange));

            // ----- Settlements -----
            Model("Daily Food", m.SettlementFoodModel, typeof(SettlementFoodModel), nameof(SettlementFoodModel.CalculateTownFoodStocksChange), nameof(TownFood));
            Model("Daily Loyalty", m.SettlementLoyaltyModel, typeof(SettlementLoyaltyModel), nameof(SettlementLoyaltyModel.CalculateLoyaltyChange), nameof(TownLoyalty));
            Model("Daily Security", m.SettlementSecurityModel, typeof(SettlementSecurityModel), nameof(SettlementSecurityModel.CalculateSecurityChange), nameof(TownSecurity));
            Model("Daily Prosperity", m.SettlementProsperityModel, typeof(SettlementProsperityModel), nameof(SettlementProsperityModel.CalculateProsperityChange), nameof(TownProsperity));
            Model("Daily Hearths", m.SettlementProsperityModel, typeof(SettlementProsperityModel), nameof(SettlementProsperityModel.CalculateHearthChange), nameof(VillageHearths));
            Model("Daily Militia", m.SettlementMilitiaModel, typeof(SettlementMilitiaModel), nameof(SettlementMilitiaModel.CalculateMilitiaChange), nameof(SettlementMilitia));
            Model("Daily Garrison", m.SettlementGarrisonModel, typeof(SettlementGarrisonModel), nameof(SettlementGarrisonModel.CalculateBaseGarrisonChange), nameof(SettlementGarrison));
            Model("Construction Speed", m.BuildingConstructionModel, typeof(BuildingConstructionModel), nameof(BuildingConstructionModel.CalculateDailyConstructionPower), nameof(CalculateDailyConstructionPower));
            Model("Disguise Always Works", m.DisguiseDetectionModel, typeof(DisguiseDetectionModel), nameof(DisguiseDetectionModel.CalculateDisguiseDetectionProbability), nameof(CalculateDisguiseDetectionProbability));
            Model("No Bribe for Keep", m.BribeCalculationModel, typeof(BribeCalculationModel), nameof(BribeCalculationModel.IsBribeNotNeededToEnterKeep), nameof(IsBribeNotNeededToEnterKeep));
            Model("Item Prices", m.TradeItemPriceFactorModel, typeof(TradeItemPriceFactorModel), nameof(TradeItemPriceFactorModel.GetPrice), nameof(GetPrice));
            Patcher.Postfix("Never Rebel", Patcher.Method(typeof(RebellionsCampaignBehavior), "CheckRebellionEvent"), typeof(CampaignPatches), nameof(CheckRebellionEvent));
            Patcher.Prefix("Villages Never Raided", Patcher.Method(typeof(StartBattleAction), nameof(StartBattleAction.ApplyStartRaid)), typeof(CampaignPatches), nameof(ApplyStartRaid));
            Patcher.Postfix("Tournament Max Bet", Patcher.Method(typeof(TournamentBehavior), nameof(TournamentBehavior.GetMaximumBet)), typeof(CampaignPatches), nameof(GetMaximumBet));

            // ----- Smithing -----
            Model("Smithing Stamina (smith)", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.GetEnergyCostForSmithing), nameof(SmithingEnergy));
            Model("Smithing Stamina (smelt)", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.GetEnergyCostForSmelting), nameof(SmithingEnergy));
            Model("Smithing Stamina (refine)", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.GetEnergyCostForRefining), nameof(SmithingEnergy));
            Model("Smithing Part Difficulty", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.GetCraftingPartDifficulty), nameof(SmithingDifficulty));
            Model("Smithing Design Difficulty", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.CalculateWeaponDesignDifficulty), nameof(SmithingDifficulty));
            Model("Smithing Material Cost", m.SmithingModel, typeof(SmithingModel), nameof(SmithingModel.GetSmithingCostsForWeaponDesign), nameof(GetSmithingCostsForWeaponDesign));
            Patcher.Postfix("Unlock All Parts", Patcher.Method(typeof(CraftingCampaignBehavior), nameof(CraftingCampaignBehavior.IsOpened)), typeof(CampaignPatches), nameof(IsOpened));

            // ----- Workshops -----
            Model("Workshop Buying Cost", m.WorkshopModel, typeof(WorkshopModel), nameof(WorkshopModel.GetCostForPlayer), nameof(GetWorkshopCostForPlayer));
        }

        private static void Model(string label, object model, Type baseType, string method, string patch, Type[] args = null) =>
            Patcher.Postfix(label, Patcher.Impl(model, AccessTools.Method(baseType, method, args)), typeof(CampaignPatches), patch);

        // ---------- ExplainedNumber helpers (Add scales with factors, so solve for the exact delta) ----------
        private static void SetResult(ref ExplainedNumber n, float target)
        {
            float scale = 1f + n.SumOfFactors;
            if (Math.Abs(scale) < 1e-4f)
                return;
            float delta = (target - n.ResultNumber) / scale;
            if (Math.Abs(delta) > 1e-5f)
                n.Add(delta, Label);
        }

        private static void Multiply(ref ExplainedNumber n, float factor)
        {
            if (Math.Abs(factor - 1f) > 1e-4f)
                SetResult(ref n, n.ResultNumber * factor);
        }

        private static void AddFlat(ref ExplainedNumber n, float value)
        {
            if (value != 0f)
                SetResult(ref n, n.ResultNumber + value);
        }

        private static int Scale(int value, float factor) => (int)Math.Round(value * factor);

        private static bool Owned(Settlement settlement) => settlement.IsPlayerSettlement();

        // ---------- Map ----------
        private static void CalculateFinalSpeed(MobileParty __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null || __0 == null)
                return;
            if (__0.IsMainParty())
                Multiply(ref __result, s.MapSpeedMultiplier);
            else
                Multiply(ref __result, s.NpcMapSpeedPercentage / 100f);
        }

        private static void GetPartySpottingRange(MobileParty __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                Multiply(ref __result, S.MapVisibilityMultiplier);
        }

        private static void ShouldBeIgnored(MobileParty __instance, ref bool __result)
        {
            var s = S;
            if (s == null || __result || __instance == null)
                return;
            if ((s.PartyInvisibleOnMap && __instance.IsMainParty()) ||
                (s.CaravansInvisibleOnMap && __instance.IsCaravan && Clan.PlayerClan != null && __instance.ActualClan == Clan.PlayerClan))
                __result = true;
        }

        // ---------- Party ----------
        private static void GetPartyMemberSizeLimit(PartyBase __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null)
                return;
            if (__0.IsMainParty())
                AddFlat(ref __result, s.ExtraPartyMemberSize);
            else if (__0.IsPlayerClanParty())
                AddFlat(ref __result, s.ExtraClanPartySize);
        }

        private static void GetPartyPrisonerSizeLimit(PartyBase __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                AddFlat(ref __result, S.ExtraPartyPrisonerSize);
        }

        private static void GetEffectivePartyMorale(MobileParty __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                AddFlat(ref __result, S.ExtraPartyMorale);
        }

        private static void CalculateDailyFoodConsumption(MobileParty __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null)
                return;
            if (__0.IsMainParty())
                Multiply(ref __result, s.FoodConsumptionPercentage / 100f);
            else if (__0.IsPlayerArmy())
                Multiply(ref __result, s.ArmyFoodConsumptionPercentage / 100f);
        }

        private static void GetTotalWage(MobileParty __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null || __0 == null)
                return;
            if (__0.IsMainParty())
                Multiply(ref __result, s.TroopWagesPercentage / 100f);
            else if (__0.IsGarrison && Owned(__0.CurrentSettlement))
                Multiply(ref __result, s.GarrisonWagesPercentage / 100f);
        }

        private static void GetTroopRecruitmentCost(Hero __1, ref ExplainedNumber __result)
        {
            if (S?.FreeTroopRecruitment == true && __1.IsPlayerHero())
                SetResult(ref __result, 0f);
        }

        private static void GetGoldCostForUpgrade(PartyBase __0, ref ExplainedNumber __result)
        {
            if (S?.FreeTroopUpgrades == true && __0.IsMainParty())
                SetResult(ref __result, 0f);
        }

        private static void DoesPartyHaveRequiredItemsForUpgrade(PartyBase __0, ref bool __result)
        {
            if (S?.UpgradeWithoutItems == true && __0.IsMainParty())
                __result = true;
        }

        /// <summary>The party screen checks the required item itself; re-run its other upgrade checks without that one.</summary>
        private static void ValidateCommand(PartyScreenLogic __instance, PartyScreenLogic.PartyCommand command, ref bool __result)
        {
            try
            {
                if (__result || S?.UpgradeWithoutItems != true || command == null || command.Code != PartyScreenLogic.PartyCommandCode.UpgradeTroop)
                    return;
                var troop = command.Character;
                var roster = __instance.MemberRosters[(int)command.RosterSide];
                int index = roster.FindIndexOfTroop(troop);
                if (index < 0 || roster.GetElementNumber(index) < command.TotalNumber || command.UpgradeTarget >= troop.UpgradeTargets.Length)
                    return;
                if (roster.GetElementXp(index) < troop.GetUpgradeXpCost(PartyBase.MainParty, command.UpgradeTarget) * command.TotalNumber)
                    return;
                var leader = (command.RosterSide == PartyScreenLogic.PartyRosterSide.Left ? __instance.LeftPartyLeader : __instance.RightPartyLeader)?.HeroObject;
                int gold = troop.GetUpgradeGoldCost(PartyBase.MainParty, command.UpgradeTarget) * command.TotalNumber;
                if (leader == null || leader.Gold + __instance.CurrentData.PartyGoldChangeAmount < gold)
                    return;
                __result = true;
            }
            catch (Exception e)
            {
                Patcher.Error("Upgrade Without Items", e);
            }
        }

        private static void GetCompanionHiringPrice(ref int __result)
        {
            if (S?.FreeCompanionHiring == true)
                __result = 0;
        }

        private static void CalculateRecruitableNumber(PartyBase __0, CharacterObject __1, ref int __result)
        {
            if (S?.InstantPrisonerRecruitment == true && __0.IsMainParty() && __1 != null && !__1.IsHero)
                __result = __0.PrisonRoster.GetTroopRoster().Where(e => e.Character == __1).Sum(e => e.Number);
        }

        private static void PartyHealing(PartyBase __0, bool __1, ref ExplainedNumber __result)
        {
            if (S != null && !__1 && __0.IsMainParty())
                Multiply(ref __result, S.PartyHealingMultiplier);
        }

        private static void CalculateInventoryCapacity(MobileParty __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                AddFlat(ref __result, S.ExtraInventoryCapacity);
        }

        private static bool ApplyByEscape(Hero character)
        {
            try
            {
                var prison = character?.PartyBelongedToAsPrisoner;
                if (S?.NoPrisonerEscape == true && character.IsPrisoner && prison != null && MainHero != null &&
                    prison.MapFaction == MainHero.MapFaction && !character.IsPlayerHero())
                    return false;
            }
            catch (Exception e)
            {
                Patcher.Error("No Prisoner Escape", e);
            }
            return true;
        }

        private static void CheckCaptivityChange()
        {
            try
            {
                if (S?.InstantEscape == true && MainHero != null && MainHero.IsPrisoner)
                    PlayerCaptivity.EndCaptivity();
            }
            catch (Exception e)
            {
                Patcher.Error("Instant Escape", e);
            }
        }

        private static void InventoryInitialize(ItemRoster leftItemRoster, MobileParty party, bool isTrading)
        {
            try
            {
                if (S?.NativeItemSpawning != true || isTrading || !party.IsMainParty() || leftItemRoster == null || Game.Current == null || Game.Current.CheatMode)
                    return;
                foreach (var item in MBObjectManager.Instance.GetObjectTypeList<ItemObject>())
                    leftItemRoster.AddToCounts(item, 10);
            }
            catch (Exception e)
            {
                Patcher.Error("Item Spawning", e);
            }
        }

        // ---------- Clan ----------
        private static void GetCompanionLimit(Clan __0, ref int __result)
        {
            if (S != null && __0 != null && __0 == Clan.PlayerClan)
                __result += S.ExtraCompanionLimit;
        }

        private static void GetPartyLimitForTier(Clan __0, ref int __result)
        {
            if (S != null && __0 != null && __0 == Clan.PlayerClan)
                __result += S.ExtraClanPartyLimit;
        }

        // ---------- Characters ----------
        private static void GetPlayerGainedRelationAmount(ref int __result)
        {
            if (S != null && __result > 0)
                __result = Scale(__result, S.RelationGainAfterBattleMultiplier);
        }

        private static void GetRelation(Hero __instance, Hero otherHero, ref int __result)
        {
            if (S?.PerfectRelationships == true && (__instance.IsPlayerHero() || otherHero.IsPlayerHero()) && __instance != otherHero)
                __result = 100;
        }

        private static bool ApplyByOldAge(Hero victim) => !(S?.NeverDieOfOldAge == true && victim.IsPlayerClan());

        private static void IsOfferAcceptable(BarterData args, ref bool __result)
        {
            if (S?.BarterOfferAlwaysAccepted == true && args != null && (args.OffererHero.IsPlayerHero() || args.OtherHero.IsPlayerHero()))
                __result = true;
        }

        private static void CanPlayerBarterWithHero(ref bool __result)
        {
            if (S?.NoBarterCooldown == true)
                __result = true;
        }

        private static void GetChances(ref float __1, ref float __2, ref float __3, ref float __4)
        {
            if (S?.ConversationAlwaysSuccessful != true)
                return;
            __1 = 0f; // success
            __2 = 1f; // critical success
            __3 = 0f; // critical fail
            __4 = 0f; // fail
        }

        private static void GetAttractionValuePercentage(Hero __0, Hero __1, ref int __result)
        {
            if (S?.PerfectAttraction == true && (__0.IsPlayerHero() || __1.IsPlayerHero()))
                __result = 100;
        }

        private static void IsCoupleSuitableForMarriage(Hero __0, Hero __1, ref bool __result)
        {
            if (__result || S?.AllowSameSexMarriage != true || __0 == null || __1 == null || __0.IsFemale != __1.IsFemale)
                return;
            if (!__0.IsPlayerHero() && !__1.IsPlayerHero())
                return;
            // Re-run the vanilla checks except the sex check. Relatedness is a private helper on the default model,
            // so if a mod's model lacks it we refuse rather than risk allowing close relatives.
            var marriage = Campaign.Current?.Models?.MarriageModel;
            if (marriage == null || __0.Clan == __1.Clan || !__0.CanMarry() || !__1.CanMarry() ||
                !marriage.IsSuitableForMarriage(__0) || !marriage.IsSuitableForMarriage(__1) ||
                (__0.Clan != null && !marriage.IsClanSuitableForMarriage(__0.Clan)) ||
                (__1.Clan != null && !marriage.IsClanSuitableForMarriage(__1.Clan)))
                return;
            var related = AccessTools.Method(marriage.GetType(), "AreHeroesRelated", new[] { typeof(Hero), typeof(Hero), typeof(int) });
            if (related == null || (bool)related.Invoke(marriage, new object[] { __0, __1, 3 }))
                return;
            __result = true;
        }

        private static void GetDailyChanceOfPregnancyForHero(Hero __0, ref float __result)
        {
            if (S != null && __0.IsPlayerClan())
                __result = Math.Min(1f, __result * S.PregnancyChanceMultiplier);
        }

        private static void PregnancyDurationInDays(ref float __result)
        {
            if (S != null && S.AdjustPregnancyDuration != 36)
                __result = S.AdjustPregnancyDuration;
        }

        // ---------- Kingdom ----------
        private static void TotalSupportPoints(DecisionOutcome __instance, ref float __result)
        {
            if (S == null || S.KingdomDecisionWeightMultiplier <= 1f || __instance?.SupporterList == null)
                return;
            foreach (var supporter in __instance.SupporterList)
            {
                if (supporter.IsPlayer)
                {
                    __result *= S.KingdomDecisionWeightMultiplier;
                    return;
                }
            }
        }

        /// <summary>Army disband / clan expel costs have no hero argument, so this applies to everyone.</summary>
        private static void NoRelationLoss(ref int __result)
        {
            if (S?.NoRelationshipLossOnDecision == true && __result < 0)
                __result = 0;
        }

        private static readonly FieldInfo ElectionDecision = AccessTools.Field(typeof(KingdomElection), "_decision");

        /// <summary>Kingdom votes: only protect relations that involve the player.</summary>
        private static void GetRelationChangeWithSponsor(KingdomElection __instance, Hero __0, ref int __result)
        {
            if (S?.NoRelationshipLossOnDecision != true || __result >= 0)
                return;
            var sponsor = (ElectionDecision?.GetValue(__instance) as KingdomDecision)?.ProposerClan;
            if (__0.IsPlayerHero() || (sponsor != null && sponsor == Clan.PlayerClan))
                __result = 0;
        }

        private static void ChangeCrimeRating(ref float deltaCrimeRating)
        {
            if (S?.NoCrimeRatingForCrimes == true && deltaCrimeRating > 0f)
                deltaCrimeRating = 0f;
        }

        private static void GetInfluenceRequiredToOverride(ref int __result)
        {
            if (S != null)
                __result = Scale(__result, S.DecisionOverrideInfluenceCostPercentage / 100f);
        }

        // ---------- Experience ----------
        private static void AddSkillXp(HeroDeveloper __instance, ref float rawXp)
        {
            var s = S;
            var hero = __instance?.Hero;
            if (s == null || hero == null || rawXp <= 0f)
                return;
            if (hero.IsPlayerHero())
                rawXp *= s.ExperienceMultiplier;
            else if (hero.IsPlayerCompanion())
                rawXp *= s.CompanionExperienceMultiplier;
            else if (hero.IsPlayerClan())
                rawXp *= s.ClanExperienceMultiplier;
        }

        private static void GetRequiredFocusPointsToAddFocus(ref int __result)
        {
            if (S?.FreeFocusPointAssignment == true)
                __result = 0;
        }

        /// <summary>Learning models only receive the attribute owner, so map it back to a player clan hero.</summary>
        private static Hero OwnerInPlayerClan(object attributes)
        {
            if (attributes == null || Clan.PlayerClan == null)
                return null;
            if (MainHero != null && ReferenceEquals(MainHero.CharacterAttributes, attributes))
                return MainHero;
            return Clan.PlayerClan.Heroes.FirstOrDefault(h => ReferenceEquals(h.CharacterAttributes, attributes));
        }

        private static void CalculateLearningRate(object __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null || (s.LearningRateMultiplier <= 1f && s.CompanionLearningRateMultiplier <= 1f))
                return;
            var hero = OwnerInPlayerClan(__0);
            if (hero == null)
                return;
            Multiply(ref __result, hero.IsPlayerHero() ? s.LearningRateMultiplier : s.CompanionLearningRateMultiplier);
        }

        private static void CalculateLearningLimit(object __0, ref ExplainedNumber __result)
        {
            if (S != null && S.LearningLimitMultiplier > 1f && OwnerInPlayerClan(__0) != null)
                Multiply(ref __result, S.LearningLimitMultiplier);
        }

        private static void GetXpFromHit(PartyBase __3, ref ExplainedNumber __result)
        {
            if (S != null && __3.IsMainParty())
                Multiply(ref __result, S.TroopExperienceMultiplier);
        }

        // ---------- Battle rewards / auto-resolve ----------
        private static void CalculateRenownGain(PartyBase __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                Multiply(ref __result, S.RenownRewardMultiplier);
        }

        private static void CalculateInfluenceGain(PartyBase __0, ref ExplainedNumber __result)
        {
            if (S != null && __0.IsMainParty())
                Multiply(ref __result, S.InfluenceRewardMultiplier);
        }

        private static void GetTournamentRenownReward(Hero __0, ref int __result)
        {
            if (S != null && __0.IsPlayerHero())
                __result = Scale(__result, S.RenownRewardMultiplier);
        }

        private static void SimulateHit(PartyBase __3, ref ExplainedNumber __result)
        {
            if (S?.AlwaysWinBattleSimulation == true && __3.IsMainParty())
                SetResult(ref __result, 0f);
        }

        private static void LostTroopCount(MobileParty __0, ref ExplainedNumber __result)
        {
            if (S?.NoTroopSacrifice == true && __0.IsMainParty())
                SetResult(ref __result, 0f);
        }

        private static void TroopsSacrificedToGetAway(ref int __result)
        {
            if (S?.NoTroopSacrifice == true)
                __result = 0;
        }

        private static void GetMaximumTroopCountForHideoutMission(MobileParty __0, ref int __result)
        {
            if (S != null && S.BanditHideoutTroopLimit > 0 && __0.IsMainParty())
                __result = S.BanditHideoutTroopLimit;
        }

        // ---------- Sieges & armies ----------
        private static bool HasMainParty(ISiegeEventSide side) =>
            side != null && side.GetInvolvedPartiesForEventType(MapEvent.BattleTypes.Siege).Any(p => p.IsMainParty());

        private static void GetConstructionProgressPerHour(SiegeEvent __1, ISiegeEventSide __2, ref float __result)
        {
            try
            {
                var s = S;
                if (s == null || __1 == null || __2 == null)
                    return;
                if (HasMainParty(__2))
                {
                    __result *= s.SiegeBuildingSpeedMultiplier;
                    return;
                }
                var opposite = __2.BattleSide == BattleSideEnum.Attacker ? BattleSideEnum.Defender : BattleSideEnum.Attacker;
                if (HasMainParty(__1.GetSiegeEventSide(opposite)))
                    __result *= s.EnemySiegeBuildingSpeedPercentage / 100f;
            }
            catch (Exception e)
            {
                Patcher.Error("Siege Construction", e);
            }
        }

        private static void CalculateDailyCohesionChange(Army __0, ref ExplainedNumber __result)
        {
            var s = S;
            if (s == null || __0 == null || __result.ResultNumber >= 0f)
                return;
            if (__0.LeaderParty.IsMainParty())
                Multiply(ref __result, s.ArmyCohesionLossPercentage / 100f);
            else if (Clan.PlayerClan?.Kingdom != null && __0.Kingdom == Clan.PlayerClan.Kingdom)
                Multiply(ref __result, s.FactionArmyCohesionLossPercentage / 100f);
        }

        // ---------- Settlements ----------
        private static void TownFood(Town __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                AddFlat(ref __result, S.DailyFoodBonus);
        }

        private static void TownLoyalty(Town __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                AddFlat(ref __result, S.DailyLoyaltyBonus);
        }

        private static void TownSecurity(Town __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                AddFlat(ref __result, S.DailySecurityBonus);
        }

        private static void TownProsperity(Town __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                AddFlat(ref __result, S.DailyProsperityBonus);
        }

        private static void VillageHearths(Village __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                AddFlat(ref __result, S.DailyHearthsBonus);
        }

        private static void SettlementMilitia(Settlement __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0))
                AddFlat(ref __result, S.DailyMilitiaBonus);
        }

        private static void SettlementGarrison(Settlement __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0))
                AddFlat(ref __result, S.DailyGarrisonBonus);
        }

        private static void CalculateDailyConstructionPower(Town __0, ref ExplainedNumber __result)
        {
            if (S != null && Owned(__0?.Settlement))
                Multiply(ref __result, S.ConstructionPowerMultiplier);
        }

        private static void CalculateDisguiseDetectionProbability(ref float __result)
        {
            if (S?.DisguiseAlwaysWorks == true)
                __result = 0f;
        }

        private static void IsBribeNotNeededToEnterKeep(ref bool __result)
        {
            if (S?.NoBribeToEnterKeep == true)
                __result = true;
        }

        private static void GetPrice(MobileParty __1, bool __3, ref int __result)
        {
            var s = S;
            if (s == null || !__1.IsMainParty())
                return;
            __result = __3 ? Scale(__result, s.SellingPriceMultiplier) : Scale(__result, s.ItemTradingCostPercentage / 100f);
        }

        private static void CheckRebellionEvent(Settlement settlement, ref bool __result)
        {
            if (S?.SettlementsNeverRebel == true && Owned(settlement))
                __result = false;
        }

        private static bool ApplyStartRaid(Settlement settlement) => !(S?.VillagesNeverRaided == true && Owned(settlement));

        private static void GetMaximumBet(ref int __result)
        {
            if (S != null)
                __result = Scale(__result, S.TournamentMaximumBetMultiplier);
        }

        // ---------- Smithing ----------
        private static void SmithingEnergy(ref int __result)
        {
            if (S != null)
                __result = Scale(__result, S.SmithingEnergyCostPercentage / 100f);
        }

        private static void SmithingDifficulty(ref int __result)
        {
            if (S != null)
                __result = Scale(__result, S.SmithingDifficultyPercentage / 100f);
        }

        private static void GetSmithingCostsForWeaponDesign(ref int[] __result)
        {
            if (S == null || __result == null || S.SmithingCostPercentage >= 100f)
                return;
            float factor = S.SmithingCostPercentage / 100f;
            // Copy so a model that caches its array is never scaled twice. Costs are negative; scaling keeps the sign.
            __result = __result.Select(cost => Scale(cost, factor)).ToArray();
        }

        private static void IsOpened(ref bool __result)
        {
            if (S?.UnlockAllParts == true)
                __result = true;
        }

        // ---------- Workshops ----------
        private static void GetWorkshopCostForPlayer(ref int __result)
        {
            if (S != null)
                __result = Scale(__result, S.WorkshopBuyingCostPercentage / 100f);
        }
    }
}
