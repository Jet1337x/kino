using BepInEx;
using KN_Core;

namespace KN_Cinematic {
  [BepInPlugin("trbflxr.kn_cinematic", "KN_Cinematic", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Cinematic(Core.CoreInstance));
    }
  }
}