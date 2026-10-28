using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;
using static CustomCheats.Ext;

namespace CustomCheats
{
    /// <summary>
    /// Per-mission cheats: god modes, regeneration, infinite ammo, unbreakable shields and battle hotkeys.
    /// Player effects run every tick; party/ally effects are throttled to keep big battles cheap.
    /// </summary>
    internal class CombatCheatBehavior : MissionLogic
    {
        private const float GroupInterval = 0.25f;

        private readonly Dictionary<Agent, Agent.MortalityState> _originalMortality = new Dictionary<Agent, Agent.MortalityState>();
        private float _groupTimer;
        private Agent _playerMount; // kept after dismounting so the horse stays protected

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        /// <summary>Runs before agents spawn, so the player keeps battle gear in towns and villages.</summary>
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            if (S?.NeverRequireCivilianEquipment == true && Mission != null)
                Mission.DoesMissionRequireCivilianEquipment = false;
        }

        public override void OnMissionTick(float dt)
        {
            try
            {
                var s = S;
                var mission = Mission.Current;
                if (s == null || mission == null)
                    return;

                var player = mission.MainAgent;
                if (player != null && player.IsActive())
                {
                    SetInvincible(player, s.Invincible);
                    if (player.MountAgent != null)
                        _playerMount = player.MountAgent;

                    Regenerate(player, s.HealthRegeneration * dt);
                    if (s.InfiniteAmmo)
                        RefillAmmo(player);
                    if (s.UnbreakableShield)
                        RepairShields(player);
                }

                if (_playerMount != null && _playerMount.IsActive())
                    SetInvincible(_playerMount, s.PlayerHorseInvincible);

                _groupTimer += dt;
                if (_groupTimer >= GroupInterval)
                {
                    TickGroup(s, mission, _groupTimer);
                    _groupTimer = 0f;
                }

                if (s.EnableHotkeys)
                    HandleHotkeys(mission);
            }
            catch (Exception e)
            {
                Patcher.Error("Combat cheats", e);
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            _originalMortality.Remove(affectedAgent);
            if (affectedAgent == _playerMount)
                _playerMount = null;
        }

        private void TickGroup(CheatSettings s, Mission mission, float elapsed)
        {
            foreach (var agent in mission.Agents.ToList())
            {
                if (agent == null || !agent.IsActive() || agent.IsMainAgent || agent == _playerMount)
                    continue;

                bool partyTroop = agent.IsPlayerPartyTroop();
                bool invincible = agent.IsHuman && (
                    (partyTroop && (s.PartyInvincible || (agent.IsHero && s.PartyHeroesInvincible))) ||
                    (s.AlliesInvincible && agent.IsPlayerAlly()));
                SetInvincible(agent, invincible);

                if (partyTroop)
                {
                    Regenerate(agent, s.PartyHealthRegeneration * elapsed);
                    if (s.PartyInfiniteAmmo)
                        RefillAmmo(agent);
                }
            }
        }

        /// <summary>Makes an agent invulnerable at full health, and restores its own state when the cheat turns off.</summary>
        private void SetInvincible(Agent agent, bool on)
        {
            if (on)
            {
                if (!_originalMortality.ContainsKey(agent))
                    _originalMortality[agent] = agent.CurrentMortalityState;
                if (agent.CurrentMortalityState != Agent.MortalityState.Invulnerable)
                    agent.SetMortalityState(Agent.MortalityState.Invulnerable);
                if (agent.Health < agent.HealthLimit)
                    agent.Health = agent.HealthLimit;
            }
            else if (_originalMortality.TryGetValue(agent, out var original))
            {
                agent.SetMortalityState(original);
                _originalMortality.Remove(agent);
            }
        }

        private static void Regenerate(Agent agent, float amount)
        {
            if (amount > 0f && agent.Health < agent.HealthLimit)
                agent.Health = Math.Min(agent.HealthLimit, agent.Health + amount);
        }

        private static void RefillAmmo(Agent agent)
        {
            for (var slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
            {
                var weapon = agent.Equipment[slot];
                if (!weapon.IsEmpty && weapon.IsAnyConsumable() && weapon.Amount < weapon.ModifiedMaxAmount)
                    agent.SetWeaponAmountInSlot(slot, weapon.ModifiedMaxAmount, false);
            }
        }

        private static void RepairShields(Agent agent)
        {
            for (var slot = EquipmentIndex.WeaponItemBeginSlot; slot < EquipmentIndex.NumAllWeaponSlots; slot++)
            {
                var weapon = agent.Equipment[slot];
                if (!weapon.IsEmpty && weapon.IsShield() && weapon.HitPoints < weapon.ModifiedMaxHitPoints)
                    agent.ChangeWeaponHitPoints(slot, weapon.ModifiedMaxHitPoints);
            }
        }

        private static void HandleHotkeys(Mission mission)
        {
            if (!Input.IsKeyDown(InputKey.LeftControl) && !Input.IsKeyDown(InputKey.RightControl))
                return;

            if (Input.IsKeyPressed(InputKey.H))
            {
                var player = mission.MainAgent;
                if (player == null)
                    return;
                player.Health = player.HealthLimit;
                if (player.MountAgent != null)
                    player.MountAgent.Health = player.MountAgent.HealthLimit;
                Log.Message("Healed player and mount.");
            }
            else if (Input.IsKeyPressed(InputKey.K))
            {
                var enemies = mission.Agents.Where(a => a != null && a.IsActive() && a.IsHuman && a.IsPlayerEnemy()).ToList();
                foreach (var enemy in enemies)
                    mission.KillAgentCheat(enemy);
                Log.Message($"Killed {enemies.Count} enemies.");
            }
        }
    }
}
