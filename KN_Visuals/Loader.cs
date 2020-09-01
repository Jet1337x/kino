using BepInEx;
using KN_Core;

namespace KN_Visuals {
  [BepInPlugin("trbflxr.kn_1visuals", "KN_Visuals", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Visuals(Core.CoreInstance));
    }
  }
}