using FMODUnity;
using KN_Core;
using UnityEngine;

namespace KN_Cinematic {
  public class CTCamera {
    public string Tag { get; }

    private bool enabled_;
    public bool Enabled {
      get => enabled_;
      set {
        enabled_ = value;
        if (GameObject != null) {
          var cam = GameObject.GetComponent<Camera>();
          if (cam != null) {
            cam.enabled = value;
          }

          var lis = GameObject.GetComponent<StudioListener>();
          if (lis != null) {
            lis.enabled = value;
          }
        }

        if (CameraSwitch.instance != null) {
          if (HookTo && value) {
            CameraSwitch.instance.DeattachCam();
          }
          else {
            CameraSwitch.instance.AttachCam();
          }
        }
      }
    }

    public GameObject GameObject { get; }

    private float fov_;
    public float Fov {
      get => fov_;
      set {
        fov_ = value;
        GameObject.GetComponent<Camera>().fieldOfView = fov_;
      }
    }

    private readonly Cinematic container_;

    public CTAnimation Animation { get; }

    private bool pickTarget_;
    private bool pickParent_;
    public bool LookAt { get; private set; }
    public bool HookTo { get; private set; }

    public TFCar Target { get; private set; }
    public TFCar Parent { get; private set; }

    private GameObject dummy_;
    private GameObject heading_;
    public Vector3 HeadingOffset;

    private CARXRearCamera[] cxFreecam_;

    public CTCamera(Cinematic container, GameObject camera, string tag) {
      GameObject = new GameObject();
      container_ = container;

      Tag = tag;
      enabled_ = false;

      GameObject.AddComponent<Camera>();
      var newCam = GameObject.GetComponent<Camera>();
      newCam.CopyFrom(camera.GetComponent<Camera>());
      newCam.enabled = enabled_;
      GameObject.AddComponent<StudioListener>();
      GameObject.GetComponent<StudioListener>().enabled = enabled_;

      var transform = camera.transform;
      GameObject.transform.position = transform.position;
      GameObject.transform.rotation = transform.rotation;

      fov_ = newCam.fieldOfView;

      Animation = new CTAnimation(container);

      dummy_ = new GameObject("Dummy");
      heading_ = new GameObject("Heading_Dummy");
      HeadingOffset = Vector3.up; // 0, 1, 0
    }

    public void ResetState() {
      if (HookTo && CameraSwitch.instance != null) {
        CameraSwitch.instance.AttachCam();
      }
    }

    public void ResetPickers() {
      pickTarget_ = false;
      pickParent_ = false;
    }

    public bool OnGUI(Gui gui, ref float x, ref float y, float width) {
      if (gui.ScrollViewButton(ref x, ref y, width, Gui.Height, $"{Tag}",
        out bool delPressed, Enabled ? Skin.ButtonActive : Skin.Button, Skin.RedSkin)) {
        if (!delPressed) {
          Enabled = !Enabled;
          container_.DisableAllCamerasBut(Tag);
          container_.SetActiveCamera(Enabled ? this : null);
        }
        else {
          container_.RemoveCamera(this);
          return false;
        }
      }
      return true;
    }

    public void GuiTransformMode(Gui gui, ref float x, ref float y) {
      string target = string.Empty;
      if (Target != null) {
        target = Target == container_.Core.PlayerCar ? ": OWN CAR" : $": {Target.Name}";
      }

      string parent = string.Empty;
      if (Parent != null) {
        parent = Parent == container_.Core.PlayerCar ? ": OWN CAR" : $": {Parent.Name}";
      }

      if (gui.Button(ref x, ref y, $"TARGET{target}", Skin.Button)) {
        pickTarget_ = !pickTarget_;
        container_.Core.ShowCars = pickTarget_;
        if (pickTarget_) {
          pickParent_ = false;
        }
      }

      if (gui.Button(ref x, ref y, $"PARENT{parent}", Skin.Button)) {
        pickParent_ = !pickParent_;
        container_.Core.ShowCars = pickParent_;
        if (pickParent_) {
          pickTarget_ = false;
        }
      }

      gui.Line(x, y, Gui.Width, 1.0f, Skin.SeparatorColor);
      y += Gui.OffsetY;

      bool old = GUI.enabled;
      GUI.enabled = Target != null && old;

      if (gui.Button(ref x, ref y, "LOOK AT", LookAt ? Skin.ButtonActive : Skin.Button)) {
        LookAt = !LookAt;
        ToggleLookAt();
      }

      GUI.enabled = Parent != null && old;

      if (gui.Button(ref x, ref y, "HOOK TO", HookTo ? Skin.ButtonActive : Skin.Button)) {
        HookTo = !HookTo;
        ToggleHook();
      }

      GUI.enabled = old;
    }

