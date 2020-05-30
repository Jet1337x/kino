using HarmonyLib;

namespace KN_Core {
  public static class Patcher {
    public static void Hook() {
      var harmony = new Harmony("trbflxr.kn_core.flippedpatch");
      harmony.PatchAll();
    }
  }

  [HarmonyPatch(typeof(RaceCar), "PlaceOnTrackIfFlipped")]
  internal class FlippedPatch {
    private static bool Prefix() {
      return false;
    }
  }
}