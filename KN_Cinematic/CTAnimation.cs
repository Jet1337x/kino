using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KN_Core;
using UnityEngine;

namespace KN_Cinematic {
  public class CTAnimation {
    public List<CTKeyframe> Keyframes { get; }
    public CTKeyframe CurrentFrame { get; private set; }

    public bool AllowPlay { get; set; }

    public float Smooth { get; set; }

    public float ActualLength { get; private set; }
    public float BeginTime { get; set; }

    //position
    private AnimationCurve curveX_;
    private readonly List<Keyframe> keyframesX_;

    private AnimationCurve curveY_;
    private readonly List<Keyframe> keyframesY_;

    private AnimationCurve curveZ_;
    private readonly List<Keyframe> keyframesZ_;

    //rotation
    private AnimationCurve curveRx_;
    private readonly List<Keyframe> keyframesRX_;

    private AnimationCurve curveRy_;
    private readonly List<Keyframe> keyframesRY_;

    private AnimationCurve curveRz_;
    private readonly List<Keyframe> keyframesRZ_;

    private AnimationCurve curveRw_;
    private readonly List<Keyframe> keyframesRW_;

    private AnimationCurve curveFov_;
    private readonly List<Keyframe> keyframesFov_;

    private AnimationCurve curvePitch_;
    private readonly List<Keyframe> keyframesPitch_;

    private AnimationCurve curveYaw_;
    private readonly List<Keyframe> keyframesYaw_;

    private AnimationCurve curveRoll_;
    private readonly List<Keyframe> keyframesRoll_;

    private AnimationCurve curveHeadingX_;
    private readonly List<Keyframe> keyframesHeadingX_;

    private AnimationCurve curveHeadingY_;
    private readonly List<Keyframe> keyframesHeadingY_;

    private AnimationCurve curveHeadingZ_;
    private readonly List<Keyframe> keyframesHeadingZ_;

    public Cinematic Container { get; }

    public CTAnimation(Cinematic container) {
      Container = container;
      CurrentFrame = null;
      Smooth = 1.0f;
      ActualLength = 1.0f;
      BeginTime = 0.0f;

      Keyframes = new List<CTKeyframe>();

      keyframesX_ = new List<Keyframe>();
      keyframesY_ = new List<Keyframe>();
      keyframesZ_ = new List<Keyframe>();

      keyframesRX_ = new List<Keyframe>();
      keyframesRY_ = new List<Keyframe>();
      keyframesRZ_ = new List<Keyframe>();
      keyframesRW_ = new List<Keyframe>();

      keyframesFov_ = new List<Keyframe>();

      keyframesPitch_ = new List<Keyframe>();
      keyframesYaw_ = new List<Keyframe>();
      keyframesRoll_ = new List<Keyframe>();

      keyframesHeadingX_ = new List<Keyframe>();
      keyframesHeadingY_ = new List<Keyframe>();
      keyframesHeadingZ_ = new List<Keyframe>();

      //pre init to avoid nullptr ref
      curveX_ = new AnimationCurve();
      curveY_ = new AnimationCurve();
      curveZ_ = new AnimationCurve();
      curveRx_ = new AnimationCurve();
      curveRy_ = new AnimationCurve();
      curveRz_ = new AnimationCurve();
      curveRw_ = new AnimationCurve();
      curveFov_ = new AnimationCurve();
      curvePitch_ = new AnimationCurve();
      curveYaw_ = new AnimationCurve();
      curveRoll_ = new AnimationCurve();
      curveHeadingX_ = new AnimationCurve();
      curveHeadingY_ = new AnimationCurve();
      curveHeadingZ_ = new AnimationCurve();
    }

    public void Add(CTKeyframe keyframe) {
      keyframe.Time = keyframe.Time - BeginTime < 0.0f ? 0.0f : keyframe.Time - BeginTime;
      Keyframes.Add(keyframe);
      Keyframes.Sort((kf0, kf1) => kf0.Time.CompareTo(kf1.Time));

      DisableAllKeyframes();
      CurrentFrame = keyframe;
      CurrentFrame.Active = true;

      UpdateActualLength();
      MakeAnimation();
    }

