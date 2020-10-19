using BepInEx;
using KN_Core;

namespace KN_Maps {
  [BepInPlugin("trbflxr.kn_maps", "KN_Maps", StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 200;
    private const int Patch = 1;
    private const int ClientVersion = 273;
    private const string StringVersion = "2.0.0";

    public Loader() {
      Core.CoreInstance.AddMod(new Maps(Core.CoreInstance, Version, Patch, ClientVersion));
      Patcher.Hook();
    }
  }
}