using BepInEx;
using KN_Core;

namespace KN_Maps {
  [BepInPlugin("trbflxr.kn_1maps", "KN_Maps", "1.2.0")]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Maps(Core.CoreInstance));
    }
  }
}