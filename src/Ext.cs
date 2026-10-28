using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace CustomCheats
{
    /// <summary>Null-safe "who does this belong to" checks shared by all patches.</summary>
    internal static class Ext
    {
        internal static CheatSettings S => CheatSettings.Instance;

        // ---------- campaign ----------
        /// <summary>Hero.MainHero throws while a save is loading (Game.Current/PlayerTroop not set yet); this returns null instead.</summary>
        internal static Hero MainHero => (Game.Current?.PlayerTroop as CharacterObject)?.HeroObject;

        internal static bool IsPlayerHero(this Hero hero) => hero != null && hero == MainHero;

        internal static bool IsPlayerClan(this Hero hero) => hero != null && Clan.PlayerClan != null && hero.Clan == Clan.PlayerClan;

        internal static bool IsPlayerCompanion(this Hero hero) => hero != null && hero.IsPlayerClan() && hero.IsPlayerCompanion;

        internal static bool IsMainParty(this MobileParty party) => party != null && party == MobileParty.MainParty;

        internal static bool IsMainParty(this PartyBase party) => party != null && party == PartyBase.MainParty;

        /// <summary>Non-caravan parties led by the player clan (excluding the main party).</summary>
        internal static bool IsPlayerClanParty(this PartyBase party) =>
            party?.MobileParty != null && !party.IsMainParty() && !party.MobileParty.IsCaravan &&
            Clan.PlayerClan != null && party.MobileParty.ActualClan == Clan.PlayerClan;

        internal static bool IsPlayerSettlement(this Settlement settlement) =>
            settlement != null && Clan.PlayerClan != null && settlement.OwnerClan == Clan.PlayerClan;

        internal static bool IsPlayerArmy(this MobileParty party) =>
            party?.Army != null && party.Army.LeaderParty == MobileParty.MainParty;

        // ---------- mission ----------
        internal static PartyBase Party(this Agent agent)
        {
            switch (agent?.Origin)
            {
                case PartyAgentOrigin o: return o.Party;
                case PartyGroupAgentOrigin o: return o.Party;
                case SimpleAgentOrigin o: return o.Party;
                default: return null;
            }
        }

        /// <summary>For a mount, returns its rider; for a human, the agent itself.</summary>
        internal static Agent Human(this Agent agent) => agent == null ? null : agent.IsMount ? agent.RiderAgent : agent;

        internal static bool IsPlayer(this Agent agent) => agent != null && agent.IsMainAgent;

        internal static bool IsPlayerPartyTroop(this Agent agent) =>
            agent != null && !agent.IsMainAgent && agent.IsHuman && agent.Party().IsMainParty();

        internal static bool IsPlayerPartyHero(this Agent agent) => agent.IsPlayerPartyTroop() && agent.IsHero;

        internal static bool IsPlayerAlly(this Agent agent)
        {
            var playerTeam = Mission.Current?.PlayerTeam;
            return playerTeam != null && agent?.Team != null && agent.Team.Side == playerTeam.Side;
        }

        internal static bool IsPlayerEnemy(this Agent agent)
        {
            var playerTeam = Mission.Current?.PlayerTeam;
            return playerTeam != null && agent?.Team != null && agent.Team.Side != playerTeam.Side && agent.Team.Side != BattleSideEnum.None;
        }
    }
}
