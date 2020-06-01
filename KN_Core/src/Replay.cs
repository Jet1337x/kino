using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyncMultiplayer;
using UnityEngine;
using Object = UnityEngine.Object;

namespace KN_Core {
  public class Replay {
    public GhostRecorder Recorder { get; }
    public GhostPlayer Player { get; }

    public bool IsRecording { get; set; }
    public bool IsPlaying { get; set; }

    private Vector2 carsListScroll_;
    private float carsListScrollH_;

    private bool allowPick_;
    private readonly List<TFCar> cars_;
    private readonly List<GhostData> ghostData_;

    private bool pickingFile_;

    private readonly Core core_;

    public Replay(Core container) {
      core_ = container;

      Player = new GhostPlayer();
      Recorder = new GhostRecorder();

      cars_ = new List<TFCar>();
      ghostData_ = new List<GhostData>();
    }

    public void ResetState() {
      pickingFile_ = false;
      allowPick_ = false;
    }

    public void FixedUpdate() {
      if (IsRecording) {
        Recorder.FixedUpdate();
      }
    }

    public void Update() {
      if (core_.PickedCar != null) {
        if (allowPick_) {
          if (!core_.PickedCar.IsGhost) {
            if (!cars_.Contains(core_.PickedCar)) {
              cars_.Add(core_.PickedCar);
            }
          }
          core_.PickedCar = null;
          allowPick_ = false;
        }
      }

      if (pickingFile_) {
        if (core_.FilePicker.PickedFile != null) {
          string file = core_.FilePicker.PickedFile;
          core_.FilePicker.PickedFile = null;
          core_.FilePicker.IsPicking = false;
          pickingFile_ = false;

          LoadPickedReplay(file);
        }
      }
    }

    public void TimeUpdate(float time, bool drag) {
      if (!IsPlaying && !drag) {
        return;
      }

      if (time >= Player.length) {
        if (Player.isPlaying) {
          Player.Stop();
        }
        return;
      }
      Player.CurTimeOverride(time);
    }

    public void PlayPause(bool play) {
      if (Player.players.Count == 0) {
        return;
      }

      IsPlaying = play;
      if (IsPlaying) {
        Player.Play();
      }
      else {
        Player.Pause();
      }
    }

    public void Stop(float time) {
      IsPlaying = false;
      Player.Pause();
      Player.CurTimeOverride(time);
    }

    private void Reset() {
      Recorder.Stop();
      Recorder.ClearRecorderSource();

      Player.Stop();
      Player.ClearPlayers();

      IsPlaying = false;
      core_.Timeline.IsPlaying = false;
      core_.Timeline.MaxTime = 1.0f;
      core_.Timeline.HighBound = 1.0f;
      core_.Timeline.CurrentTime = 0.0f;
    }

    private void StartRecord() {
      if (cars_.Count == 0) {
        return;
      }

      IsRecording = true;

      Log.Write("[KN_Replay]: Record prep begin");

      var gp = Object.FindObjectOfType<GamePrefs>();
      if (gp != null) {
        Log.Write("[KN_Replay]: GP ok");
        Reset();
        Log.Write("[KN_Replay]: Reset ok");
        Log.Write($"[KN_Replay]: Cars size: {cars_.Count}");

        foreach (var car in cars_) {
          if (car == null || car.Base == null) {
            Log.Write($"[KN_Replay]: Null car");
            continue;
          }
          int id = car.Id;
          var vis = car != core_.PlayerCar &&
                    car.Base != null &&
                    car.Base.networkPlayer != null
            ? car.Base.networkPlayer.PlayerProperties.VisualSettings.Copy()
            : gp.carSettings.GetVisualForCar(id).Copy();
          Log.Write($"[KN_Replay]: Car '{car.Name}' | id: {id} visuals: {vis != null} | base: {car.Base != null}");

          if (vis != null) {
            var cvTemp = gp.carSettings.GetVisualForCar(id).Copy();
            gp.carSettings.SetVisualForCar(id, vis.Copy());
            Recorder.AddRecordSource(car.Base);
            gp.carSettings.SetVisualForCar(id, cvTemp.Copy());
          }
          Log.Write($"[KN_Replay]: Add source '{car.Name}'");
        }

        Recorder.RaceStart();
        Recorder.Record();
      }

      Log.Write("[KN_Replay]: Record prep end");
    }

    public void StopRecord() {
      if (cars_.Count == 0) {
        return;
      }

      IsRecording = false;

      Recorder.Stop();

      string name = DateTime.Now.ToString("dd.MM.yyyy_HH.mm.ss") + ".knrp";
      Log.Write($"[KN_Replay]: Saving to '{Config.ReplaysDir + name}'");
      SaveReplay(Config.ReplaysDir + name);

      cars_.Clear();
      ghostData_.Clear();
    }