    public void GuiTransformAdjust(Gui gui, ref float x, ref float y) {
      const float offset = 1.0f;

      bool old = GUI.enabled;
      GUI.enabled = old && Target != null && LookAt;
      if (gui.SliderH(ref x, ref y, ref HeadingOffset.x, HeadingOffset.x - offset, HeadingOffset.x + offset, $"HEADING X: {HeadingOffset.x:F}")) {
        heading_.transform.localPosition = HeadingOffset;
      }
      if (gui.SliderH(ref x, ref y, ref HeadingOffset.y, HeadingOffset.y - offset, HeadingOffset.y + offset, $"HEADING Y: {HeadingOffset.y:F}")) {
        heading_.transform.localPosition = HeadingOffset;
      }
      if (gui.SliderH(ref x, ref y, ref HeadingOffset.z, HeadingOffset.z - offset, HeadingOffset.z + offset, $"HEADING Z: {HeadingOffset.z:F}")) {
        heading_.transform.localPosition = HeadingOffset;
      }
      GUI.enabled = old;
    }

    public void Update() {
      CheckTargetParent();

      if (pickTarget_) {
        if (container_.Core.PickedCar != null) {
          if (Target != container_.Core.PickedCar) {
            LookAt = false;
          }
          Target = container_.Core.PickedCar;
          container_.Core.PickedCar = null;
          pickTarget_ = false;
        }
      }
      if (pickParent_) {
        if (container_.Core.PickedCar != null) {
          if (Parent != container_.Core.PickedCar) {
            HookTo = false;
          }
          Parent = container_.Core.PickedCar;
          container_.Core.PickedCar = null;
          pickParent_ = false;
        }
      }

      if (HookTo && cxFreecam_ != null) {
        foreach (var cam in cxFreecam_) {
          cam.transform.position = Parent.Transform.position;
        }
      }

      if (HookTo && dummy_ != null) {
        GameObject.transform.position = dummy_.transform.position;
        GameObject.transform.rotation = dummy_.transform.rotation;
      }

      if (LookAt && Target != null) {
        var g = HookTo ? dummy_ : GameObject;
        if (heading_ != null) {
          heading_.transform.localPosition = HeadingOffset;
          g.transform.LookAt(heading_.transform);
        }
      }
    }

    public void UpdateAnimation(float time) {
      Animation.Update(this, time, ref HeadingOffset, LookAt, HookTo);
    }

    public void MakeKeyframe(float time) {
      CheckTargetParent();
      Animation.Add(new CTKeyframe(Animation, time, HookTo ? dummy_.transform : GameObject.transform, Fov));
    }

    public void RemoveAnimation() {
      Animation.Reset();
    }

    public void Rotate(float pitch, float yaw, float roll) {
      CheckTargetParent();
      var g = HookTo ? dummy_ : GameObject;

      var rot = g.transform.rotation;
      rot *= Quaternion.AngleAxis(pitch, Vector3.right);
      rot = Quaternion.AngleAxis(yaw, Vector3.up) * rot;
      rot = Quaternion.AngleAxis(roll, Vector3.forward) * rot;
      g.transform.rotation = rot;
    }

    public void Move(float speed, float multiplier) {
      CheckTargetParent();
      var g = HookTo ? dummy_ : GameObject;
      g.transform.position += Movement.Move(g.transform, speed, multiplier);
    }

    private void ToggleHook() {
      if (Parent != null) {
        var transform = Parent.Transform;
        dummy_.transform.parent = HookTo ? transform : null;
        dummy_.transform.position = transform.position + new Vector3(0.0f, 1.5f, 1.0f);

        var rot = dummy_.transform.eulerAngles;
        rot.z = 0.0f;
        dummy_.transform.eulerAngles = rot;
      }

      if (CameraSwitch.instance != null) {
        if (HookTo) {
          cxFreecam_ = Object.FindObjectsOfType<CARXRearCamera>();
          CameraSwitch.instance.DeattachCam();
        }
        else {
          CameraSwitch.instance.AttachCam();
        }
      }
    }

    private void ToggleLookAt() {
      if (LookAt && Target != null) {
        heading_.transform.parent = Target.Transform;
        HeadingOffset = Vector3.zero;
      }
    }

    private void CheckTargetParent() {
      if (HookTo && (Parent == null || dummy_ == null)) {
        HookTo = false;
        dummy_ = new GameObject();

        dummy_.transform.position = GameObject.transform.position;
        dummy_.transform.rotation = GameObject.transform.rotation;

        var rot = dummy_.transform.eulerAngles;
        rot.z = 0.0f;
        dummy_.transform.eulerAngles = rot;

        CameraSwitch.instance.AttachCam();
      }
      if (LookAt && (Target == null || heading_ == null)) {
        LookAt = false;
        heading_ = new GameObject();
      }
    }
  }
}