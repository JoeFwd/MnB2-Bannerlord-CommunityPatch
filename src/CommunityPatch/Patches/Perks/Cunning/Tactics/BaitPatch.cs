using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static System.Reflection.BindingFlags;
using static CommunityPatch.HarmonyHelpers;

namespace CommunityPatch.Patches.Perks.Cunning.Tactics {

  public class BaitPatch : PatchBase<BaitPatch> {

    public override bool Applied { get; protected set; }
    
    private static readonly MethodInfo TargetMethodInfo = PlayerEncounterHelper.TargetMethodInfo;
    private static readonly MethodInfo PatchMethodInfo = typeof(BaitPatch).GetMethod(nameof(Postfix), Public | NonPublic | Static | DeclaredOnly);
    
    public override IEnumerable<MethodBase> GetMethodsChecked() {
      yield return TargetMethodInfo;
    }

    private PerkObject _perk;

    private static readonly byte[][] Hashes = PlayerEncounterHelper.TargetHashes;

    public override void Reset()
      => _perk = PerkObject.FindFirst(x => x.Name.GetID() == "6MBoNlxj");

    public override bool? IsApplicable(Game game) {
      if (_perk == null) return false;

      var patchInfo = Harmony.GetPatchInfo(TargetMethodInfo);
      if (AlreadyPatchedByOthers(patchInfo)) return false;

      var hash = TargetMethodInfo.MakeCilSignatureSha256();
      return hash.MatchesAnySha256(Hashes);
    }

    public override void Apply(Game game) {
      var textObjStrings = TextObject.ConvertToStringList(
        new List<TextObject> {
          _perk.Name,
          _perk.Description
        }
      );

      _perk.Initialize(
        textObjStrings[0],
        textObjStrings[1],
        _perk.Skill,
        (int) _perk.RequiredSkillValue,
        _perk.AlternativePerk,
        _perk.PrimaryRole, 30f,
        _perk.SecondaryRole, _perk.SecondaryBonus,
        SkillEffect.EffectIncrementType.AddFactor
      );
      
      if (Applied) return;
      CommunityPatchSubModule.Harmony.Patch(TargetMethodInfo, postfix: new HarmonyMethod(PatchMethodInfo));
      Applied = true;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Postfix(ref PlayerEncounter __instance, ref List<MobileParty> partiesToJoinOurSide, ref List<MobileParty> partiesToJoinEnemySide) {
      var ourParty = PartyBase.MainParty;
      var encounterParty = PlayerEncounterHelper.GetEncounteredParty(__instance);

      AddNearbyAlliesToParty(ourParty, encounterParty, partiesToJoinOurSide);
      AddNearbyAlliesToParty(encounterParty, ourParty, partiesToJoinEnemySide, true);
      
      partiesToJoinEnemySide = partiesToJoinEnemySide.Distinct().ToList();
      partiesToJoinOurSide = partiesToJoinOurSide.Distinct().ToList();
    }

    private static void AddNearbyAlliesToParty(PartyBase party, PartyBase enemyParty, List<MobileParty> alliesFound,  bool allyMustBeAbleToAttack = false) {
      var isSiege = IsSiegeEncounter();
      var position2D = GetEncounterPosition(isSiege);
      var radius = CalculateCallToArmsRadius(party, isSiege);

      var possibleAllies = PlayerEncounterHelper.FindPartiesAroundPosition(position2D, radius);
      
      foreach (var possibleAlly in possibleAllies) {
        if (!IsAlly(party, enemyParty, possibleAlly, allyMustBeAbleToAttack)) continue;

        alliesFound.Add(possibleAlly);
          
        if (possibleAlly.Army != null && possibleAlly.Army.LeaderParty == possibleAlly)
          alliesFound.AddRange(possibleAlly.Army.LeaderParty.AttachedParties);
      }
    }
    
    private static bool IsSiegeEncounter() 
      => PlayerEncounter.EncounteredParty != null && 
        PlayerEncounter.EncounteredParty.IsMobile && 
        PlayerEncounter.EncounteredParty.MobileParty.BesiegedSettlement != null || 
        MobileParty.MainParty.BesiegedSettlement != null;

    private static Vec2 GetEncounterPosition(bool isSiegeEvent) {
      if (!isSiegeEvent) return MobileParty.MainParty.Position2D;

      if (PlayerEncounter.EncounteredParty?.IsMobile == true && PlayerEncounter.EncounteredParty?.MobileParty?.BesiegedSettlement != null)
      {
        if (PlayerEncounter.EncounteredParty.SiegeEvent?.BesiegerCamp?.BesiegerParty != null)
          return PlayerEncounter.EncounteredParty.SiegeEvent.BesiegerCamp.BesiegerParty.Position2D;
      }
      else if (MobileParty.MainParty.BesiegerCamp?.BesiegerParty != null)
        return MobileParty.MainParty.BesiegerCamp.BesiegerParty.Position2D;

      return MobileParty.MainParty.Position2D;
    }
    
    private static float CalculateCallToArmsRadius(PartyBase party, bool isSiege)
    {
      var baseRadius = 3f * (isSiege ? 1.5f : 1f);
      var finalRadius = new ExplainedNumber(baseRadius);
      
      if (party.MobileParty != null)
        PerkHelper.AddPerkBonusForParty(ActivePatch._perk, party.MobileParty, ref finalRadius);
      
      return finalRadius.ResultNumber;
    }
    
    private static bool IsAlly(PartyBase party, PartyBase enemyParty, MobileParty possibleAlly, bool allyMustBeAbleToAttack)
    {
      if (!IsAValidPossibleAlly(party, possibleAlly)) 
        return false;
      
      if (!MapEventHelper.PartyCanJoinSideOf(possibleAlly, party, enemyParty)) 
        return false;

      return !allyMustBeAbleToAttack || PlayerEncounterHelper.CanPartyAttack(possibleAlly, enemyParty.MobileParty);
    }

    private static bool IsAValidPossibleAlly(PartyBase party, MobileParty possibleAlly)
      => possibleAlly != MobileParty.MainParty && possibleAlly != PlayerEncounter.EncounteredParty.MobileParty && possibleAlly.MapEvent == null && possibleAlly.IsActive && (possibleAlly.BesiegedSettlement == null || (MobileParty.MainParty.MapEvent != null && MobileParty.MainParty.MapEvent.IsSiege && MobileParty.MainParty.MapEvent.MapEventSettlement == possibleAlly.BesiegedSettlement) || (MobileParty.MainParty.Army != null && MobileParty.MainParty.Army.LeaderParty.AttachedParties.Contains(possibleAlly))) && (possibleAlly.Army == null || !possibleAlly.Army.LeaderParty.AttachedParties.Contains(possibleAlly)) && (possibleAlly.CurrentSettlement == null || MobileParty.MainParty.BesiegedSettlement == possibleAlly.CurrentSettlement || (party.MobileParty != null && possibleAlly.CurrentSettlement == party.MobileParty.CurrentSettlement));
  }

}