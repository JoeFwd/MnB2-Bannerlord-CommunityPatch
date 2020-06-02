using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using HarmonyLib;
using Helpers;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using static System.Reflection.BindingFlags;
using static CommunityPatch.HarmonyHelpers;

namespace CommunityPatch.Patches.Perks.Endurance.Riding {

  public sealed class VigorousPatch : PerkPatchBase<VigorousPatch> {

    public override bool Applied { get; protected set; }

    private static readonly MethodInfo SetMountAgentBeforeBuildMethodInfo = typeof(Agent).GetMethod("SetMountAgentBeforeBuild", NonPublic | Instance | DeclaredOnly);
    
    private static readonly MethodInfo PatchMethodInfo = typeof(VigorousPatch).GetMethod(nameof(SetMountAgentBeforeBuildPrefix), NonPublic | Static | DeclaredOnly);
    
    public override IEnumerable<MethodBase> GetMethodsChecked() {
      yield return SetMountAgentBeforeBuildMethodInfo;
    }

    public static readonly byte[][] TargetHashes = {
      new byte[] {
        // e1.4.1.229965
        0xD3, 0x16, 0x7D, 0x95, 0x6B, 0x88, 0xC0, 0x26,
        0x05, 0xC8, 0xCA, 0x75, 0x70, 0xF0, 0x60, 0x37,
        0x97, 0x15, 0x76, 0x0D, 0xF8, 0xA9, 0xE8, 0x39,
        0x3F, 0x39, 0x52, 0x6E, 0xD3, 0xF1, 0x8D, 0x48
      }
    };

    public VigorousPatch() : base("tE0u8uoS") {
    }

    public override bool? IsApplicable(Game game) {
      if (SetMountAgentBeforeBuildMethodInfo == null)
        return false;

      if (AlreadyPatchedByOthers(Harmony.GetPatchInfo(SetMountAgentBeforeBuildMethodInfo)))
        return false;

      if (!SetMountAgentBeforeBuildMethodInfo.MakeCilSignatureSha256().MatchesAnySha256(TargetHashes))
        return false;

      if (PatchMethodInfo == null) {
        return false;
      }

      return base.IsApplicable(game);
    }

    public override void Apply(Game game) {
      if (Applied) return;
      CommunityPatchSubModule.Harmony.Patch(SetMountAgentBeforeBuildMethodInfo,new HarmonyMethod(PatchMethodInfo));

      Applied = true;
    }

    private static bool HeroHasPerk(BasicCharacterObject character, PerkObject perk)
      => (character as CharacterObject)?.GetPerkValue(perk) ?? false;
    
    private static void SetMountAgentBeforeBuildPrefix(ref Agent mount, BasicCharacterObject ____character) {
      var agentCharacter = ____character;
      if (!(agentCharacter != null && mount != null && HeroHasPerk(agentCharacter, ActivePatch.Perk))) {
        return;
      }

      var bonusFactor = 0.06f;
      mount.HealthLimit += mount.HealthLimit * bonusFactor;
      mount.Health += mount.Health * bonusFactor;
      
      var activatedPerkMessage = $"{agentCharacter.Name} has activated {(agentCharacter.IsFemale ? "her" : "his")} {ActivePatch.Perk.Name} perk.";
      InformationManager.DisplayMessage(new InformationMessage(activatedPerkMessage));
    }

  }

}