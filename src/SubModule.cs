using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace CustomCheats
{
    public class SubModule : MBSubModuleBase
    {
        private static object _patchedMissionModels;

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();
            Log.Write("Custom Cheats loaded.");
        }

        /// <summary>
        /// Models are final once every module has registered its own (RBM, Diplomacy, ...), so patch here.
        /// Patches are re-applied per campaign so they always target the models that campaign uses.
        /// </summary>
        public override void OnGameInitializationFinished(Game game)
        {
            base.OnGameInitializationFinished(game);
            if (!(game?.GameType is Campaign))
                return;

            try
            {
                Patcher.UnpatchAll();
                _patchedMissionModels = null;

                CombatPatches.Apply();
                CampaignPatches.Apply(Campaign.Current?.Models);
                Patcher.Postfix("Encyclopedia Hotkeys", Patcher.Method(typeof(EncyclopediaPageVM), nameof(EncyclopediaPageVM.OnTick)), typeof(Hotkeys), nameof(Hotkeys.EncyclopediaOnTick));
                PatchMissionModels();

                Report();
            }
            catch (Exception e)
            {
                Log.Write("Patching failed: " + e);
                Log.Message("Custom Cheats failed to apply its patches. See CustomCheats.log.", Colors.Red);
            }
        }

        /// <summary>Remove all patches so cheats never leak into custom battles or multiplayer.</summary>
        public override void OnGameEnd(Game game)
        {
            base.OnGameEnd(game);
            Patcher.UnpatchAll();
            _patchedMissionModels = null;
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            if (Campaign.Current == null || mission == null)
                return;
            PatchMissionModels();
            mission.AddMissionBehavior(new CombatCheatBehavior());
        }

        protected override void OnApplicationTick(float dt)
        {
            base.OnApplicationTick(dt);
            Hotkeys.Tick();
        }

        /// <summary>Mission models can be created later than campaign models, so patch them on first sight.</summary>
        private static void PatchMissionModels()
        {
            var models = MissionGameModels.Current;
            if (models == null || ReferenceEquals(models, _patchedMissionModels))
                return;
            int failedBefore = Patcher.Failed.Count;
            CombatPatches.ApplyModels(models);
            _patchedMissionModels = models;
            if (Patcher.Failed.Count > failedBefore)
                Report();
        }

        private static void Report()
        {
            Log.Write($"Applied {Patcher.AppliedCount} patches.");
            foreach (var failure in Patcher.Failed)
                Log.Write("Patch skipped: " + failure);
            if (Patcher.Failed.Count > 0)
                Log.Message($"Custom Cheats: {Patcher.Failed.Count} cheat(s) unavailable in this game version. See CustomCheats.log.", Colors.Yellow);
        }
    }
}
