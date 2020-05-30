using UnityEngine;

namespace KN_Core {
  public static class Movement {
    public static Vector3 Move(Transform transform, float speed, float multiplier) {
      var position = new Vector3(0.0f, 0.0f, 0.0f);

      if (Controls.Key("fast")) {
        speed *= multiplier;
      }

      if (Controls.Key("slow")) {
        speed /= multiplier;
      }

      speed *= Time.deltaTime;

      if (Controls.Key("forward")) {
        position += transform.forward * speed;
      }

      if (Controls.Key("backward")) {
        position -= transform.forward * speed;
      }

      if (Controls.Key("left")) {
        position -= transform.right * speed;
      }

      if (Controls.Key("right")) {
        position += transform.right * speed;
      }

      if (Controls.Key("up")) {
        position += Vector3.up * speed;
      }

      if (Controls.Key("down")) {
        position -= Vector3.up * speed;
      }

      return position;
    }
  }
}