    public void OnGui(Gui gui, ref float x, ref float y) {
      float yBegin = y;
      float xBegin = x;

      if (gui.Button(ref x, ref y, "RESET", Skin.Button)) {
        Reset();
      }

      string replayMode = "RECORD";
      if (IsRecording) {
        replayMode = "STOP";
      }

      if (gui.Button(ref x, ref y, replayMode, IsRecording ? Skin.ButtonActive : Skin.Button)) {
        if (cars_.Count == 0) {
          Log.Write("[KN_Replay]: Select cars to record");
          return;
        }

        IsRecording = !IsRecording;
        if (IsRecording) {
          StartRecord();
        }
        else {
          StopRecord();
        }
      }

      if (gui.Button(ref x, ref y, "LOAD", Skin.Button)) {
        pickingFile_ = !pickingFile_;
        if (pickingFile_) {
          core_.FilePicker.PickIn(Config.ReplaysDir);
        }
      }

      float yAfterReplay = y;

      y = yBegin;
      x += Gui.Width + 2.0f;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, core_.GuiTabsHeight - (Gui.OffsetY * 3.0f + Gui.Height + Gui.OffsetY), Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      GuiCarList(gui, ref x, ref y);

      y = yAfterReplay;
      x = xBegin;
    }

    private void GuiCarList(Gui gui, ref float x, ref float y) {
      bool enabled = GUI.enabled;

      GUI.enabled = !IsRecording && !core_.IsInGarage;
      if (gui.Button(ref x, ref y, "PICK CAR", Skin.Button)) {
        allowPick_ = !allowPick_;
        core_.ShowCars = allowPick_;
      }

      const float listHeight = 300.0f;
      gui.BeginScrollV(ref x, ref y, listHeight, carsListScrollH_, ref carsListScroll_, "PICKED CARS");

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth / 2.0f;
      bool scrollVisible = carsListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll - offset : Gui.WidthScroll + offset;

      foreach (var c in cars_) {
        if (!GuiCarEntry(c, gui, ref sx, ref sy, width)) {
          break;
        }
      }

      carsListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);
      y += Gui.OffsetSmall;

      GUI.enabled = enabled;
    }

    private bool GuiCarEntry(TFCar car, Gui gui, ref float x, ref float y, float width) {
      if (gui.ScrollViewButton(ref x, ref y, width, Gui.Height, $"{car.Name}", out bool delPressed, Skin.Button, Skin.RedSkin)) {
        if (delPressed) {
          cars_.Remove(car);
          return false;
        }
        return true;
      }
      return true;
    }

    private void FitToTimeline() {
      core_.Timeline.MaxTime = Player.length;
      core_.Timeline.HighBound = Player.length;
    }

    private void LoadPickedReplay(string file) {
      Reset();

      Log.Write($"[KN_Replay]: Loading replay '{file}'");

      LoadReplay(file);
      foreach (var gd in ghostData_) {
        Player.AddPlayer(new GhostPlayer.StartArgs {
          data = gd,
          needCollisions = false
        });
      }

      Log.Write("[KN_Replay]: Replay info:");
      Log.Write($"\tLength: {Player.length:F}, Players: {Player.players.Count:D}");

      FitToTimeline();
    }

    private void LoadReplay(string path) {
      ghostData_.Clear();

      if (!File.Exists(path)) {
        Log.Write($"[KN_Replay]: Replay not exists '{path}'");
        return;
      }

      try {
        using (var memoryStream = new MemoryStream(File.ReadAllBytes(path))) {
          using (var reader = new BinaryReader(memoryStream)) {
            int size = reader.ReadInt32();
            Log.Write($"[KN_Replay]: Reading '{size}' cars");
            for (int i = 0; i < size; i++) {
              var gd = new GhostData();

              gd.carId = reader.ReadInt32();
              gd.profileId = reader.ReadInt32();

              CarVisualManager.VisualSettings visual = null;
              int visualsSize = reader.ReadInt32();
              if (visualsSize > 0) {
                visual = CustomTypes.DeserializeCarVS(reader.ReadBytes(visualsSize));
                visual = visual?.MutateToLocalParts();
                Log.Write($"[KN_Replay]: Car visuals '{visual != null}'");
              }
              gd.visual = visual;

              ProfilesInfo.Profile profile = null;
              int profileSize = reader.ReadInt32();
              if (profileSize > 0) {
                profile = CustomTypes.DeserializeCarProfile(reader.ReadBytes(profileSize));
                Log.Write($"[KN_Replay]: Car profile '{profile != null}'");
              }
              gd.profile = profile;

              int streamsSize = reader.ReadInt32();
              Log.Write($"[KN_Replay]: Car streams size '{streamsSize}'");
              DeserializeStreams(reader, reader.BaseStream.Position, streamsSize, gd);

              ghostData_.Add(gd);
            }
          }
        }
      }
      catch (Exception ex) {
        Log.Write($"[KN_Replay]: Error reading replay '{ex.Message}'");
      }
    }

