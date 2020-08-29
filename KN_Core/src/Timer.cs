using UnityEngine;

namespace KN_Core {
  public class Timer {
    public float MaxTime { get; set; }
    public float CurrentTime { get; set; }

    public bool IsStarted { get; set; }

    public delegate void TimerCallback();
    public event TimerCallback Callback;

    private readonly bool runOnce_;

    public Timer(float maxTime, bool runOnce = false) {
      MaxTime = maxTime;
      runOnce_ = runOnce;
      CurrentTime = 0.0f;
      IsStarted = false;
    }

    public void Update() {
      if (!runOnce_ && !IsStarted) {
        IsStarted = true;
      }

      if (IsStarted) {
        CurrentTime += Time.deltaTime;
        if (CurrentTime >= MaxTime) {
          CurrentTime = 0.0f;
          Callback?.Invoke();
          IsStarted = !runOnce_;
        }
      }
    }
  }
}