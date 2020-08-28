using System;
using System.Reflection;
using KN_Core.Submodule;
using SyncMultiplayer;

namespace KN_Core {
  public class Udp {
    public const int TypeSuspension = 0;

    public SmartfoxRoomClient Room { get; private set; }
    public SmartfoxClient Client { get; private set; }

    public NetGameSubroomsSystem SubRoom { get; private set; }

    public bool ReloadClient;
    public bool ReloadSubRoom;

    public delegate void PacketCallback(SmartfoxDataPackage data);

    public event PacketCallback ProcessPacket;

    private Guid guid_;
    private string guidString_;
    private MethodInfo sendSubRoomId_;

    private Settings settings_;

    public Udp(Settings settings) {
      settings_ = settings;

      guidString_ = "8de61547-4c31-49cd-b8c6-7e12d6ff23bc";
      guid_ = new Guid(guidString_);
    }

    public void Update() {
      if (Room == null || ReloadClient) {
        Room = NetworkController.InstanceGame.Client;
        TrySetupSubRoom();
        TrySetupClient();
      }

      if (SubRoom == null || ReloadSubRoom) {
        TrySetupSubRoom();
      }

      if (Client == null || ReloadClient) {
        TrySetupClient();
      }

      if (Client != null && !Client.Sfs.IsConnected) {
        Client.Sfs.InitUDP();
      }

      if (Client != null && Client.State != ClientState.Joined) {
        Client.Sfs.InitUDP();
      }
    }

    public void SendChangeRoomId(NetworkPlayer receiver, bool enabled) {
      if (SubRoom != null) {
        string guid = enabled ? "" : guidString_;
        sendSubRoomId_?.Invoke(SubRoom, new object[] {receiver, guid});
      }
    }

    private void TrySetupSubRoom() {
      SubRoom = NetworkController.InstanceGame.systems.Get<NetGameSubroomsSystem>();
      sendSubRoomId_ = typeof(NetGameSubroomsSystem).GetMethod("SEND_ChangeRoomID", BindingFlags.NonPublic | BindingFlags.Instance);

      ReloadSubRoom = false;
    }

    private void TrySetupClient() {
      if (Room != null) {
        Client = typeof(SmartfoxRoomClient).GetField("m_client", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(Room) as SmartfoxClient;

        if (!Client?.Sfs.IsConnected ?? false) {
          Client?.Sfs.InitUDP();
        }

        NetworkController.InstanceGame.packetHandler.Subscribe(PacketId.Subroom, MainPacketHandler);
        typeof(SmartfoxRoomClient).GetField("m_client", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(Room, Client);

        ReloadClient = false;
      }
    }

    private void MainPacketHandler(NetworkPlayer sender, SmartfoxDataPackage data) {
      if (settings_.ReceiveUdp) {
        ProcessPacket?.Invoke(data);
      }
    }
  }
}