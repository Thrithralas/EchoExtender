using System;
using System.IO;
using UnityEngine;

namespace EchoExtender {
    public struct EchoSettings {
        public float EchoSizeMultiplier;
        public int EffectRadius;
        public bool RequirePriming;
        public int MinimumKarma;
        public int MinimumKarmaCap;
        public bool RequireHunter;

        public static EchoSettings Default => new EchoSettings {EchoSizeMultiplier = 1f, EffectRadius = 1, MinimumKarma = 0, MinimumKarmaCap = 0, RequirePriming = false, RequireHunter = false};

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
                            break;
                        case "radius":
                            settings.EffectRadius = int.Parse(split[1]);
                            break;
                        case "priming":
                            settings.RequirePriming = bool.Parse(split[1]);
                            break;
                        case "minkarma":
                            settings.MinimumKarma = int.Parse(split[1]);
                            break;
                        case "minkarmacap":
                            settings.MinimumKarmaCap = int.Parse(split[1]);
                            break;
                        case "hunteronly":
                            settings.RequireHunter = bool.Parse(split[1]);
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