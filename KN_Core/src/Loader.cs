using BepInEx;
using KN_Loader;

namespace KN_Core {
  [BepInPlugin("trbflxr.1kn_core", "KN_Core", ModLoader.StringVersion)]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      ModLoader.SetCore(new Core(ModLoader.Instance));
    }
  }
}