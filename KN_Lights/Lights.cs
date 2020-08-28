using System.Collections.Generic;
using System.Reflection;
using KN_Core;
using UnityEngine;

namespace KN_Lights {
  public class Lights : BaseMod {
    public static Texture2D LightMask;

    private const string LightsConfigFile = "kn_lights.knl";
    private const string NwLightsConfigFile = "kn_nwlights.knl";
    private const string LightsConfigDefault = "kn_lights_default.knl";
#if KN_DEV_TOOLS
    private const string LightsDevConfigFile = "kn_lights_dev.knl";
#endif

    private LightsConfig lightsConfig_;
    private NwLightsConfig nwLightsConfig_;
    private LightsConfig lightsConfigDefault_;
#if KN_DEV_TOOLS
    private LightsConfig carLightsDev_;
#endif

    private CarLights activeLights_;
    private CarLights ownLights_;
    private readonly List<CarLights> carLights_;
    private readonly List<CarLights> carLightsToRemove_;

    private float clListScrollH_;
    private Vector2 clListScroll_;

    private bool hlTabActive_ = true;
    private bool wlTabActive_;

    private float carLightsDiscard_;

    private bool prevScene_;
    private bool autoAddLights_;

    private readonly WorldLights worldLights_;

    private readonly ColorPicker colorPicker_;
    private readonly CarPicker carPicker_;

    public Lights(Core core) : base(core, "LIGHTS", 1) {
      worldLights_ = new WorldLights(core);

      colorPicker_ = new ColorPicker();
      carPicker_ = new CarPicker(core);

      carLights_ = new List<CarLights>();
      carLightsToRemove_ = new List<CarLights>();
    }

    public override void ResetState() {
      ResetPickers();
    }

    public override void ResetPickers() {
      colorPicker_.Reset();
      carPicker_.Reset();
    }

    public override void OnStart() {
      var assembly = Assembly.GetExecutingAssembly();

      LightMask = Core.LoadTexture(assembly, "KN_Lights", "HeadLightMask.png");

      LoadDefaultLights(assembly);
#if KN_DEV_TOOLS
      carLightsDev_ = LightsConfigSerializer.Deserialize(LightsDevConfigFile, out var devLights) ? new LightsConfig(devLights) : new LightsConfig();
#endif

      carLightsDiscard_ = Core.ModConfig.Get<float>("cl_discard_distance");

      nwLightsConfig_ = LightsConfigSerializer.Deserialize(NwLightsConfigFile, out var nwLights) ? new NwLightsConfig(nwLights) : new NwLightsConfig();
      lightsConfig_ = LightsConfigSerializer.Deserialize(LightsConfigFile, out var lights) ? new LightsConfig(lights) : new LightsConfig();

      worldLights_.OnStart();
    }

    public override void OnStop() {
      if (!LightsConfigSerializer.Serialize(lightsConfig_, LightsConfigFile)) { }
      if (!LightsConfigSerializer.Serialize(nwLightsConfig_, NwLightsConfigFile)) { }

#if KN_DEV_TOOLS
      LightsConfigSerializer.Serialize(carLightsDev_, LightsDevConfigFile);
#endif

      worldLights_.OnStop();
    }

    protected override void OnCarLoaded() {
      AutoAddLights();
    }

    public override void Update(int id) {
      if (Id != id) {
        return;
      }

      if (!Core.IsGuiEnabled && activeLights_ != null && activeLights_.IsDebugObjectsEnabled) {
        activeLights_.IsDebugObjectsEnabled = false;
      }

      OptimizeLights();

      ToggleOwnLights();

      worldLights_.Update();

      if (id != Id) {
        return;
      }

      if (carPicker_.IsPicking && !TFCar.IsNull(carPicker_.PickedCar)) {
        if (carPicker_.PickedCar != Core.PlayerCar) {
          EnableLightsOn(carPicker_.PickedCar);
        }
        else {
          EnableLightsOn(Core.PlayerCar);
        }
        carPicker_.Reset();
      }

      if (colorPicker_.IsPicking) {
        if (activeLights_ != null && colorPicker_.PickedColor != activeLights_.HeadLightsColor) {
          activeLights_.HeadLightsColor = colorPicker_.PickedColor;
        }
      }
    }

    public override void LateUpdate(int id) {
      RemoveAllNullLights();
    }

    public override void OnGUI(int id, Gui gui, ref float x, ref float y) {
      if (id != Id) {
        return;
      }

      GuiSideBar(gui, ref x, ref y);

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      if (hlTabActive_) {
        GuiHeadLightsTab(gui, ref x, ref y);
      }
      else if (wlTabActive_) {
        worldLights_.OnGUI(gui, ref x, ref y);
      }
    }

