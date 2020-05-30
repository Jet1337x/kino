using BepInEx;
using KN_Core;

namespace KN_Cinematic {
  [BepInPlugin("trbflxr.kn_1cinematic", "KN_Cinematic", "0.1.3")]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Cinematic(Core.CoreInstance));
    }
  }
}