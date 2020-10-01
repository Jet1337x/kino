using BepInEx;
using KN_Core;

namespace KN_Visuals {
  [BepInPlugin("trbflxr.kn_visuals", "KN_Visuals", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 124;
    private const int Patch = 1;
    private const int ClientVersion = 271;

    public Loader() {
      Core.CoreInstance.AddMod(new Visuals(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}