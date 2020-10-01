using BepInEx;
using KN_Core;

namespace KN_Lights {
  [BepInPlugin("trbflxr.kn_lights", "KN_Lights", KnConfig.StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 124;
    private const int Patch = 0;
    private const int ClientVersion = 271;

    public Loader() {
      Core.CoreInstance.AddMod(new Lights(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}