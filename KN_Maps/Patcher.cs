using BepInEx.Harmony;
using HarmonyLib;

namespace KN_Maps {
  public static class Patcher {
    public static bool Valid { get; set; }

    public static void Hook() {
      Valid = true;
      HarmonyWrapper.PatchAll();
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(ModelValidator), "isValid", MethodType.Getter)]
    private static bool ValidatorPatch(ref bool __result) {
      __result = Valid;
      return false;
    }
  }
}