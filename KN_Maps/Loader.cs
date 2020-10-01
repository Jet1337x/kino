using BepInEx;
using KN_Core;

namespace KN_Maps {
  [BepInPlugin("trbflxr.kn_maps", "KN_Maps", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 124;
    private const int Patch = 1;
    private const int ClientVersion = 271;

    public Loader() {
      Core.CoreInstance.AddMod(new Maps(Core.CoreInstance, Version, Patch, ClientVersion));
      Patcher.Hook();
    }
  }
}