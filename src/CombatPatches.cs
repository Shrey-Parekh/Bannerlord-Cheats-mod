using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using static CustomCheats.Ext;

namespace CustomCheats
{
    /// <summary>Battle cheats implemented as Harmony patches.</summary>
    internal static class CombatPatches
    {
        private const float LethalDamage = 10000f;

        internal static void Apply()
        {
            var t = typeof(CombatPatches);

            // Final damage of every agent blow. Runs after all damage models (vanilla, RBM, ...) so multipliers stack on top.
            Patcher.Postfix("Damage", Patcher.Method(typeof(MissionCombatMechanicsHelper), "ComputeBlowDamage"), t, nameof(ComputeBlowDamage));
            Patcher.Prefix("No Stuck Arrows", Patcher.Method(typeof(Mission), nameof(Mission.HandleMissileCollisionReaction)), t, nameof(HandleMissileCollisionReaction));
        }

        /// <summary>Patches the mission models the game actually uses (may be RBM's or another mod's override).</summary>
        internal static void ApplyModels(MissionGameModels models)
        {
            if (models == null)
                return;
            var t = typeof(CombatPatches);

            var dmg = models.AgentApplyDamageModel;
            Patcher.Postfix("Crush Through", Patcher.Impl(dmg, AccessTools.Method(typeof(AgentApplyDamageModel), nameof(AgentApplyDamageModel.DecideCrushedThrough))), t, nameof(DecideCrushedThrough));
            Patcher.Postfix("Knock Down", Patcher.Impl(dmg, AccessTools.Method(typeof(AgentApplyDamageModel), nameof(AgentApplyDamageModel.DecideAgentKnockedDownByBlow))), t, nameof(DecideKnockedDown));
            Patcher.Postfix("Knock Back", Patcher.Impl(dmg, AccessTools.Method(typeof(AgentApplyDamageModel), nameof(AgentApplyDamageModel.DecideAgentKnockedBackByBlow))), t, nameof(DecideKnockedBack));
            Patcher.Postfix("Slice Through", Patcher.Impl(dmg, AccessTools.Method(typeof(AgentApplyDamageModel), nameof(AgentApplyDamageModel.DecideWeaponCollisionReaction))), t, nameof(DecideWeaponCollisionReaction));
            Patcher.Postfix("Slice Through (couched)", Patcher.Impl(dmg, AccessTools.Method(typeof(AgentApplyDamageModel), nameof(AgentApplyDamageModel.DecidePassiveAttackCollisionReaction))), t, nameof(DecidePassiveAttackCollisionReaction));

            Patcher.Postfix("Knockout or Killed", Patcher.Impl(models.AgentDecideKilledOrUnconsciousModel, AccessTools.Method(typeof(AgentDecideKilledOrUnconsciousModel), nameof(AgentDecideKilledOrUnconsciousModel.GetAgentStateProbability))), t, nameof(GetAgentStateProbability));
            Patcher.Postfix("Never Flee", Patcher.Impl(models.BattleMoraleModel, AccessTools.Method(typeof(BattleMoraleModel), nameof(BattleMoraleModel.CalculateMoraleChangeToCharacter))), t, nameof(CalculateMoraleChange));

            var stats = models.AgentStatCalculateModel;
            Patcher.Postfix("Instant Crossbow Reload", Patcher.Impl(stats, AccessTools.Method(typeof(AgentStatCalculateModel), nameof(AgentStatCalculateModel.UpdateAgentStats))), t, nameof(UpdateAgentStats));
            Patcher.Postfix("Camera Zoom", Patcher.Impl(stats, AccessTools.Method(typeof(AgentStatCalculateModel), nameof(AgentStatCalculateModel.GetMaxCameraZoom))), t, nameof(GetMaxCameraZoom));
        }

