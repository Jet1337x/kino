using System.Collections.Generic;
using KN_Loader;

namespace KN_Core {
  public static class SwapsLoader {
    public static bool LoadData(ref List<EngineData> engines, ref List<SwapBalance> balance) {
      return LoadEngines(ref engines) && LoadBalance(ref balance);
    }

    private static bool LoadEngines(ref List<EngineData> engines) {
      var data = WebDataLoader.LoadAsBytes("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIva25fZGF0YTAua25k");
      if (data == null) {
        return false;
      }

      Log.Write("[KN_Core::SwapsLoader]: Engine data loaded from remote");

      if (DataSerializer.Deserialize<EngineData>("KN_Swaps", data, out var enginesOut)) {
        engines.AddRange(enginesOut.ConvertAll(d => (EngineData) d));
        Log.Write($"[KN_Core::SwapsLoader]: Engine data parsed, count: {engines.Count}");
      }
      else {
        Log.Write("[KN_Core::SwapsLoader]: Unable to parse engine data");
        return false;
      }

      return true;
    }

    private static bool LoadBalance(ref List<SwapBalance> balance) {
      var data = WebDataLoader.LoadAsBytes("aHR0cHM6Ly9naXRodWIuY29tL3RyYmZseHIva2lub19kYXRhL3Jhdy9tYXN0ZXIva25fZGF0YTEua25k");
      if (data == null) {
        return false;
      }

      Log.Write("[KN_Core::SwapsLoader]: Balance data loaded from remote");

      if (DataSerializer.Deserialize<SwapBalance>("KN_Swaps", data, out var balanceOut)) {
        balance.AddRange(balanceOut.ConvertAll(d => (SwapBalance) d));
        Log.Write($"[KN_Core::SwapsLoader]: Balance data parsed, count: {balance.Count}");
      }
      else {
        Log.Write("[KN_Core::SwapsLoader]: Unable to parse balance data");
        return false;
      }

      return true;
    }
  }
}