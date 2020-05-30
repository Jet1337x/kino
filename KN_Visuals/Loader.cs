using BepInEx;
using KN_Core;

namespace KN_Visuals {
  [BepInPlugin("trbflxr.kn_4visuals", "KN_Visuals", "0.0.1")]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      Core.CoreInstance.AddMod(new Visuals(Core.CoreInstance));
    }
  }
}