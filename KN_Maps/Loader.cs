using BepInEx;
using KN_Core;

namespace KN_Maps {
  [BepInPlugin("trbflxr.kn_maps", "KN_Maps", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 119;
    private const int ClientVersion = 270;

    public Loader() {
      Core.CoreInstance.AddMod(new Maps(Core.CoreInstance, Version, ClientVersion));
      Patcher.Hook();
    }
  }
}