    private void DeserializeStreams(BinaryReader reader, long start, int size, GhostData target) {
      var replayStreamTypeList = new List<GhostData.ReplayStreamType> {
        GhostData.ReplayStreamType.Position,
        GhostData.ReplayStreamType.Rotation,
        GhostData.ReplayStreamType.Steer,
        GhostData.ReplayStreamType.RPM,
        GhostData.ReplayStreamType.Velocity,
        GhostData.ReplayStreamType.AngVelocity
      };

      int i = 0;
      while (reader.BaseStream.Position < start + size) {
        ReadSingleStream(reader, out var streamType, out var streamData);
        var streamOfType = target.GetStreamOfType(streamType);
        if (streamOfType != null) {
          replayStreamTypeList.Remove(streamType);
          using (var ms = new MemoryStream(streamData)) {
            using (var r = new BinaryReader(ms)) {
              streamOfType.Deserialize(r);
              Log.Write($"[KN_Replay]: Stream '{streamOfType}' ({i}) was deserialized");
            }
          }
        }
        ++i;
      }
      if (replayStreamTypeList.Count > 0) {
        string str = replayStreamTypeList.Aggregate("", (current, rs) => current + rs + ", ");
        throw new Exception($"[KN_Replay]: Replay data is incomplete, missing streams: '{str}'");
      }
    }

    private void ReadSingleStream(BinaryReader reader, out GhostData.ReplayStreamType streamType, out byte[] streamData) {
      byte type = reader.ReadByte();
      int count = reader.ReadInt32();
      if (!Enum.IsDefined(typeof(GhostData.ReplayStreamType), type)) {
        throw new Exception($"[KN_Replay]: Wrong stream type '{type}'");
      }
      streamType = (GhostData.ReplayStreamType) type;
      streamData = reader.BaseStream.ForcedReadBytes(count);
      Log.Write($"[KN_Replay]: Stream '{streamType}' ({count} bytes) was read");
    }

    private void SaveReplay(string path) {
      using (var memoryStream = new MemoryStream()) {
        using (var writer = new BinaryWriter(memoryStream)) {
          writer.Write(cars_.Count);
          for (int i = 0; i < cars_.Count; i++) {
            Log.Write($"[KN_Replay]: Saving '{cars_[i].Name}'");
            var gd = Recorder.GetRecordedData(i);
            const int none = 0;

            writer.Write(gd.carId);
            writer.Write(gd.profileId);
            if (gd.visual != null) {
              var buffer = CustomTypes.SerializeCarVS(gd.visual.Copy().MutateToNetworkParts());
              writer.Write(buffer.Length);
              writer.Write(buffer);
              Log.Write($"[KN_Replay]: Car '{cars_[i].Name}' has visuals");
            }
            else {
              writer.Write(none);
            }

            if (gd.profile != null) {
              var buffer = CustomTypes.SerializeCarProfile(gd.profile);
              writer.Write(buffer.Length);
              writer.Write(buffer);
              Log.Write($"[KN_Replay]: Car '{cars_[i].Name}' has profile");
            }
            else {
              writer.Write(none);
            }

            byte[] array;
            using (var ms = new MemoryStream()) {
              using (var w = new BinaryWriter(ms)) {
                SerializeInternal(gd, w);
                array = ms.ToArray();
                Log.Write($"[KN_Replay]: Car '{cars_[i].Name}' streams size: {array.Length}");
              }
            }
            writer.Write(array.Length);
            writer.Write(array);
          }
          using (var fileStream = File.Open(path, FileMode.Create)) {
            memoryStream.WriteTo(fileStream);
          }
        }
      }
    }

    private void SerializeInternal(GhostData gd, BinaryWriter writer) {
      int i = 0;
      foreach (var type in (GhostData.ReplayStreamType[]) Enum.GetValues(typeof(GhostData.ReplayStreamType))) {
        var streamOfType = gd.GetStreamOfType(type);
        if (streamOfType != null && !streamOfType.IsEmpty()) {
          byte[] array;
          using (var ms = new MemoryStream()) {
            using (var w = new BinaryWriter(ms)) {
              streamOfType.Serialize(w);
              array = ms.ToArray();
              Log.Write($"[KN_Replay]: Stream '{streamOfType}' ({i}, {array.Length} bytes) serialized");
            }
          }
          writer.Write((byte) type);
          writer.Write(array.Length);
          writer.Write(array);
        }
        ++i;
      }
    }
  }
}