    private async Task<List<CTKeyframe>> MakeSmoothCurve(IReadOnlyCollection<CTKeyframe> frames, float smooth) {
      if (smooth < 1.0f) {
        smooth = 1.0f;
      }

      int curvedLength = frames.Count * Mathf.RoundToInt(smooth) - 1;
      var curvedFrames = new List<CTKeyframe>(curvedLength);

      var tasks = new List<Task<CTKeyframe>>();
      for (int frameInTimeOnCurve = 0; frameInTimeOnCurve < curvedLength + 1; frameInTimeOnCurve++) {
        float t = Mathf.InverseLerp(0.0f, curvedLength, frameInTimeOnCurve);
        tasks.Add(ProcessKeyframes(frames, t));
      }

      await Task.WhenAll(tasks);
      curvedFrames.AddRange(tasks.Select(t => t.Result));

      return curvedFrames;
    }

    private async Task<CTKeyframe> ProcessKeyframes(IReadOnlyCollection<CTKeyframe> values, float t) {
      var points = new List<CTKeyframe>(values);
      for (int j = values.Count - 1; j > 0; j--) {
        for (int i = 0; i < j; i++) {
          points[i] = new CTKeyframe(this, points[i], points[i + 1], t);
        }
      }
      return await Task.FromResult(points[0]);
    }

    private Task<bool> ProcessPos(IEnumerable<CTKeyframe> values) {
      foreach (var v in values) {
        var position = v.Position;
        keyframesX_.Add(new Keyframe(v.Time, position.x));
        keyframesY_.Add(new Keyframe(v.Time, position.y));
        keyframesZ_.Add(new Keyframe(v.Time, position.z));
      }
      return Task.FromResult(true);
    }

    private Task<bool> ProcessRot(IEnumerable<CTKeyframe> values) {
      foreach (var v in values) {
        var rotation = v.Rotation;
        keyframesRX_.Add(new Keyframe(v.Time, rotation.x));
        keyframesRY_.Add(new Keyframe(v.Time, rotation.y));
        keyframesRZ_.Add(new Keyframe(v.Time, rotation.z));
        keyframesRW_.Add(new Keyframe(v.Time, rotation.w));
      }
      return Task.FromResult(true);
    }

    private Task<bool> ProcessAngles(IEnumerable<CTKeyframe> values) {
      foreach (var v in values) {
        keyframesFov_.Add(new Keyframe(v.Time, v.Fov));

        keyframesPitch_.Add(new Keyframe(v.Time, v.Pitch));
        keyframesYaw_.Add(new Keyframe(v.Time, v.Yaw));
        keyframesRoll_.Add(new Keyframe(v.Time, v.Roll));
      }
      return Task.FromResult(true);
    }

    private Task<bool> ProcessHeading(IEnumerable<CTKeyframe> values) {
      foreach (var v in values) {
        keyframesHeadingX_.Add(new Keyframe(v.Time, v.HeadingX));
        keyframesHeadingY_.Add(new Keyframe(v.Time, v.HeadingY));
        keyframesHeadingZ_.Add(new Keyframe(v.Time, v.HeadingZ));
      }
      return Task.FromResult(true);
    }

