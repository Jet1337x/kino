using BepInEx;
using KN_Core;

namespace KN_Lights {
  [BepInPlugin("trbflxr.kn_lights", "KN_Lights", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Lights(Core.CoreInstance));
    }
  }
}