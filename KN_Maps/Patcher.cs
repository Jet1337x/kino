using HarmonyLib;

namespace KN_Maps {
  public static class Patcher {
    public static bool Valid { get; set; }

    public static void Hook() {
      Valid = true;
      var harmony = new Harmony("trbflxr.kn_maps.patch");
      harmony.PatchAll();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ModelValidator), "isValid", MethodType.Getter)]
    private static bool ValidatorPatch(ref bool __result) {
      __result = Valid;
      return false;
    }
  }
}