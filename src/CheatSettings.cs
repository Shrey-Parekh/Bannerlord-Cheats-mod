using System;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

namespace CustomCheats
{
    /// <summary>All cheats, shown in Esc -> Mod Options -> Custom Cheats. Values are read live by the patches.</summary>
    public class CheatSettings : AttributeGlobalSettings<CheatSettings>
    {
        public override string Id => "CustomCheats_v1";
        public override string DisplayName => "Custom Cheats";
        public override string FolderName => "CustomCheats";
        public override string FormatType => "json2";

        private const string General = "1. General";
        private const string Actions = "2. Instant Actions";
        private const string Player = "3. Combat - Player";
        private const string Party = "4. Combat - Party & Allies";
        private const string Enemies = "5. Combat - Enemies";
        private const string CombatMisc = "6. Combat - Rewards & Misc";
        private const string Map = "7. Map";
        private const string PartyGroup = "8. Party";
        private const string Inventory = "9. Inventory & Trade";
        private const string Clan = "10. Clan";
        private const string Characters = "11. Characters";
        private const string Kingdom = "12. Kingdom";
        private const string Experience = "13. Experience";
        private const string Settlements = "14. Settlements";
        private const string Sieges = "15. Sieges & Armies";
        private const string Smithing = "16. Smithing";
        private const string Workshops = "17. Workshops";

        internal static readonly string[] KnockoutOptions = { "Default", "Always knocked out", "Always killed" };
        private static Dropdown<string> KnockoutDropdown() => new Dropdown<string>(KnockoutOptions, 0);

        // ---------------- General ----------------
        [SettingPropertyBool("Enable Hotkeys", Order = 0, RequireRestart = false, HintText = "Enables all cheat hotkeys (battle: Ctrl+H heal, Ctrl+K kill enemies; screens: see README).")]
        [SettingPropertyGroup(General, GroupOrder = 1)]
        public bool EnableHotkeys { get; set; }

        [SettingPropertyBool("Show Hotkey Tips", Order = 1, RequireRestart = false, HintText = "Shows the available hotkeys when opening a screen that has them.")]
        [SettingPropertyGroup(General, GroupOrder = 1)]
        public bool EnableHotkeyTips { get; set; }

