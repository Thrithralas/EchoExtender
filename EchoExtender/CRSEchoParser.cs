using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PastebinMachine.EnumExtender;
using RWCustom;
using UnityEngine;

namespace EchoExtender {
    public static class CRSEchoParser {

        public static readonly Dictionary<Conversation.ID, string> EchoConversations = new Dictionary<Conversation.ID, string>();
        public static readonly HashSet<GhostWorldPresence.GhostID> ExtendedEchoIDs = new HashSet<GhostWorldPresence.GhostID>();
        public static readonly Dictionary<string, string> EchoLocations = new Dictionary<string, string>();
        public static readonly Dictionary<GhostWorldPresence.GhostID, EchoSettings> EchoSettings = new Dictionary<GhostWorldPresence.GhostID, EchoSettings>();

        public static readonly Dictionary<string, string> EchoSongs = new Dictionary<string, string> {
            {"CC", "NA_32 - Else1"},
            {"SI", "NA_38 - Else7"},
            {"LF", "NA_36 - Else5"},
            {"SH", "NA_34 - Else3"},
            {"UW", "NA_35 - Else4"},
            {"SB", "NA_33 - Else2"}
        };

        public static GhostWorldPresence.GhostID GetEchoID(string regionShort) => (GhostWorldPresence.GhostID) Enum.Parse(typeof(GhostWorldPresence.GhostID), regionShort);
        public static Conversation.ID GetConversationID(string regionShort) => (Conversation.ID) Enum.Parse(typeof(Conversation.ID), "Ghost_" + regionShort);
        public static bool EchoIDExists(string regionShort) {
            try {
                GetEchoID(regionShort);
                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        
        
        // ReSharper disable once InconsistentNaming
        public static void LoadAllCRSPacks() {
            string crsInstallations = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar + "CustomResources";
            foreach (var crsPack in Directory.GetDirectories(crsInstallations)) {
                Debug.Log("[Echo Extender : Info] Checking pack " + crsPack.Split(Path.DirectorySeparatorChar).Last() + " for custom Echoes");
                if (!CustomRegions.Mod.CustomWorldMod.activatedPacks.ContainsKey(crsPack.Split(Path.DirectorySeparatorChar).Last())) {
                    Debug.Log("[Echo Extender : Info] CRS Pack is disabled, skipping");
                    continue;
                }
                string regions = crsPack + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                if (!Directory.Exists(regions)) continue;
                foreach (var region in Directory.GetDirectories(regions)) {
                    string regionShort = region.Split(Path.DirectorySeparatorChar).Last();
                    Debug.Log("[Echo Extender : Info] Found region " + regionShort + "! Checking for Echo.");
                    string echoConv = region + Path.DirectorySeparatorChar + "echoConv.txt";
                    if (!File.Exists(echoConv)) {
                        Debug.Log("[Echo Extender : Info] No echoConv.txt found, skipping region!");
                        continue;
                    }
                    string conversationText = File.ReadAllText(echoConv);
                    string settingsPath = region + Path.DirectorySeparatorChar + "echoSettings.txt";
                    EchoSettings settings = File.Exists(settingsPath) ? EchoExtender.EchoSettings.FromFile(settingsPath) : EchoExtender.EchoSettings.Default;
                    if (EchoIDExists(regionShort)) {
                        Debug.Log("[Echo Extender : Warning] Region " + regionShort + " already has an echo assigned, skipping!");
                    }
                    else {
                        EnumExtender.AddDeclaration(typeof(GhostWorldPresence.GhostID), regionShort);
                        EnumExtender.AddDeclaration(typeof(Conversation.ID), "Ghost_" + regionShort);
                        EnumExtender.ExtendEnumsAgain();
                        ExtendedEchoIDs.Add(GetEchoID(regionShort));
                        EchoConversations.Add(GetConversationID(regionShort), conversationText);
                        Debug.Log("[Echo Extender : Info] Added conversation for echo in region " + regionShort);
                    }

                    EchoSettings.TryAdd(GetEchoID(regionShort), settings);


                }
            }
        }

        public static void GetEchoLocationInRegion(string regionShort) {
            if (!EchoIDExists(regionShort) || !ExtendedEchoIDs.Contains(GetEchoID(regionShort))) return;
            string crsInstallations = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "Mods" + Path.DirectorySeparatorChar + "CustomResources";
            foreach (var crsPack in Directory.GetDirectories(crsInstallations)) {
                Debug.Log("[Echo Extender : Info] Checking pack " + crsPack.Split(Path.DirectorySeparatorChar).Last() + " for Echo location");
                string region = crsPack + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + regionShort;
                if (Directory.Exists(region)) {
                    Debug.Log("[Echo Extender : Info] Region found! Checking for GhostSpot");
                    string rooms = region + Path.DirectorySeparatorChar + "Rooms";
                    foreach (var file in Directory.GetFiles(rooms)) {
                        string roomTxt = file.Split(Path.DirectorySeparatorChar).Last();
                        if (roomTxt.EndsWith("_Settings.txt")) {
                            if (File.ReadAllText(file).Contains("GhostSpot")) {
                                string roomName = roomTxt.Replace("_Settings.txt", "");
                                Debug.Log("[Echo Extender : Info] Registering Echo room " + roomName);
                                EchoLocations.TryAdd(regionShort, roomName);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}