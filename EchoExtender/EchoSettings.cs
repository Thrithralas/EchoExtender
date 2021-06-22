using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace EchoExtender {
    public struct EchoSettings {
        public float EchoSizeMultiplier;
        public float EffectRadius;
        public bool RequirePriming;
        public int MinimumKarma;
        public int MinimumKarmaCap;
        public int[] SpawnOnDifficulty;
        public string EchoSong;

        public static EchoSettings Default => new EchoSettings {EchoSizeMultiplier = 1f, EffectRadius = 1, MinimumKarma = 0, MinimumKarmaCap = 0, RequirePriming = false, SpawnOnDifficulty = new[] {0, 1, 2}, EchoSong = "NA_34 - Else3"};

        public static EchoSettings FromFile(string path) {
            Debug.Log("[Echo Extender : Info] Found settings file: " + path);
            string[] rows = File.ReadAllLines(path);
            EchoSettings settings = Default;
            foreach (string row in rows) {
                if (row.StartsWith("#")) continue;
                try {
                    string[] split = row.Split(':');
                    switch (split[0].Trim().ToLower()) {
                        case "size":
                            settings.EchoSizeMultiplier = float.Parse(split[1]);
                            Debug.Log("[Echo Extender : Info] Settings size multiplier to " + settings.EchoSizeMultiplier);
                            break;
                        case "radius":
                            settings.EffectRadius = float.Parse(split[1]);
                            Debug.Log("[Echo Extender : Info] Settings effect radius to " + settings.EffectRadius);
                            break;
                        case "priming":
                            settings.RequirePriming = bool.Parse(split[1]);
                            Debug.Log(settings.RequirePriming ? "[Echo Extender : Info] Enabling priming" : "[Echo Extender : Info] Disabling priming");
                            break;
                        case "minkarma":
                            settings.MinimumKarma = int.Parse(split[1]);
                            Debug.Log("[Echo Extender : Info] Setting minimum karma to " + settings.MinimumKarma);
                            break;
                        case "minkarmacap":
                            settings.MinimumKarmaCap = int.Parse(split[1]);
                            Debug.Log("[Echo Extender : Info] Setting minimum karma cap to " + settings.MinimumKarmaCap);
                            break;
                        case "difficulties":
                            settings.SpawnOnDifficulty = split[1].Split(',').Select(s => int.Parse(s.Trim())).ToArray();
                            Debug.Log("[Echo Extender : Info] Difficulties set to " + string.Join(", ", settings.SpawnOnDifficulty.Select(i => i.ToString()).ToArray()));
                            break;
                        case "echosong":
                            string trimmed = split[1].Trim();
                            settings.EchoSong = CRSEchoParser.EchoSongs.TryGetValue(trimmed, out string song) ? song : trimmed;
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
    }
}