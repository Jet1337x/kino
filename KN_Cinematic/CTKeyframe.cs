#if false
using KN_Core;
using UnityEngine;

namespace KN_Cinematic {
  public class CTKeyframe {
    public float Time;
    public bool Active;

    public Vector3 Position;
    public Quaternion Rotation;

    public float Fov;
    public float Yaw;
    public float Pitch;
    public float Roll;

    public float HeadingX;
    public float HeadingY;
    public float HeadingZ;

    private readonly CTAnimation container_;

    public CTKeyframe(CTAnimation container, float time, Transform transform, float fov) {
      container_ = container;
      Time = time;
      Active = false;

      Position = transform.position;
      Rotation = transform.rotation;

      Fov = fov;
      Pitch = 0.0f;
      Yaw = 0.0f;
      Roll = 0.0f;

      HeadingX = 0.0f;
      HeadingY = 0.0f;
      HeadingZ = 0.0f;
    }

    public CTKeyframe(CTAnimation container, CTKeyframe kf0, CTKeyframe kf1, float t) {
      container_ = container;
      Active = false;

      Time = (1.0f - t) * kf0.Time + t * kf1.Time;

      Position = (1.0f - t) * kf0.Position + t * kf1.Position;

      Rotation = new Quaternion {
        x = (1.0f - t) * kf0.Rotation.x + t * kf1.Rotation.x,
        y = (1.0f - t) * kf0.Rotation.y + t * kf1.Rotation.y,
        z = (1.0f - t) * kf0.Rotation.z + t * kf1.Rotation.z,
        w = (1.0f - t) * kf0.Rotation.w + t * kf1.Rotation.w
      };

      Fov = (1.0f - t) * kf0.Fov + t * kf1.Fov;
      Pitch = (1.0f - t) * kf0.Pitch + t * kf1.Pitch;
      Yaw = (1.0f - t) * kf0.Yaw + t * kf1.Yaw;
      Roll = (1.0f - t) * kf0.Roll + t * kf1.Roll;

      HeadingX = (1.0f - t) * kf0.HeadingX + t * kf1.HeadingX;
      HeadingY = (1.0f - t) * kf0.HeadingY + t * kf1.HeadingY;
      HeadingZ = (1.0f - t) * kf0.HeadingZ + t * kf1.HeadingZ;
    }

    public CTKeyframe(CTKeyframe other) {
      Time = other.Time;
      Active = other.Active;
      Position = other.Position;
      Rotation = other.Rotation;
      Fov = other.Fov;
      Pitch = other.Pitch;
      Yaw = other.Yaw;
      Roll = other.Roll;
      HeadingX = other.HeadingX;
      HeadingY = other.HeadingY;
      HeadingZ = other.HeadingZ;
      container_ = other.container_;
    }

    public bool OnGUI(Gui gui, ref float x, ref float y, float width) {
      if (gui.ScrollViewButton(ref x, ref y, width, Gui.Height, $"{Time:F}",
        out bool delPressed, Active ? Skin.ButtonActive : Skin.Button, Skin.RedSkin)) {
        if (!delPressed) {
          Active = !Active;
          container_.ToggleKeyframe(this);
          container_.Container.Core.Timeline.CurrentTime = Time;
          float time = Time > 0.0f ? Time - 0.0001f : Time;
          container_.Container.ActiveCamera.UpdateAnimation(time);
        }
        else {
          container_.RemoveKeyframe(this);
          return false;
        }
      }
      return true;
    }
  }
}
#endif