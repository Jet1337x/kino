using BepInEx;
using KN_Loader;

namespace KN_Core {
  [BepInPlugin("1trbflxr.kn_core", "KN_Core", ModLoader.StringVersion)]
  public class Loader : BaseUnityPlugin {
    public Loader() {
      ModLoader.SetCore(new Core(ModLoader.Instance));
    }
  }
}