        // ---------------- Instant actions ----------------
        [SettingPropertyButton("Add 1,000,000 Gold", Content = "Add", Order = 0, RequireRestart = false, HintText = "Gives the player 1,000,000 denars.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action AddGold { get; set; } = () => CampaignCheatBehavior.AddGold(1_000_000);

        [SettingPropertyButton("Add 1,000 Influence", Content = "Add", Order = 1, RequireRestart = false, HintText = "Adds 1,000 influence to the player clan.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action AddInfluence { get; set; } = () => CampaignCheatBehavior.AddInfluence(1000);

        [SettingPropertyButton("Add 1,000 Renown", Content = "Add", Order = 2, RequireRestart = false, HintText = "Adds 1,000 renown to the player clan.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action AddRenown { get; set; } = () => CampaignCheatBehavior.AddRenown(1000);

        [SettingPropertyButton("Add 10 Attribute + 10 Focus Points", Content = "Add", Order = 3, RequireRestart = false, HintText = "Adds unspent attribute and focus points to the player.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action AddPoints { get; set; } = () => CampaignCheatBehavior.AddPoints(10, 10);

        [SettingPropertyButton("Max All Player Skills (300)", Content = "Apply", Order = 4, RequireRestart = false, HintText = "Sets every player skill to 300.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action MaxSkills { get; set; } = () => CampaignCheatBehavior.MaxSkills(300);

        [SettingPropertyButton("Max All Player Attributes", Content = "Apply", Order = 5, RequireRestart = false, HintText = "Raises every player attribute to the game maximum.")]
        [SettingPropertyGroup(Actions, GroupOrder = 2)]
        public Action MaxAttributes { get; set; } = () => CampaignCheatBehavior.MaxAttributes();

        // ---------------- Combat - Player ----------------
        [SettingPropertyBool("God Mode", Order = 0, RequireRestart = false, HintText = "Player takes no damage and is kept at full health.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool Invincible { get; set; }

        [SettingPropertyBool("Mount God Mode", Order = 1, RequireRestart = false, HintText = "Player's horse takes no damage and is kept at full health.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool PlayerHorseInvincible { get; set; }

        [SettingPropertyBool("One-Hit Kills", Order = 2, RequireRestart = false, HintText = "Every hit the player lands on an enemy is lethal.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool OneHitKill { get; set; }

        [SettingPropertyFloatingInteger("Damage Multiplier", 1f, 10f, "0.0x", Order = 3, RequireRestart = false, HintText = "Multiplies damage the player deals.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public float DamageMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Damage Taken", 0f, 100f, "0'%'", Order = 4, RequireRestart = false, HintText = "Percentage of damage the player receives.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public float DamageTakenPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Health Regeneration (HP/s)", 0f, 100f, "0", Order = 5, RequireRestart = false, HintText = "Health the player regenerates each second in battle.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public float HealthRegeneration { get; set; }

        [SettingPropertyBool("Infinite Ammo", Order = 6, RequireRestart = false, HintText = "Keeps the player's arrows, bolts and throwing weapons full.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool InfiniteAmmo { get; set; }

        [SettingPropertyBool("Unbreakable Shield", Order = 7, RequireRestart = false, HintText = "Keeps the player's shields at full hit points.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool UnbreakableShield { get; set; }

        [SettingPropertyBool("Always Crush Through Shields", Order = 8, RequireRestart = false, HintText = "Player attacks always crush through blocks.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool AlwaysCrushThroughShields { get; set; }

        [SettingPropertyBool("Slice Through Everyone", Order = 9, RequireRestart = false, HintText = "Player swings never get stuck in a target.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool SliceThroughEveryone { get; set; }

        [SettingPropertyBool("Always Knock Down", Order = 10, RequireRestart = false, HintText = "Player hits always knock enemies down.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool AlwaysKnockDown { get; set; }

        [SettingPropertyBool("Never Knocked Back", Order = 11, RequireRestart = false, HintText = "Player is never knocked back by hits.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool NeverKnockedBackByAttacks { get; set; }

        [SettingPropertyBool("No Stuck Arrows", Order = 12, RequireRestart = false, HintText = "Projectiles do not stick to the player.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool NoStuckArrows { get; set; }

        [SettingPropertyBool("Instant Crossbow Reload", Order = 13, RequireRestart = false, HintText = "Player reloads crossbows instantly.")]
        [SettingPropertyGroup(Player, GroupOrder = 3)]
        public bool InstantCrossbowReload { get; set; }

        // ---------------- Combat - Party & Allies ----------------
        [SettingPropertyBool("Party Invincible", Order = 0, RequireRestart = false, HintText = "All troops of the player's party take no damage.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool PartyInvincible { get; set; }

        [SettingPropertyBool("Party Heroes Invincible", Order = 1, RequireRestart = false, HintText = "Companions and family in the player's party take no damage.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool PartyHeroesInvincible { get; set; }

        [SettingPropertyBool("Allies Invincible", Order = 2, RequireRestart = false, HintText = "Every agent on the player's side (including allied lords) takes no damage.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool AlliesInvincible { get; set; }

        [SettingPropertyBool("Party One-Hit Kills", Order = 3, RequireRestart = false, HintText = "Every hit by the player's party on an enemy is lethal.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool PartyOneHitKill { get; set; }

        [SettingPropertyFloatingInteger("Party Damage Multiplier", 1f, 10f, "0.0x", Order = 4, RequireRestart = false, HintText = "Multiplies damage dealt by the player's party.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public float PartyDamageMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Party Damage Taken", 0f, 100f, "0'%'", Order = 5, RequireRestart = false, HintText = "Percentage of damage the player's party receives.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public float PartyDamageTakenPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Party Health Regeneration (HP/s)", 0f, 100f, "0", Order = 6, RequireRestart = false, HintText = "Health the player's party regenerates each second.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public float PartyHealthRegeneration { get; set; }

        [SettingPropertyBool("Party Infinite Ammo", Order = 7, RequireRestart = false, HintText = "Keeps ammo full for the player's party.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool PartyInfiniteAmmo { get; set; }

        [SettingPropertyBool("Party Never Flees", Order = 8, RequireRestart = false, HintText = "Player's party troops never lose morale and run away.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool NoRunningAway { get; set; }

        [SettingPropertyBool("No Friendly Fire", Order = 9, RequireRestart = false, HintText = "Player and party deal no damage to friendly troops.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public bool NoFriendlyFire { get; set; }

        [SettingPropertyDropdown("Party Troops: Knockout or Killed", Order = 10, RequireRestart = false, HintText = "Fate of the player's regular troops when they fall.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public Dropdown<string> PartyKnockoutOrKilled { get; set; } = KnockoutDropdown();

        [SettingPropertyDropdown("Companions: Knockout or Killed", Order = 11, RequireRestart = false, HintText = "Fate of the player's companions when they fall.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public Dropdown<string> CompanionsKnockoutOrKilled { get; set; } = KnockoutDropdown();

        [SettingPropertyDropdown("Friendly Lords: Knockout or Killed", Order = 12, RequireRestart = false, HintText = "Fate of lords on the player's side when they fall.")]
        [SettingPropertyGroup(Party, GroupOrder = 4)]
        public Dropdown<string> FriendlyLordsKnockoutOrKilled { get; set; } = KnockoutDropdown();

        // ---------------- Combat - Enemies ----------------
        [SettingPropertyDropdown("Enemy Lords: Knockout or Killed", Order = 0, RequireRestart = false, HintText = "Fate of enemy lords when they fall.")]
        [SettingPropertyGroup(Enemies, GroupOrder = 5)]
        public Dropdown<string> EnemyLordsKnockoutOrKilled { get; set; } = KnockoutDropdown();

        [SettingPropertyDropdown("Enemy Troops: Knockout or Killed", Order = 1, RequireRestart = false, HintText = "Fate of enemy troops when they fall.")]
        [SettingPropertyGroup(Enemies, GroupOrder = 5)]
        public Dropdown<string> EnemyTroopsKnockoutOrKilled { get; set; } = KnockoutDropdown();

        [SettingPropertyBool("Enemies Never Flee", Order = 2, RequireRestart = false, HintText = "Enemy troops never lose morale and run away.")]
        [SettingPropertyGroup(Enemies, GroupOrder = 5)]
        public bool EnemiesNoRunningAway { get; set; }

        [SettingPropertyFloatingInteger("Enemy Damage Dealt", 0f, 100f, "0'%'", Order = 3, RequireRestart = false, HintText = "Percentage of normal damage dealt by enemies.")]
        [SettingPropertyGroup(Enemies, GroupOrder = 5)]
        public float EnemyDamagePercentage { get; set; } = 100f;

        // ---------------- Combat - Rewards & Misc ----------------
        [SettingPropertyFloatingInteger("Battle Renown Multiplier", 1f, 1000f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies renown from battles and tournaments.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public float RenownRewardMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Battle Influence Multiplier", 1f, 1000f, "0.0x", Order = 1, RequireRestart = false, HintText = "Multiplies influence from battles.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public float InfluenceRewardMultiplier { get; set; } = 1f;

        [SettingPropertyBool("Always Win Simulated Battles", Order = 2, RequireRestart = false, HintText = "Player's party takes no casualties in auto-resolved battles.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public bool AlwaysWinBattleSimulation { get; set; }

        [SettingPropertyBool("No Troop Sacrifice", Order = 3, RequireRestart = false, HintText = "No troops are lost when escaping, breaking in or breaking out of sieges.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public bool NoTroopSacrifice { get; set; }

        [SettingPropertyInteger("Bandit Hideout Troop Limit", 0, 1000, "0", Order = 4, RequireRestart = false, HintText = "Max troops you can take into a hideout. 0 = game default.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public int BanditHideoutTroopLimit { get; set; }

        [SettingPropertyFloatingInteger("Camera Zoom-Out Multiplier", 1f, 1000f, "0.0x", Order = 5, RequireRestart = false, HintText = "Multiplies the maximum camera zoom distance in battle.")]
        [SettingPropertyGroup(CombatMisc, GroupOrder = 6)]
        public float CombatZoomMultiplier { get; set; } = 1f;

        // ---------------- Map ----------------
        [SettingPropertyFloatingInteger("Map Speed Multiplier", 1f, 100f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies the player party's campaign map speed.")]
        [SettingPropertyGroup(Map, GroupOrder = 7)]
        public float MapSpeedMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Map Visibility Multiplier", 1f, 100f, "0.0x", Order = 1, RequireRestart = false, HintText = "Multiplies the player party's spotting range.")]
        [SettingPropertyGroup(Map, GroupOrder = 7)]
        public float MapVisibilityMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("NPC Map Speed", 0f, 100f, "0'%'", Order = 2, RequireRestart = false, HintText = "Percentage of normal map speed for all other parties.")]
        [SettingPropertyGroup(Map, GroupOrder = 7)]
        public float NpcMapSpeedPercentage { get; set; } = 100f;

        [SettingPropertyBool("Party Invisible on Map", Order = 3, RequireRestart = false, HintText = "Other parties ignore the player's party on the map.")]
        [SettingPropertyGroup(Map, GroupOrder = 7)]
        public bool PartyInvisibleOnMap { get; set; }

        [SettingPropertyBool("Caravans Invisible on Map", Order = 4, RequireRestart = false, HintText = "Other parties ignore the player's caravans.")]
        [SettingPropertyGroup(Map, GroupOrder = 7)]
        public bool CaravansInvisibleOnMap { get; set; }

        // ---------------- Party ----------------
        [SettingPropertyInteger("Extra Party Size", 0, 10000, "0", Order = 0, RequireRestart = false, HintText = "Added to the player party's troop limit.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public int ExtraPartyMemberSize { get; set; }

        [SettingPropertyInteger("Extra Prisoner Size", 0, 10000, "0", Order = 1, RequireRestart = false, HintText = "Added to the player party's prisoner limit.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public int ExtraPartyPrisonerSize { get; set; }

        [SettingPropertyInteger("Extra Party Morale", 0, 100, "0", Order = 2, RequireRestart = false, HintText = "Added to the player party's morale (100 = always max).")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public int ExtraPartyMorale { get; set; }

        [SettingPropertyFloatingInteger("Food Consumption", 0f, 100f, "0'%'", Order = 3, RequireRestart = false, HintText = "Percentage of normal food the player party eats (0% = no food needed).")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public float FoodConsumptionPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Troop Wages", 0f, 100f, "0'%'", Order = 4, RequireRestart = false, HintText = "Percentage of normal wages for the player party (0% = free).")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public float TroopWagesPercentage { get; set; } = 100f;

        [SettingPropertyBool("Free Troop Upgrades", Order = 5, RequireRestart = false, HintText = "Upgrading troops costs no gold.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool FreeTroopUpgrades { get; set; }

        [SettingPropertyBool("Upgrade Without Required Items", Order = 6, RequireRestart = false, HintText = "Troops that need a horse (or other item) to upgrade can upgrade without it.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool UpgradeWithoutItems { get; set; }

        [SettingPropertyBool("Free Companion Hiring", Order = 7, RequireRestart = false, HintText = "Companions cost nothing to hire.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool FreeCompanionHiring { get; set; }

        [SettingPropertyBool("Instant Prisoner Recruitment", Order = 8, RequireRestart = false, HintText = "All prisoners in the player party can be recruited immediately.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool InstantPrisonerRecruitment { get; set; }

        [SettingPropertyBool("No Prisoner Escape", Order = 9, RequireRestart = false, HintText = "Prisoners held by the player's faction never escape.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool NoPrisonerEscape { get; set; }

        [SettingPropertyBool("Instant Escape From Captivity", Order = 10, RequireRestart = false, HintText = "The player escapes immediately when captured.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public bool InstantEscape { get; set; }

        [SettingPropertyFloatingInteger("Party Healing Multiplier", 1f, 100f, "0.0x", Order = 11, RequireRestart = false, HintText = "Multiplies daily healing of the player party.")]
        [SettingPropertyGroup(PartyGroup, GroupOrder = 8)]
        public float PartyHealingMultiplier { get; set; } = 1f;

        // ---------------- Inventory & Trade ----------------
        [SettingPropertyInteger("Extra Carrying Capacity", 0, 1000000, "0", Order = 0, RequireRestart = false, HintText = "Added to the player party's inventory capacity.")]
        [SettingPropertyGroup(Inventory, GroupOrder = 9)]
        public int ExtraInventoryCapacity { get; set; }

        [SettingPropertyBool("Item Spawning in Inventory", Order = 1, RequireRestart = false, HintText = "Opening the inventory outside a trade lists 10 of every item for free.")]
        [SettingPropertyGroup(Inventory, GroupOrder = 9)]
        public bool NativeItemSpawning { get; set; }

        [SettingPropertyFloatingInteger("Item Buying Price", 0f, 100f, "0'%'", Order = 2, RequireRestart = false, HintText = "Percentage of normal price when the player buys items.")]
        [SettingPropertyGroup(Inventory, GroupOrder = 9)]
        public float ItemTradingCostPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Item Selling Price Multiplier", 1f, 1000f, "0.0x", Order = 3, RequireRestart = false, HintText = "Multiplies the price the player gets when selling items.")]
        [SettingPropertyGroup(Inventory, GroupOrder = 9)]
        public float SellingPriceMultiplier { get; set; } = 1f;

        // ---------------- Clan ----------------
        [SettingPropertyInteger("Extra Companion Limit", 0, 100, "0", Order = 0, RequireRestart = false, HintText = "Added to the player clan's companion limit.")]
        [SettingPropertyGroup(Clan, GroupOrder = 10)]
        public int ExtraCompanionLimit { get; set; }

        [SettingPropertyInteger("Extra Clan Party Limit", 0, 100, "0", Order = 1, RequireRestart = false, HintText = "Added to the player clan's party limit.")]
        [SettingPropertyGroup(Clan, GroupOrder = 10)]
        public int ExtraClanPartyLimit { get; set; }

        [SettingPropertyInteger("Extra Clan Party Size", 0, 10000, "0", Order = 2, RequireRestart = false, HintText = "Added to the troop limit of the player clan's other parties.")]
        [SettingPropertyGroup(Clan, GroupOrder = 10)]
        public int ExtraClanPartySize { get; set; }

        // ---------------- Characters ----------------
        [SettingPropertyFloatingInteger("Relation Gain After Battle", 1f, 100f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies relation gained with heroes you help in battle.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public float RelationGainAfterBattleMultiplier { get; set; } = 1f;

        [SettingPropertyBool("Perfect Relationships", Order = 1, RequireRestart = false, HintText = "Every hero treats the player as relation 100.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool PerfectRelationships { get; set; }

        [SettingPropertyBool("Never Die of Old Age", Order = 2, RequireRestart = false, HintText = "Player clan members never die of old age.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool NeverDieOfOldAge { get; set; }

        [SettingPropertyBool("Barter Offers Always Accepted", Order = 3, RequireRestart = false, HintText = "Every barter the player offers is accepted.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool BarterOfferAlwaysAccepted { get; set; }

        [SettingPropertyBool("No Barter Cooldown", Order = 4, RequireRestart = false, HintText = "The player can barter with heroes again immediately.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool NoBarterCooldown { get; set; }

        [SettingPropertyBool("Persuasion Always Succeeds", Order = 5, RequireRestart = false, HintText = "Persuasion attempts always critically succeed.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool ConversationAlwaysSuccessful { get; set; }

        [SettingPropertyBool("Perfect Attraction", Order = 6, RequireRestart = false, HintText = "Every hero is fully attracted to the player.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool PerfectAttraction { get; set; }

        [SettingPropertyBool("Allow Same-Sex Marriage", Order = 7, RequireRestart = false, HintText = "Allows the player to marry heroes of the same sex.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public bool AllowSameSexMarriage { get; set; }

        [SettingPropertyFloatingInteger("Pregnancy Chance Multiplier", 1f, 100f, "0.0x", Order = 8, RequireRestart = false, HintText = "Multiplies daily pregnancy chance in the player clan.")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public float PregnancyChanceMultiplier { get; set; } = 1f;

        [SettingPropertyInteger("Pregnancy Duration (days)", 1, 36, "0", Order = 9, RequireRestart = false, HintText = "Length of pregnancies in days (36 = game default).")]
        [SettingPropertyGroup(Characters, GroupOrder = 11)]
        public int AdjustPregnancyDuration { get; set; } = 36;

        // ---------------- Kingdom ----------------
        [SettingPropertyFloatingInteger("Kingdom Decision Weight", 1f, 1000f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies support for options the player backs in kingdom votes.")]
        [SettingPropertyGroup(Kingdom, GroupOrder = 12)]
        public float KingdomDecisionWeightMultiplier { get; set; } = 1f;

        [SettingPropertyBool("No Relation Loss on Decisions", Order = 1, RequireRestart = false, HintText = "No relation is lost from kingdom decisions, disbanding armies or expelling clans.")]
        [SettingPropertyGroup(Kingdom, GroupOrder = 12)]
        public bool NoRelationshipLossOnDecision { get; set; }

        [SettingPropertyBool("No Crime Rating", Order = 2, RequireRestart = false, HintText = "Crimes never raise the player's crime rating.")]
        [SettingPropertyGroup(Kingdom, GroupOrder = 12)]
        public bool NoCrimeRatingForCrimes { get; set; }

        [SettingPropertyFloatingInteger("Decision Override Influence Cost", 0f, 100f, "0'%'", Order = 3, RequireRestart = false, HintText = "Percentage of influence needed to override a kingdom decision.")]
        [SettingPropertyGroup(Kingdom, GroupOrder = 12)]
        public float DecisionOverrideInfluenceCostPercentage { get; set; } = 100f;

        // ---------------- Experience ----------------
        [SettingPropertyFloatingInteger("Player Skill XP Multiplier", 1f, 100f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies skill XP the player gains.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float ExperienceMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Companion Skill XP Multiplier", 1f, 100f, "0.0x", Order = 1, RequireRestart = false, HintText = "Multiplies skill XP gained by companions in the player clan.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float CompanionExperienceMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Clan Skill XP Multiplier", 1f, 100f, "0.0x", Order = 2, RequireRestart = false, HintText = "Multiplies skill XP gained by family members of the player clan.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float ClanExperienceMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Player Learning Rate Multiplier", 1f, 1000f, "0.0x", Order = 3, RequireRestart = false, HintText = "Multiplies the player's learning rate.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float LearningRateMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Companion Learning Rate Multiplier", 1f, 1000f, "0.0x", Order = 4, RequireRestart = false, HintText = "Multiplies the learning rate of the player's clan members.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float CompanionLearningRateMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Learning Limit Multiplier", 1f, 1000f, "0.0x", Order = 5, RequireRestart = false, HintText = "Multiplies the learning limit of the player clan.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float LearningLimitMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Troop XP Multiplier", 1f, 1000f, "0.0x", Order = 6, RequireRestart = false, HintText = "Multiplies XP troops of the player party earn from hits.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public float TroopExperienceMultiplier { get; set; } = 1f;

        [SettingPropertyBool("Free Focus Point Assignment", Order = 7, RequireRestart = false, HintText = "Assigning focus points costs nothing.")]
        [SettingPropertyGroup(Experience, GroupOrder = 13)]
        public bool FreeFocusPointAssignment { get; set; }

        // ---------------- Settlements ----------------
        [SettingPropertyInteger("Daily Food Bonus", 0, 10000, "0", Order = 0, RequireRestart = false, HintText = "Added to the daily food change of player-owned towns and castles.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyFoodBonus { get; set; }

        [SettingPropertyInteger("Daily Loyalty Bonus", 0, 10000, "0", Order = 1, RequireRestart = false, HintText = "Added to the daily loyalty change of player-owned towns and castles.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyLoyaltyBonus { get; set; }

        [SettingPropertyInteger("Daily Security Bonus", 0, 10000, "0", Order = 2, RequireRestart = false, HintText = "Added to the daily security change of player-owned towns and castles.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailySecurityBonus { get; set; }

        [SettingPropertyInteger("Daily Prosperity Bonus", 0, 10000, "0", Order = 3, RequireRestart = false, HintText = "Added to the daily prosperity change of player-owned towns and castles.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyProsperityBonus { get; set; }

        [SettingPropertyInteger("Daily Hearths Bonus", 0, 10000, "0", Order = 4, RequireRestart = false, HintText = "Added to the daily hearth change of player-owned villages.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyHearthsBonus { get; set; }

        [SettingPropertyInteger("Daily Militia Bonus", 0, 10000, "0", Order = 5, RequireRestart = false, HintText = "Added to the daily militia change of player-owned settlements.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyMilitiaBonus { get; set; }

        [SettingPropertyInteger("Daily Garrison Bonus", 0, 10000, "0", Order = 6, RequireRestart = false, HintText = "Added to the daily garrison change of player-owned towns and castles.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public int DailyGarrisonBonus { get; set; }

        [SettingPropertyFloatingInteger("Construction Speed Multiplier", 1f, 1000f, "0.0x", Order = 7, RequireRestart = false, HintText = "Multiplies building construction in player-owned settlements.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public float ConstructionPowerMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Garrison Wages", 0f, 100f, "0'%'", Order = 8, RequireRestart = false, HintText = "Percentage of normal wages for garrisons in player-owned settlements.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public float GarrisonWagesPercentage { get; set; } = 100f;

        [SettingPropertyBool("Free Troop Recruitment", Order = 9, RequireRestart = false, HintText = "Recruiting volunteers costs no gold.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool FreeTroopRecruitment { get; set; }

        [SettingPropertyBool("Villages Never Raided", Order = 10, RequireRestart = false, HintText = "Villages owned by the player clan can not be raided.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool VillagesNeverRaided { get; set; }

        [SettingPropertyBool("Settlements Never Rebel", Order = 11, RequireRestart = false, HintText = "Player-owned settlements never rebel.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool SettlementsNeverRebel { get; set; }

        [SettingPropertyBool("Disguise Always Works", Order = 12, RequireRestart = false, HintText = "Sneaking into settlements is never detected.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool DisguiseAlwaysWorks { get; set; }

        [SettingPropertyBool("No Bribe to Enter Keep", Order = 13, RequireRestart = false, HintText = "Keeps can be entered without a bribe.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool NoBribeToEnterKeep { get; set; }

        [SettingPropertyBool("Never Require Civilian Equipment", Order = 14, RequireRestart = false, HintText = "The player keeps battle equipment in towns and villages.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public bool NeverRequireCivilianEquipment { get; set; }

        [SettingPropertyFloatingInteger("Tournament Max Bet Multiplier", 1f, 1000f, "0.0x", Order = 15, RequireRestart = false, HintText = "Multiplies the maximum tournament bet.")]
        [SettingPropertyGroup(Settlements, GroupOrder = 14)]
        public float TournamentMaximumBetMultiplier { get; set; } = 1f;

        // ---------------- Sieges & Armies ----------------
        [SettingPropertyFloatingInteger("Siege Construction Speed", 1f, 1000f, "0.0x", Order = 0, RequireRestart = false, HintText = "Multiplies siege engine construction on the player's side (50x = near instant).")]
        [SettingPropertyGroup(Sieges, GroupOrder = 15)]
        public float SiegeBuildingSpeedMultiplier { get; set; } = 1f;

        [SettingPropertyFloatingInteger("Enemy Siege Construction Speed", 0f, 100f, "0'%'", Order = 1, RequireRestart = false, HintText = "Percentage of normal siege engine construction on the enemy side.")]
        [SettingPropertyGroup(Sieges, GroupOrder = 15)]
        public float EnemySiegeBuildingSpeedPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Player Army Cohesion Loss", 0f, 100f, "0'%'", Order = 2, RequireRestart = false, HintText = "Percentage of normal daily cohesion loss for armies led by the player.")]
        [SettingPropertyGroup(Sieges, GroupOrder = 15)]
        public float ArmyCohesionLossPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Faction Army Cohesion Loss", 0f, 100f, "0'%'", Order = 3, RequireRestart = false, HintText = "Percentage of normal daily cohesion loss for other armies of the player's kingdom.")]
        [SettingPropertyGroup(Sieges, GroupOrder = 15)]
        public float FactionArmyCohesionLossPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Player Army Food Consumption", 0f, 100f, "0'%'", Order = 4, RequireRestart = false, HintText = "Percentage of normal food eaten by parties in the player's army.")]
        [SettingPropertyGroup(Sieges, GroupOrder = 15)]
        public float ArmyFoodConsumptionPercentage { get; set; } = 100f;

        // ---------------- Smithing ----------------
        [SettingPropertyFloatingInteger("Smithing Stamina Cost", 0f, 100f, "0'%'", Order = 0, RequireRestart = false, HintText = "Percentage of normal stamina used by smithing, smelting and refining (0% = infinite).")]
        [SettingPropertyGroup(Smithing, GroupOrder = 16)]
        public float SmithingEnergyCostPercentage { get; set; } = 100f;

        [SettingPropertyBool("Unlock All Smithing Parts", Order = 1, RequireRestart = false, HintText = "Every crafting part is available.")]
        [SettingPropertyGroup(Smithing, GroupOrder = 16)]
        public bool UnlockAllParts { get; set; }

        [SettingPropertyFloatingInteger("Smithing Difficulty", 0f, 100f, "0'%'", Order = 2, RequireRestart = false, HintText = "Percentage of normal crafting part difficulty.")]
        [SettingPropertyGroup(Smithing, GroupOrder = 16)]
        public float SmithingDifficultyPercentage { get; set; } = 100f;

        [SettingPropertyFloatingInteger("Smithing Material Cost", 0f, 100f, "0'%'", Order = 3, RequireRestart = false, HintText = "Percentage of normal materials used to craft weapons.")]
        [SettingPropertyGroup(Smithing, GroupOrder = 16)]
        public float SmithingCostPercentage { get; set; } = 100f;

        // ---------------- Workshops ----------------
        [SettingPropertyFloatingInteger("Workshop Buying Cost", 0f, 100f, "0'%'", Order = 0, RequireRestart = false, HintText = "Percentage of normal price when the player buys a workshop.")]
        [SettingPropertyGroup(Workshops, GroupOrder = 17)]
        public float WorkshopBuyingCostPercentage { get; set; } = 100f;
    }
}
