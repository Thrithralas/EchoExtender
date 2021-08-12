using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EchoExtender {
    public struct EchoSettings {
        public Dictionary<int, float> EchoSizeMultiplier;
        public Dictionary<int, float> EffectRadius;
        public Dictionary<int, bool> RequirePriming;
        public Dictionary<int, int> MinimumKarma;
        public Dictionary<int, int> MinimumKarmaCap;
        public int[] SpawnOnDifficulty;
        public Dictionary<int, string> EchoSong;

        public float GetSizeMultiplier(int diff) {
            if (EchoSizeMultiplier.ContainsKey(diff)) return EchoSizeMultiplier[diff];
            return Default.EchoSizeMultiplier[diff];
        }

        public float GetRadius(int diff) {
            if (EffectRadius.ContainsKey(diff)) return EffectRadius[diff];
            return Default.EffectRadius[diff];
        }
        
        public bool GetPriming(int diff) {
            if (RequirePriming.ContainsKey(diff)) return RequirePriming[diff];
            return Default.RequirePriming[diff];
        }
        public int GetMinimumKarma(int diff) {
            if (MinimumKarma.ContainsKey(diff)) return MinimumKarma[diff];
            return Default.MinimumKarma[diff];
        }
        public int GetMinimumKarmaCap(int diff) {
            if (MinimumKarmaCap.ContainsKey(diff)) return MinimumKarmaCap[diff];
            return Default.MinimumKarmaCap[diff];
        }
        public string GetEchoSong(int diff) {
            if (EchoSong.ContainsKey(diff)) return EchoSong[diff];
            return Default.EchoSong[diff];
        }

        public static EchoSettings Default;

        static EchoSettings() {
            Default = Empty;
            Default.RequirePriming.AddMultiple(true, 0, 1);
            Default.RequirePriming.Add(2, false);
            Default.EffectRadius.AddMultiple(4, 0, 1, 2);
            Default.MinimumKarma.AddMultiple(-1, 0, 1, 2);
            Default.MinimumKarmaCap.AddMultiple(0, 0, 1, 2);
            Default.SpawnOnDifficulty = new[] { 0, 1, 2 };
            Default.EchoSong.AddMultiple("NA_32 - Else1", 0, 1, 2);
            Default.EchoSizeMultiplier.AddMultiple(1, 0, 1, 2);
        }

        public static EchoSettings Empty => new EchoSettings() {
            EchoSizeMultiplier = new Dictionary<int, float>(),
            EffectRadius = new Dictionary<int, float>(),
            MinimumKarma = new Dictionary<int, int>(),
            MinimumKarmaCap = new Dictionary<int, int>(),
            RequirePriming = new Dictionary<int, bool>(),
            EchoSong = new Dictionary<int, string>(),
            SpawnOnDifficulty = new int[3],
        };
        public static EchoSettings FromFile(string path) {
            Debug.Log("[Echo Extender : Info] Found settings file: " + path);
            string[] rows = File.ReadAllLines(path);
            EchoSettings settings = Empty;
            foreach (string row in rows) {
                if (row.StartsWith("#")) continue;
                try {
                    string[] split = row.Split(':');
                    string pass = split[0];
                    List<int> difficulties = new List<int>();
                    if (pass.StartsWith("(")) {
                        foreach (string rawNum in pass.Substring(1, pass.IndexOf(')')).SplitAndREE(",")) {
                            if (!int.TryParse(rawNum, out int result)) {
                                Debug.Log("[Echo Extender : Warning] Found a non-integer difficulty! Skipping");
                                continue;
                            }
                            difficulties.Add(result);
                        }
                    }
                    switch (pass.Trim().ToLower()) {
                        case "size":
                            if (difficulties.Count == 0) settings.EchoSizeMultiplier.AddMultiple(float.Parse(split[1]), 0, 1, 2);
                            else settings.EchoSizeMultiplier.AddMultiple(float.Parse(split[1]), difficulties);
                            break;
                        case "radius":
                            if (difficulties.Count == 0) settings.EffectRadius.AddMultiple(float.Parse(split[1]), 0, 1, 2);
                            else settings.EffectRadius.AddMultiple(float.Parse(split[1]), difficulties);
                            break;
                        case "priming":
                            if (difficulties.Count == 0) settings.RequirePriming.AddMultiple(bool.Parse(split[1]), 0, 1, 2);
                            else settings.RequirePriming.AddMultiple(bool.Parse(split[1]), difficulties);
                            break;
                        case "minkarma":
                            if (difficulties.Count == 0) settings.MinimumKarma.AddMultiple(int.Parse(split[1]), 0, 1, 2);
                            else settings.MinimumKarma.AddMultiple(int.Parse(split[1]), difficulties);
                            break;
                        case "minkarmacap":
                            if (difficulties.Count == 0) settings.MinimumKarmaCap.AddMultiple(int.Parse(split[1]), 0, 1, 2);
                            else settings.MinimumKarmaCap.AddMultiple(int.Parse(split[1]), difficulties);
                            break;
                        case "difficulties":
                            settings.SpawnOnDifficulty = split[1].Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                            break;
                        case "echosong":
                            string trimmed = split[1].Trim();
                            string result = CRSEchoParser.EchoSongs.TryGetValue(trimmed, out string song) ? song : trimmed;
                            if (difficulties.Count == 0) settings.EchoSong.AddMultiple(result, 0, 1, 2);
                            else settings.EchoSong.AddMultiple(result, difficulties);
                            Debug.Log("[Echo Extender : Info] Setting song to " + settings.EchoSong);
                            break;
                    }
                }

                catch (Exception) {
                    Debug.Log("[Echo Extender : Error] Failed to parse line " + row);
                }
                
            }
            
            return settings;
        }

        public bool KarmaCondition(int karma, int karmaCap, int diff) {
            if (GetMinimumKarma(diff) == -1) {
                switch (karmaCap) {
                    case 4:
                        return karma >= 4;
                    case 6:
                        return karma >= 5;
                    default:
                        return karma >= 6;
                }
            }

            return karma >= GetMinimumKarma(diff);
        }
    }
}