    public void MakeAnimation() {
      ClearBuffers();

      List<CTKeyframe> values;
      if (Smooth >= 2.0f) {
        float smoothCorrected = Smooth * ActualLength;
        values = MakeSmoothCurve(Keyframes, smoothCorrected).Result;
      }
      else {
        values = Keyframes;
      }

      Log.Write($"[KN_Animation]: Keyframes count: {values.Count}");

      var tasks = new List<Task<bool>> {
        ProcessPos(values),
        ProcessRot(values),
        ProcessAngles(values),
        ProcessHeading(values)
      };

      Task.WhenAll(tasks);

      curveX_ = new AnimationCurve(keyframesX_.ToArray());
      for (int i = 0; i < curveX_.keys.Length; i++) {
        curveX_.SmoothTangents(i, 0.0f);
      }

      curveY_ = new AnimationCurve(keyframesY_.ToArray());
      for (int i = 0; i < curveY_.keys.Length; i++) {
        curveY_.SmoothTangents(i, 0.0f);
      }

      curveZ_ = new AnimationCurve(keyframesZ_.ToArray());
      for (int i = 0; i < curveZ_.keys.Length; i++) {
        curveZ_.SmoothTangents(i, 0.0f);
      }

      curveRx_ = new AnimationCurve(keyframesRX_.ToArray());
      for (int i = 0; i < curveRx_.keys.Length; i++) {
        curveRx_.SmoothTangents(i, 0.0f);
      }

      curveRy_ = new AnimationCurve(keyframesRY_.ToArray());
      for (int i = 0; i < curveRy_.keys.Length; i++) {
        curveRy_.SmoothTangents(i, 0.0f);
      }

      curveRz_ = new AnimationCurve(keyframesRZ_.ToArray());
      for (int i = 0; i < curveRz_.keys.Length; i++) {
        curveRz_.SmoothTangents(i, 0.0f);
      }

      curveRw_ = new AnimationCurve(keyframesRW_.ToArray());
      for (int i = 0; i < curveRw_.keys.Length; i++) {
        curveRw_.SmoothTangents(i, 0.0f);
      }


      curveFov_ = new AnimationCurve(keyframesFov_.ToArray());
      for (int i = 0; i < curveFov_.keys.Length; i++) {
        curveFov_.SmoothTangents(i, 0.0f);
      }


      curvePitch_ = new AnimationCurve(keyframesPitch_.ToArray());
      for (int i = 0; i < curvePitch_.keys.Length; i++) {
        curvePitch_.SmoothTangents(i, 0.0f);
      }

      curveYaw_ = new AnimationCurve(keyframesYaw_.ToArray());
      for (int i = 0; i < curveYaw_.keys.Length; i++) {
        curveYaw_.SmoothTangents(i, 0.0f);
      }

      curveRoll_ = new AnimationCurve(keyframesRoll_.ToArray());
      for (int i = 0; i < curveRoll_.keys.Length; i++) {
        curveRoll_.SmoothTangents(i, 0.0f);
      }


      curveHeadingX_ = new AnimationCurve(keyframesHeadingX_.ToArray());
      for (int i = 0; i < curveHeadingX_.keys.Length; i++) {
        curveHeadingX_.SmoothTangents(i, 0.0f);
      }

      curveHeadingY_ = new AnimationCurve(keyframesHeadingY_.ToArray());
      for (int i = 0; i < curveHeadingY_.keys.Length; i++) {
        curveHeadingY_.SmoothTangents(i, 0.0f);
      }

      curveHeadingZ_ = new AnimationCurve(keyframesHeadingZ_.ToArray());
      for (int i = 0; i < curveHeadingZ_.keys.Length; i++) {
        curveHeadingZ_.SmoothTangents(i, 0.0f);
      }


      curveX_.postWrapMode = WrapMode.Loop;
      curveY_.postWrapMode = WrapMode.Loop;
      curveZ_.postWrapMode = WrapMode.Loop;

      curveRx_.postWrapMode = WrapMode.Loop;
      curveRy_.postWrapMode = WrapMode.Loop;
      curveRz_.postWrapMode = WrapMode.Loop;
      curveRw_.postWrapMode = WrapMode.Loop;

      curvePitch_.postWrapMode = WrapMode.Loop;
      curveYaw_.postWrapMode = WrapMode.Loop;
      curveFov_.postWrapMode = WrapMode.Loop;

      curveHeadingX_.postWrapMode = WrapMode.Loop;
      curveHeadingY_.postWrapMode = WrapMode.Loop;
      curveHeadingZ_.postWrapMode = WrapMode.Loop;

      AllowPlay = true;
    }

    private void ClearBuffers() {
      keyframesX_.Clear();
      keyframesY_.Clear();
      keyframesZ_.Clear();

      keyframesRX_.Clear();
      keyframesRY_.Clear();
      keyframesRZ_.Clear();
      keyframesRW_.Clear();

      keyframesFov_.Clear();

      keyframesPitch_.Clear();
      keyframesYaw_.Clear();
      keyframesRoll_.Clear();

      keyframesHeadingX_.Clear();
      keyframesHeadingY_.Clear();
      keyframesHeadingZ_.Clear();
    }

    public void Reset() {
      CurrentFrame = null;
      AllowPlay = false;
      ActualLength = 0.0f;
      BeginTime = 0.0f;

      Keyframes.Clear();

      ClearBuffers();

      //pos
      for (int i = 0; i < curveX_.length; i++) {
        curveX_.RemoveKey(i);
        curveY_.RemoveKey(i);
        curveZ_.RemoveKey(i);
      }

      //rot
      for (int i = 0; i < curveRx_.length; i++) {
        curveRx_.RemoveKey(i);
        curveRy_.RemoveKey(i);
        curveRz_.RemoveKey(i);
        curveRw_.RemoveKey(i);
      }

      //fov
      for (int i = 0; i < curveFov_.length; i++) {
        curveFov_.RemoveKey(i);
      }

      //pitch
      for (int i = 0; i < curvePitch_.length; i++) {
        curvePitch_.RemoveKey(i);
      }

      //yaw
      for (int i = 0; i < curveYaw_.length; i++) {
        curveYaw_.RemoveKey(i);
      }

      //roll
      for (int i = 0; i < curveRoll_.length; i++) {
        curveRoll_.RemoveKey(i);
      }

      //hc
      for (int i = 0; i < curveHeadingX_.length; i++) {
        curveHeadingX_.RemoveKey(i);
      }

      //hy
      for (int i = 0; i < curveHeadingY_.length; i++) {
        curveHeadingY_.RemoveKey(i);
      }

      //hz
      for (int i = 0; i < curveHeadingZ_.length; i++) {
        curveHeadingZ_.RemoveKey(i);
      }
    }

