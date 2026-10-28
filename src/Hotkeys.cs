using System;
using System.Linq;
using System.Reflection;
using SandBox.GauntletUI;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.ViewModelCollection.Inventory;
using TaleWorlds.CampaignSystem.ViewModelCollection.Party;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;
using static CustomCheats.Ext;

namespace CustomCheats
{
    /// <summary>
    /// Campaign screen hotkeys (same keys as the original Bannerlord Cheats mod). All require "Enable Hotkeys".
    /// Inventory: Ctrl+X +1,000 gold (Shift: +100,000), Ctrl+H +1 selected item (Shift: +10).
    /// Party: Ctrl+H +1 selected troop (Shift: +10), Ctrl+X +XP to selected troop.
    /// Character: Ctrl+A max attributes, Ctrl+1..6 +1 attribute, Ctrl+F +1 focus point, Ctrl+G +1 attribute point.
    /// Clan: Ctrl+X +1,000 influence.
    /// Encyclopedia: Ctrl+H on a hero = play as hero, on a town/castle = take it, on a troop = add troops; Ctrl+X on a hero = kill.
    /// </summary>
    internal static class Hotkeys
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static Type _lastScreen;

        private static bool Ctrl => Input.IsKeyDown(InputKey.LeftControl) || Input.IsKeyDown(InputKey.RightControl);
        private static bool Shift => Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift);

        /// <summary>Called every frame from <see cref="SubModule.OnApplicationTick"/>.</summary>
        internal static void Tick()
        {
            try
            {
                var s = S;
                if (s == null || Campaign.Current == null || MainHero == null)
                    return;
                var screen = ScreenManager.TopScreen;

                if (s.EnableHotkeys && s.EnableHotkeyTips && screen != null && screen.GetType() != _lastScreen)
                    ShowTip(screen);
                _lastScreen = screen?.GetType();

                if (!s.EnableHotkeys || !Ctrl || screen == null)
                    return;

                switch (screen)
                {
                    case GauntletInventoryScreen _: Inventory(screen); break;
                    case GauntletPartyScreen _: Party(screen); break;
                    case GauntletCharacterDeveloperScreen _: Character(screen); break;
                    case GauntletClanScreen _:
                        if (Input.IsKeyPressed(InputKey.X))
                            CampaignCheatBehavior.AddInfluence(1000);
                        break;
                }
            }
            catch (Exception e)
            {
                Patcher.Error("Hotkeys", e);
            }
        }

        private static void ShowTip(ScreenBase screen)
        {
            string tip = screen switch
            {
                GauntletInventoryScreen _ => "Ctrl+X: +1,000 gold (Shift: +100,000) | Ctrl+H: +1 selected item (Shift: +10)",
                GauntletPartyScreen _ => "Ctrl+H: +1 selected troop (Shift: +10) | Ctrl+X: +XP to selected troop",
                GauntletCharacterDeveloperScreen _ => "Ctrl+A: max attributes | Ctrl+1-6: +1 attribute | Ctrl+F: +1 focus | Ctrl+G: +1 attribute point",
                GauntletClanScreen _ => "Ctrl+X: +1,000 influence",
                _ => null,
            };
            if (tip != null)
                Log.Message("Cheat hotkeys: " + tip, Colors.Cyan);
        }

        /// <summary>Screens keep their view model in a private field; find it by type so renames don't matter.</summary>
        private static T ViewModel<T>(object screen) where T : class
        {
            for (var type = screen.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetFields(AnyInstance).FirstOrDefault(f => typeof(T).IsAssignableFrom(f.FieldType));
                if (field != null)
                    return field.GetValue(screen) as T;
                var prop = type.GetProperties(AnyInstance).FirstOrDefault(p => typeof(T).IsAssignableFrom(p.PropertyType) && p.GetIndexParameters().Length == 0);
                if (prop != null)
                    return prop.GetValue(screen) as T;
            }
            return null;
        }

        private static void Inventory(ScreenBase screen)
        {
            if (Input.IsKeyPressed(InputKey.X))
            {
                CampaignCheatBehavior.AddGold(Shift ? 100_000 : 1_000);
            }
            else if (Input.IsKeyPressed(InputKey.H))
            {
                var vm = ViewModel<SPInventoryVM>(screen);
                var item = vm?.GetType().GetField("_selectedItem", AnyInstance)?.GetValue(vm) as SPItemVM;
                if (item?.ItemRosterElement.EquipmentElement.Item == null)
                {
                    Log.Message("Select an item first.", Colors.Yellow);
                    return;
                }
                int count = Shift ? 10 : 1;
                PartyBase.MainParty.ItemRoster.AddToCounts(item.ItemRosterElement.EquipmentElement, count);
                Log.Message($"Added {count}x {item.ItemRosterElement.EquipmentElement.Item.Name}. Press Done (not Cancel) to keep them.");
            }
        }

        private static void Party(ScreenBase screen)
        {
            var character = ViewModel<PartyVM>(screen)?.CurrentCharacter?.Character;
            if (character == null)
                return;
            if (Input.IsKeyPressed(InputKey.H))
            {
                if (character.IsHero)
                    return;
                int count = Shift ? 10 : 1;
                PartyBase.MainParty.AddMember(character, count, 0);
                Log.Message($"Added {count}x {character.Name}. Press Done (not Cancel) to keep them.");
            }
            else if (Input.IsKeyPressed(InputKey.X))
            {
                var roster = PartyBase.MainParty.MemberRoster;
                int index = roster.FindIndexOfTroop(character);
                if (index < 0 || character.IsHero)
                    return;
                int xp = Math.Max(1, character.GetUpgradeXpCost(PartyBase.MainParty, 0)) * Math.Max(1, roster.GetElementNumber(index));
                roster.AddXpToTroopAtIndex(index, xp);
                Log.Message($"Added upgrade XP to {character.Name}. Press Done (not Cancel) to keep it.");
            }
        }

        private static void Character(ScreenBase screen)
        {
            var hero = ViewModel<CharacterDeveloperVM>(screen)?.CurrentCharacter?.Hero ?? MainHero;
            var dev = hero.HeroDeveloper;
            if (Input.IsKeyPressed(InputKey.A))
            {
                CampaignCheatBehavior.MaxAttributes(hero);
            }
            else if (Input.IsKeyPressed(InputKey.F))
            {
                dev.UnspentFocusPoints++;
                Log.Message($"{hero.Name}: +1 focus point. Reopen the screen to see it.");
            }
            else if (Input.IsKeyPressed(InputKey.G))
            {
                dev.UnspentAttributePoints++;
                Log.Message($"{hero.Name}: +1 attribute point. Reopen the screen to see it.");
            }
            else
            {
                var keys = new[] { InputKey.D1, InputKey.D2, InputKey.D3, InputKey.D4, InputKey.D5, InputKey.D6 };
                var attributes = new[] { DefaultCharacterAttributes.Vigor, DefaultCharacterAttributes.Control, DefaultCharacterAttributes.Endurance,
                                         DefaultCharacterAttributes.Cunning, DefaultCharacterAttributes.Social, DefaultCharacterAttributes.Intelligence };
                for (int i = 0; i < keys.Length; i++)
                {
                    if (!Input.IsKeyPressed(keys[i]))
                        continue;
                    if (hero.GetAttributeValue(attributes[i]) < Campaign.Current.Models.CharacterDevelopmentModel.MaxAttribute)
                    {
                        dev.AddAttribute(attributes[i], 1, false);
                        Log.Message($"{hero.Name}: +1 {attributes[i].Name}. Reopen the screen to see it.");
                    }
                    break;
                }
            }
        }

        /// <summary>Postfix on <see cref="EncyclopediaPageVM.OnTick"/>: actions on the open encyclopedia page.</summary>
        internal static void EncyclopediaOnTick(EncyclopediaPageVM __instance)
        {
            try
            {
                if (S?.EnableHotkeys != true || !Ctrl || Campaign.Current == null || MainHero == null)
                    return;
                bool h = Input.IsKeyPressed(InputKey.H), x = Input.IsKeyPressed(InputKey.X);
                if (!h && !x)
                    return;

                switch (__instance?.Obj)
                {
                    case Hero hero when h && hero != MainHero && hero.IsAlive:
                        Confirm($"Play as {hero.Name}?", "You will take control of this hero. This can not be undone.",
                            () => ChangePlayerCharacterAction.Apply(hero));
                        break;
                    case Hero hero when x && hero != MainHero && hero.IsAlive:
                        Confirm($"Kill {hero.Name}?", "This hero will die immediately. This can not be undone.",
                            () => KillCharacterAction.ApplyByMurder(hero, null, true));
                        break;
                    case Settlement settlement when h && (settlement.IsTown || settlement.IsCastle) && settlement.OwnerClan != Clan.PlayerClan:
                        Confirm($"Take {settlement.Name}?", "Ownership of this settlement will be transferred to you.",
                            () => ChangeOwnerOfSettlementAction.ApplyByDefault(MainHero, settlement));
                        break;
                    case CharacterObject troop when h && !troop.IsHero:
                        int count = Shift ? 10 : 1;
                        PartyBase.MainParty.AddMember(troop, count, 0);
                        Log.Message($"Added {count}x {troop.Name} to your party.");
                        break;
                }
            }
            catch (Exception e)
            {
                Patcher.Error("Encyclopedia hotkeys", e);
            }
        }

        private static void Confirm(string title, string text, Action onConfirm) =>
            InformationManager.ShowInquiry(new InquiryData(title, text, true, true, "Confirm", "Cancel", () =>
            {
                try
                {
                    onConfirm();
                }
                catch (Exception e)
                {
                    Patcher.Error(title, e);
                }
            }, null));
    }
}
