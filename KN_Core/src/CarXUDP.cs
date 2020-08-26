using System.Reflection;
using SyncMultiplayer;

namespace KN_Core {
  public class CarXUDP {
    public SmartfoxRoomClient Room { get; private set; }
    public SmartfoxClient Client{ get; private set; }

    public bool ReloadClient;

    public delegate void PacketCallback(SmartfoxDataPackage data);

    public event PacketCallback ProcessPacket;

    public void Update() {
      if (Room == null || ReloadClient) {
        Room = NetworkController.InstanceGame.Client;
      }

      if (Client == null) {
        TrySetupClient();
      }
    }

    private void TrySetupClient() {
      if (Room != null) {
        Client = typeof(SmartfoxRoomClient).GetField("m_client", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Room) as SmartfoxClient;

        Client?.Sfs.InitUDP();

        NetworkController.InstanceGame.packetHandler.Subscribe(PacketId.Subroom, MainPacketHandler);
        typeof(SmartfoxRoomClient).GetField("m_client", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Room, Client);

        ReloadClient = false;
      }
    }

    private void MainPacketHandler(NetworkPlayer sender, SmartfoxDataPackage data) {
      ProcessPacket?.Invoke(data);
    }
  }
}