    public void Update(CTCamera camera, float time, ref Vector3 heading, bool lookAt, bool hookTo) {
      if (Keyframes.Count < 1) {
        return;
      }

      time -= BeginTime;
      if (time >= ActualLength || time < 0.0f) {
        return;
      }

      //pick current frame to display in keyframes list
      for (int i = 0; i < Keyframes.Count - 1; i++) {
        float dif = (Keyframes[i + 1].Time - Keyframes[i].Time) / 2.0f;
        if (Keyframes[i + 1].Time - dif >= time) {
          CurrentFrame.Active = false;
          CurrentFrame = Keyframes[i];
          CurrentFrame.Active = true;
          break;
        }
        if (i + 1 == Keyframes.Count - 1) {
          CurrentFrame.Active = false;
          CurrentFrame = Keyframes[i + 1];
          CurrentFrame.Active = true;
        }
      }

      if (!hookTo) {
        float x = curveX_.Evaluate(time);
        float y = curveY_.Evaluate(time);
        float z = curveZ_.Evaluate(time);
        camera.GameObject.transform.position = new Vector3(x, y, z);
      }

      if (lookAt) {
        heading.x = curveHeadingX_.Evaluate(time);
        heading.y = curveHeadingY_.Evaluate(time);
        heading.z = curveHeadingZ_.Evaluate(time);
      }
      else {
        float rx = curveRx_.Evaluate(time);
        float ry = curveRy_.Evaluate(time);
        float rz = curveRz_.Evaluate(time);
        float rw = curveRw_.Evaluate(time);

        float pitch = curvePitch_.Evaluate(time);
        float yaw = curveYaw_.Evaluate(time);
        float roll = curveRoll_.Evaluate(time);

        camera.GameObject.transform.rotation = new Quaternion(rx, ry, rz, rw) *
                                               Quaternion.AngleAxis(pitch, Vector3.right) *
                                               Quaternion.AngleAxis(yaw, Vector3.up) *
                                               Quaternion.AngleAxis(roll, Vector3.forward);
      }

      float fov = curveFov_.Evaluate(time);
      camera.Fov = fov;
    }

    private void UpdateActualLength() {
      ActualLength = 0.001f;
      foreach (var kf in Keyframes) {
        float time = kf.Time - BeginTime;
        if (time > ActualLength) {
          ActualLength = time;
        }
      }
    }

    public void Scale(float newLength) {
      float change = (newLength - ActualLength) / Mathf.Abs(ActualLength);
      Keyframes.ForEach(kf => kf.Time = kf.Time * change + kf.Time);
      ActualLength = newLength;

      MakeAnimation();
    }

    public void RemoveKeyframe(CTKeyframe keyframe) {
      bool found = false;
      CTKeyframe match = null;
      foreach (var kf in Keyframes) {
        if (keyframe == kf) {
          found = true;
        }
        else {
          match = kf;
        }
      }

      if (found) {
        Keyframes.Remove(keyframe);

        if (match != null) {
          CurrentFrame = match;
        }
        else {
          CurrentFrame = Keyframes.Count > 0 ? Keyframes.First() : null;
        }
      }

      if (Keyframes.Count > 0) {
        UpdateActualLength();
        MakeAnimation();
      }
    }

    private void DisableAllKeyframes() {
      foreach (var kf in Keyframes) {
        kf.Active = false;
      }
    }

    public void ToggleKeyframe(CTKeyframe keyframe) {
      bool active = keyframe.Active;
      DisableAllKeyframes();
      if (active) {
        keyframe.Active = true;
        CurrentFrame = keyframe;
      }
      else {
        CurrentFrame = null;
      }
    }
  }
}