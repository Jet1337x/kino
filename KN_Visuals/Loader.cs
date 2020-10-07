using BepInEx;
using KN_Core;

namespace KN_Visuals {
  [BepInPlugin("trbflxr.kn_visuals", "KN_Visuals", StringVersion)]
  public class Loader : BaseUnityPlugin {
    private const int Version = 128;
    private const int Patch = 0;
    private const int ClientVersion = 272;
    private const string StringVersion = "1.2.8";

    public Loader() {
      Core.CoreInstance.AddMod(new Visuals(Core.CoreInstance, Version, Patch, ClientVersion));
    }
  }
}