    public override void GuiPickers(int id, Gui gui, ref float x, ref float y) {
      carPicker_.OnGUI(gui, ref x, ref y);

      if (colorPicker_.IsPicking) {
        if (carPicker_.IsPicking) {
          x += Gui.OffsetGuiX;
        }
        colorPicker_.OnGui(gui, ref x, ref y);
      }
    }

    private void GuiSideBar(Gui gui, ref float x, ref float y) {
      float yBegin = y;

      x += Gui.OffsetSmall;
      if (gui.ImageButton(ref x, ref y, hlTabActive_ ? Skin.IconHeadlightsActive : Skin.IconHeadlights)) {
        hlTabActive_ = true;
        wlTabActive_ = false;
        carPicker_.Reset();
        colorPicker_.Reset();
      }

      if (gui.ImageButton(ref x, ref y, wlTabActive_ ? Skin.IconSunActive : Skin.IconSun)) {
        hlTabActive_ = false;
        wlTabActive_ = true;
        carPicker_.Reset();
        colorPicker_.Reset();
      }

      x += Gui.IconSize;
      y = yBegin;
    }

    private void GuiHeadLightsTab(Gui gui, ref float x, ref float y) {
      float yBegin = y;

      const float width = Gui.Width * 2.0f;
      const float height = Gui.Height;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !TFCar.IsNull(Core.PlayerCar);

      if (gui.Button(ref x, ref y, width, height, "ENABLE OWN LIGHTS", Skin.Button)) {
        colorPicker_.Reset();
        EnableLightsOn(Core.PlayerCar);
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GUI.enabled = activeLights_ != null;

#if KN_DEV_TOOLS
      if (gui.Button(ref x, ref y, width, height, "DEV SAVE", Skin.Button)) {
        carLightsDev_.AddLights(activeLights_);
        Log.Write($"[KN_Lights]: Dev save / saved for '{activeLights_?.CarId ?? 0}'");
      }
#endif

      bool debugObjects = activeLights_?.IsDebugObjectsEnabled ?? false;
      if (gui.Button(ref x, ref y, width, height, "DEBUG OBJECTS", debugObjects ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsDebugObjectsEnabled = !activeLights_.IsDebugObjectsEnabled;
        }
      }

      bool hlIllumination = activeLights_?.IsLightsEnabledIl ?? false;
      if (gui.Button(ref x, ref y, width, height, "HEADLIGHTS ILLUMINATION", hlIllumination ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsLightsEnabledIl = !activeLights_.IsLightsEnabledIl;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref carLightsDiscard_, 50.0f, 500.0f, $"HIDE LIGHTS AFTER: {carLightsDiscard_:F1}")) {
        Core.ModConfig.Set("cl_discard_distance", carLightsDiscard_);
      }

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiHeadLights(gui, ref x, ref y, width, height);

      gui.Line(x, y, width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      GuiTailLights(gui, ref x, ref y, width, height);

      y = yBegin;
      x += width;

      x += Gui.OffsetGuiX;
      gui.Line(x, y, 1.0f, Core.GuiTabsHeight - Gui.OffsetY * 2.0f, Skin.SeparatorColor);
      x += Gui.OffsetGuiX;

      GUI.enabled = guiEnabled;

      GuiLightsList(gui, ref x, ref y);
    }

    private void GuiHeadLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;

      bool lh = activeLights_?.IsHeadLightLeftEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "LEFT HEADLIGHT", lh ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightLeftEnabled = !activeLights_.IsHeadLightLeftEnabled;
        }
      }
      y -= Gui.OffsetY + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rh = activeLights_?.IsHeadLightRightEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "RIGHT HEADLIGHT", rh ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsHeadLightRightEnabled = !activeLights_.IsHeadLightRightEnabled;
        }
      }
      x = xBegin;

      if (gui.Button(ref x, ref y, width, height, "COLOR", Skin.Button)) {
        if (activeLights_ != null) {
          colorPicker_.Toggle(activeLights_.HeadLightsColor, false);
        }
      }

      float brightness = activeLights_?.HeadLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 100.0f, 10000.0f, $"HEADLIGHTS BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightBrightness = brightness;
        }
      }

      float angle = activeLights_?.HeadLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 160.0f, $"HEADLIGHTS ANGLE: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.HeadLightAngle = angle;
        }
      }

      float hlPitch = activeLights_?.Pitch ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref hlPitch, -20.0f, 20.0f, $"HEADLIGHTS PITCH: {hlPitch:F}")) {
        if (activeLights_ != null) {
          activeLights_.Pitch = hlPitch;
        }
      }

      var offset = activeLights_?.HeadlightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, 3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.HeadlightOffset = offset;
        }
      }
    }

    private void GuiTailLights(Gui gui, ref float x, ref float y, float width, float height) {
      float xBegin = x;
      float widthLight = width / 2.0f - Gui.OffsetSmall;

      bool lt = activeLights_?.IsTailLightLeftEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "LEFT TAILLIGHT", lt ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightLeftEnabled = !activeLights_.IsTailLightLeftEnabled;
        }
      }
      y -= Gui.OffsetY + height;
      x += widthLight + Gui.OffsetSmall * 2.0f;

      bool rt = activeLights_?.IsTailLightRightEnabled ?? false;
      if (gui.Button(ref x, ref y, widthLight, height, "RIGHT TAILLIGHT", rt ? Skin.ButtonActive : Skin.Button)) {
        if (activeLights_ != null) {
          activeLights_.IsTailLightRightEnabled = !activeLights_.IsTailLightRightEnabled;
        }
      }
      x = xBegin;

      float brightness = activeLights_?.TailLightBrightness ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref brightness, 15.0f, 80.0f, $"TAILLIGHTS BRIGHTNESS: {brightness:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightBrightness = brightness;
        }
      }

      float angle = activeLights_?.TailLightAngle ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref angle, 50.0f, 170.0f, $"TAILLIGHTS ANGLE: {angle:F1}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightAngle = angle;
        }
      }

      float tlPitch = activeLights_?.PitchTail ?? 0.0f;
      if (gui.SliderH(ref x, ref y, width, ref tlPitch, -20.0f, 20.0f, $"TAILLIGHTS PITCH: {tlPitch:F1}")) {
        if (activeLights_ != null) {
          activeLights_.PitchTail = tlPitch;
        }
      }

      var offset = activeLights_?.TailLightOffset ?? Vector3.zero;
      if (gui.SliderH(ref x, ref y, width, ref offset.x, 0.0f, 3.0f, $"X: {offset.x:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.y, 0.0f, 3.0f, $"Y: {offset.y:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }

      if (gui.SliderH(ref x, ref y, width, ref offset.z, 0.0f, -3.0f, $"Z: {offset.z:F}")) {
        if (activeLights_ != null) {
          activeLights_.TailLightOffset = offset;
        }
      }
    }

    private void GuiLightsList(Gui gui, ref float x, ref float y) {
      const float listHeight = 320.0f;
      const float widthScale = 1.2f;
      const float buttonWidth = Gui.Width * widthScale;

      bool guiEnabled = GUI.enabled;
      GUI.enabled = !Core.IsInGarage;

      if (gui.Button(ref x, ref y, buttonWidth, Gui.Height, "ADD LIGHTS TO EVERYONE", autoAddLights_ ? Skin.ButtonActive : Skin.Button)) {
        autoAddLights_ = !autoAddLights_;

        if (autoAddLights_) {
          AddLightsToEveryone();
          EnableLightsOn(Core.PlayerCar);
        }
      }

      if (gui.Button(ref x, ref y, buttonWidth, Gui.Height, "ADD LIGHTS TO", Skin.Button)) {
        carPicker_.Toggle();
        colorPicker_.Reset();
      }
      GUI.enabled = guiEnabled;

      gui.BeginScrollV(ref x, ref y, buttonWidth, listHeight, clListScrollH_, ref clListScroll_, $"LIGHTS {carLights_.Count}");

      float sx = x;
      float sy = y;
      const float offset = Gui.ScrollBarWidth;
      bool scrollVisible = clListScrollH_ > listHeight;
      float width = scrollVisible ? Gui.WidthScroll * widthScale - offset : Gui.WidthScroll * widthScale + offset;
      foreach (var cl in carLights_) {
        if (cl != null) {
          bool active = activeLights_ == cl;
          if (gui.ScrollViewButton(ref sx, ref sy, width, Gui.Height, $"{cl.UserName}", out bool delPressed, active ? Skin.ButtonActive : Skin.Button, Skin.RedSkin)) {
            if (delPressed) {
              if (cl == activeLights_) {
                activeLights_ = null;
              }
              if (cl == ownLights_) {
                ownLights_ = null;
              }
              cl.Dispose();
              carLights_.Remove(cl);
              break;
            }
            activeLights_ = cl;
            if (colorPicker_.IsPicking) {
              colorPicker_.Pick(activeLights_.HeadLightsColor, false);
            }
          }
        }
      }

      clListScrollH_ = gui.EndScrollV(ref x, ref y, sx, sy);
      y += Gui.OffsetSmall;
    }

    private void EnableLightsOn(TFCar car) {
      bool player = car == Core.PlayerCar;

      var lights = player ? lightsConfig_.GetLights(car.Id) : nwLightsConfig_.GetLights(car.Id, car.Name);
      if (lights == null) {
        if (player) {
          lights = CreateLights(car, lightsConfig_);
          lightsConfig_.AddLights(lights);
        }
        else {
          lights = CreateLights(car, nwLightsConfig_);
          nwLightsConfig_.AddLights(lights);
        }
      }
      else {
        lights.Attach(car);
      }

      int index = carLights_.FindIndex(cl => cl.Car == car);
      if (index != -1) {
        carLights_[index] = lights;
      }
      else {
        carLights_.Add(lights);
      }
      activeLights_ = lights;
      if (player) {
        ownLights_ = lights;
      }
    }

    private CarLights CreateLights(TFCar car, LightsConfigBase config) {
      var l = lightsConfigDefault_.GetLights(car.Id);
      if (l == null) {
        l = new CarLights {
          HeadLightsColor = Color.white,
          Pitch = 0.0f,
          PitchTail = 0.0f,
          HeadLightBrightness = 1500.0f,
          HeadLightAngle = 100.0f,
          TailLightBrightness = 80.0f,
          TailLightAngle = 170.0f,
          IsHeadLightLeftEnabled = true,
          IsHeadLightRightEnabled = true,
          HeadlightOffset = new Vector3(0.6f, 0.6f, 1.9f),
          IsTailLightLeftEnabled = true,
          IsTailLightRightEnabled = true,
          TailLightOffset = new Vector3(0.6f, 0.6f, -1.6f)
        };
        Log.Write($"[KN_Lights]: Lights for car '{car.Id}' not found. Creating default.");
      }

      var light = l.Copy();
      light.Attach(car);
      config.AddLights(light);
      Log.Write($"[KN_Lights]: Car lights attached to '{car.Id}'");

      return light;
    }

    private void ToggleOwnLights() {
      if (Controls.KeyDown("toggle_lights")) {
        if (ownLights_ == null) {
          colorPicker_.Reset();
          EnableLightsOn(Core.PlayerCar);
        }
        else {
          if (ownLights_ == activeLights_) {
            activeLights_ = null;
          }
          ownLights_.Dispose();
          carLights_.Remove(ownLights_);
          ownLights_ = null;
        }
      }
    }

    private void OptimizeLights() {
      var cam = Core.ActiveCamera;
      if (cam != null) {
        foreach (var cl in carLights_) {
          if (!TFCar.IsNull(cl.Car)) {
            if (Vector3.Distance(cam.transform.position, cl.Car.Transform.position) > carLightsDiscard_) {
              if (cl.IsLightsEnabled) {
                cl.IsLightsEnabled = false;
              }
            }
            else {
              if (!cl.IsLightsEnabled) {
                cl.IsLightsEnabled = true;
              }
            }
          }
        }
      }
    }

    private void AutoAddLights() {
      bool sceneChanged = prevScene_ && !Core.IsInGarage || !prevScene_ && Core.IsInGarage;
      prevScene_ = Core.IsInGarage;

      if ((sceneChanged || TFCar.IsNull(Core.PlayerCar)) && carLights_.Count > 0) {
        autoAddLights_ = false;
        RemoveAllNullLights();
      }

      if (autoAddLights_) {
        if (!carLights_.Contains(ownLights_)) {
          EnableLightsOn(Core.PlayerCar);
        }
        AddLightsToEveryone();
      }
    }

    private void AddLightsToEveryone() {
      carPicker_.IsPicking = true;
      foreach (var car in carPicker_.Cars) {
        bool found = false;
        foreach (var cl in carLights_) {
          if (cl.Car == car) {
            found = true;
          }
        }
        if (!found) {
          EnableLightsOn(car);
        }
      }
      carPicker_.IsPicking = false;
    }

    private void RemoveAllNullLights() {
      foreach (var cl in carLights_) {
        if (TFCar.IsNull(cl.Car)) {
          carLightsToRemove_.Add(cl);
          continue;
        }
        cl.LateUpdate();
      }

      if (carLightsToRemove_.Count > 0) {
        foreach (var cl in carLightsToRemove_) {
          if (activeLights_ == cl) {
            activeLights_ = null;
          }
          if (ownLights_ == cl) {
            ownLights_ = null;
          }
          carLights_.Remove(cl);
        }
        carLightsToRemove_.Clear();
      }
    }

    private void LoadDefaultLights(Assembly assembly) {
      using (var stream = assembly.GetManifestResourceStream("KN_Lights.Resources." + LightsConfigDefault)) {
        if (LightsConfigSerializer.Deserialize(stream, out var lights)) {
          lightsConfigDefault_ = new LightsConfig(lights);
#if false
          foreach (var l in lightsConfigDefault_.Lights) { }
          LightsConfigSerializer.Serialize(lightsConfigDefault_, "dump.knl");
#endif
        }
      }
    }
  }
}