        private static void ComputeBlowDamage(ref AttackInformation attackInformation, ref int inflictedDamage)
        {
            try
            {
                var s = S;
                if (s == null || inflictedDamage <= 0)
                    return;

                var victim = attackInformation.VictimAgent;
                var attacker = attackInformation.AttackerAgent.Human();
                float damage = inflictedDamage;

                if (attackInformation.IsFriendlyFire)
                {
                    if (s.NoFriendlyFire && (attacker.IsPlayer() || attacker.IsPlayerPartyTroop()))
                        damage = 0f;
                }
                else if (attacker.IsPlayer())
                {
                    damage = s.OneHitKill ? LethalDamage : damage * s.DamageMultiplier;
                }
                else if (attacker.IsPlayerPartyTroop())
                {
                    damage = s.PartyOneHitKill ? LethalDamage : damage * s.PartyDamageMultiplier;
                }
                else if (attacker.IsPlayerEnemy())
                {
                    damage *= s.EnemyDamagePercentage / 100f;
                }

                if (victim.IsPlayer())
                    damage *= s.DamageTakenPercentage / 100f;
                else if (victim.IsPlayerPartyTroop())
                    damage *= s.PartyDamageTakenPercentage / 100f;

                inflictedDamage = (int)Math.Round(damage);
            }
            catch (Exception e)
            {
                Patcher.Error("Damage", e);
            }
        }

        private static void HandleMissileCollisionReaction(ref Mission.MissileCollisionReaction collisionReaction, Agent attachedAgent)
        {
            try
            {
                if (S?.NoStuckArrows == true && collisionReaction == Mission.MissileCollisionReaction.Stick && attachedAgent.IsPlayer())
                    collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
            }
            catch (Exception e)
            {
                Patcher.Error("No Stuck Arrows", e);
            }
        }

        // Model postfixes use positional (__N) arguments so they keep working if an overriding mod renames parameters.
        private static void DecideCrushedThrough(Agent __0, ref bool __result)
        {
            if (S?.AlwaysCrushThroughShields == true && __0.IsPlayer())
                __result = true;
        }

        private static void DecideKnockedDown(Agent __0, Agent __1, ref bool __result)
        {
            if (S?.AlwaysKnockDown == true && __0.IsPlayer() && __1.IsPlayerEnemy())
                __result = true;
        }

        private static void DecideKnockedBack(Agent __1, ref bool __result)
        {
            if (S?.NeverKnockedBackByAttacks == true && __1.IsPlayer())
                __result = false;
        }

        private static void DecideWeaponCollisionReaction(Agent __2, ref MeleeCollisionReaction __8)
        {
            if (S?.SliceThroughEveryone == true && __2.IsPlayer())
                __8 = MeleeCollisionReaction.SlicedThrough;
        }

        private static void DecidePassiveAttackCollisionReaction(Agent __0, ref MeleeCollisionReaction __result)
        {
            if (S?.SliceThroughEveryone == true && __0.IsPlayer())
                __result = MeleeCollisionReaction.SlicedThrough;
        }

        /// <summary>Result is the chance the agent dies instead of being knocked out.</summary>
        private static void GetAgentStateProbability(Agent __1, ref float __result)
        {
            try
            {
                var s = S;
                if (s == null || __1 == null || __1.IsMainAgent || !__1.IsHuman)
                    return;

                int choice;
                if (__1.IsPlayerPartyTroop())
                    choice = __1.IsHero ? s.CompanionsKnockoutOrKilled.SelectedIndex : s.PartyKnockoutOrKilled.SelectedIndex;
                else if (__1.IsPlayerAlly())
                    choice = __1.IsHero ? s.FriendlyLordsKnockoutOrKilled.SelectedIndex : 0;
                else if (__1.IsPlayerEnemy())
                    choice = __1.IsHero ? s.EnemyLordsKnockoutOrKilled.SelectedIndex : s.EnemyTroopsKnockoutOrKilled.SelectedIndex;
                else
                    return;

                if (choice == 1)
                    __result = 0f;
                else if (choice == 2)
                    __result = 1f;
            }
            catch (Exception e)
            {
                Patcher.Error("Knockout or Killed", e);
            }
        }

        private static void CalculateMoraleChange(Agent __0, ref float __result)
        {
            var s = S;
            if (s == null || __result >= 0f)
                return;
            if ((s.NoRunningAway && (__0.IsPlayerPartyTroop() || __0.IsPlayer())) || (s.EnemiesNoRunningAway && __0.IsPlayerEnemy()))
                __result = 0f;
        }

        private static void UpdateAgentStats(Agent __0, AgentDrivenProperties __1)
        {
            if (S?.InstantCrossbowReload == true && __0.IsPlayer() && __1 != null)
                __1.ReloadSpeed = Math.Max(__1.ReloadSpeed, 10f);
        }

        private static void GetMaxCameraZoom(Agent __0, ref float __result)
        {
            var s = S;
            if (s != null && s.CombatZoomMultiplier > 1f && __0.IsPlayer())
                __result *= s.CombatZoomMultiplier;
        }
    }
}
