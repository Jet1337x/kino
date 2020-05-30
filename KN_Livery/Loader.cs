using BepInEx;
using KN_Core;

namespace KN_Livery {
  [BepInPlugin("trbflxr.kn_3livery", "KN_Livery", "0.0.4")]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Livery(Core.CoreInstance));
    }
  }
}