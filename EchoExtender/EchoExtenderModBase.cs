using System;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using RWCustom;
using UnityEngine;

namespace EchoExtender {
    

    [BepInPlugin("com.rainworldgame.echoextender.plugin", "Echo Extender", "0.9")]
    public class EchoExtenderModBase : BaseUnityPlugin {


        //TODO Figure out something better, probably breaks if player quits to main menu(?)
        // ReSharper disable once MemberCanBePrivate.Global
        public static RainWorldGame GameInstance;
        public void OnEnable() {
            
            On.RainWorld.Start += RainWorldOnStart;

        }

        private float GhostWorldPresenceOnGhostMode(On.GhostWorldPresence.orig_GhostMode_AbstractRoom_Vector2 orig, GhostWorldPresence self, AbstractRoom testRoom, Vector2 worldPos) {
            var result = orig(self, testRoom, worldPos);
            if (!CRSEchoParser.EchoSettings.TryGetValue(self.ghostID, out var settings)) return result;
            var echoEffectLimit = settings.GetRadius(GameInstance.StoryCharacter) * 1000f; //I think 1 screen is like a 1000 so I'm going with that
            Vector2 globalDistance = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(self.world.RoomToWorldPos(new Vector2(), self.ghostRoom.index), self.world.RoomToWorldPos(self.ghostRoom.size.ToVector2() * 20f, self.ghostRoom.index)));
            if (!Custom.DistLess(worldPos, globalDistance, echoEffectLimit)) return 0;
            var someValue = self.DegreesOfSeparation(testRoom); //No clue what this number does
            return someValue == -1 
                ? 0.0f 
                : (float) (Mathf.Pow(Mathf.InverseLerp(echoEffectLimit, echoEffectLimit / 8f, Vector2.Distance(worldPos, globalDistance)), 2f) * (double) Custom.LerpMap(someValue, 1f, 3f, 0.6f, 0.15f) * (testRoom.layer != self.ghostRoom.layer ? 0.600000023841858 : 1.0));
        }


        private void RainWorldOnStart(On.RainWorld.orig_Start orig, RainWorld self) {
            orig(self);
            On.WorldLoader.ctor += WorldLoaderOnCtor;
            On.GhostWorldPresence.ctor += GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += GhostWorldPresenceOnGetGhostID;
            On.Ghost.ctor += GhostOnCtor;
            On.Ghost.StartConversation += GhostOnStartConversation;
            On.GhostConversation.AddEvents += GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_AbstractRoom_Vector2 += GhostWorldPresenceOnGhostMode;
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgressionOnGetOrInitiateSaveState;
            On.Room.Loaded += RoomOnLoaded;
        }

        private void RoomOnLoaded(On.Room.orig_Loaded orig, Room self) {
            foreach (var pObj in self.roomSettings.placedObjects) {
                if (pObj.type == PlacedObject.Type.GhostSpot) {
                    Debug.Log($"[Echo Extender : RoomLoader] GhostSpot Active : {pObj.active}");
                    Debug.Log($"[Echo Extender : RoomLoader] GhostWorldPresence {(self.world.worldGhost is null ? "is null" : "is not null")}");
                    if (self.world.worldGhost is not null) {
                        Debug.Log($"[Echo Extender : RoomLoader] Room {(self.world.worldGhost.ghostRoom is null ? "is null" : "not null")}");
                    }
                }
            }

            orig(self);
        }

        private void WorldLoaderOnCtor(On.WorldLoader.orig_ctor orig, object self, RainWorldGame game, int playercharacter, bool singleroomworld, string worldname, Region region, RainWorldGame.SetupValues setupvalues) {
            orig(self, game, playercharacter, singleroomworld, worldname, region, setupvalues);
            if (game is null || game.IsArenaSession || singleroomworld) return;
            if (region is null) {
                Debug.Log("[Echo Extender : Warning] Region is NULL, skipping getting echo location.");
            }
            else {
                try {
                    CRSEchoParser.GetEchoLocationInRegion(region.name);
                }
                catch (Exception e) {
                    Debug.Log(e);
                }
            }
            GameInstance = game;
        }

        private SaveState PlayerProgressionOnGetOrInitiateSaveState(On.PlayerProgression.orig_GetOrInitiateSaveState orig, PlayerProgression self, int savestatenumber, RainWorldGame game, ProcessManager.MenuSetup setup, bool saveasdeathorquit) {
            CRSEchoParser.LoadAllCRSPacks();
            return orig(self, savestatenumber, game, setup, saveasdeathorquit);
        }
        
        private void DeathPersistentSaveDataOnCtor(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, int slugcat) {
            orig(self, slugcat);
            self.ghostsTalkedTo = new int[Enum.GetValues(typeof(GhostWorldPresence.GhostID)).Length];
        }

        private bool GhostWorldPresenceOnSpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostid, int karma, int karmacap, int ghostpreviouslyencountered, bool playingasred) {
            var result = orig(ghostid, karma, karmacap, ghostpreviouslyencountered, playingasred);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(ghostid)) return result;
            EchoSettings settings = CRSEchoParser.EchoSettings[ghostid];
            bool SODcondition = settings.SpawnOnDifficulty.Contains(GameInstance.StoryCharacter);
            bool karmaCondition = settings.KarmaCondition(karma, karmacap, GameInstance.StoryCharacter);
            bool karmaCapCondition = settings.GetMinimumKarmaCap(GameInstance.StoryCharacter) <= karmacap;
            Debug.Log($"[Echo Extender : Info] Getting echo conditions for {ghostid}");
            Debug.Log($"[Echo Extender : Info] Using difficulty {GameInstance.StoryCharacter}");
            Debug.Log($"[Echo Extender : Info] Spawn On Difficulty : {(SODcondition ? "Met" : "Not Met")}");
            Debug.Log($"[Echo Extender : Info] Minimum Karma : {(karmaCondition ? "Met" : "Not Met")} [Required: {settings.GetMinimumKarma(GameInstance.StoryCharacter)}, Having: {karma}]");
            Debug.Log($"[Echo Extender : Info] Minimum Karma Cap : {(karmaCapCondition ? "Met" : "Not Met")} [Required: {settings.GetMinimumKarmaCap(GameInstance.StoryCharacter)}, Having: {karmacap}]");
            bool prime = settings.GetPriming(GameInstance.StoryCharacter);
            bool primedCond = prime ? ghostpreviouslyencountered == 1 : ghostpreviouslyencountered != 2;
            Debug.Log($"[Echo Extender : Info] Primed : {(primedCond ? "Met" : "Not Met")} [Required: {(prime ? 1 : 0)}, Having {ghostpreviouslyencountered}]");
            Debug.Log($"[Echo Extender : Info] Spawning Echo : {primedCond && SODcondition && karmaCondition && karmaCapCondition}");
            return
                primedCond &&
                SODcondition &&
                karmaCondition &&
                karmaCapCondition;
        }

        private void GhostConversationOnAddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self) {
            orig(self);
            if (CRSEchoParser.EchoConversations.ContainsKey(self.id)) {
                foreach (string line in CRSEchoParser.EchoConversations[self.id].Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
                    if (line.StartsWith("(")) {
                        var difficulties = line.Substring(1, line.IndexOf(")", StringComparison.Ordinal) - 1);
                        foreach (string s in difficulties.Split(',')) {
                            if (int.Parse(s) == GameInstance.StoryCharacter) {
                                self.events.Add(new Conversation.TextEvent(self, 0, Regex.Replace(line, @"^\((\d|(\d+,)+\d)\)", ""), 0));
                                break;
                            }
                        }
                        continue;
                    }
                    self.events.Add(new Conversation.TextEvent(self, 0, line, 0));
                } 
            }
        }

        private void GhostOnStartConversation(On.Ghost.orig_StartConversation orig, Ghost self) {
            orig(self);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            string echoRegionString = self.worldGhost.ghostID.ToString();
            self.currentConversation = new GhostConversation(CRSEchoParser.GetConversationID(echoRegionString), self, self.room.game.cameras[0].hud.dialogBox);
        }

        private GhostWorldPresence.GhostID GhostWorldPresenceOnGetGhostID(On.GhostWorldPresence.orig_GetGhostID orig, string regionname) {
            var origResult = orig(regionname);
            return CRSEchoParser.EchoIDExists(regionname) ? CRSEchoParser.GetEchoID(regionname) : origResult;
        }

        private void GhostWorldPresenceOnCtor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostid) {
            orig(self, world, ghostid);
            if (self.ghostRoom is null && CRSEchoParser.ExtendedEchoIDs.Contains(self.ghostID)) {
                string region = ghostid.ToString();
                self.ghostRoom = CRSEchoParser.EchoLocations.ContainsKey(region) ? world.GetAbstractRoom(CRSEchoParser.EchoLocations[region]) : world.abstractRooms[0];
                self.songName = CRSEchoParser.EchoSettings[ghostid].GetEchoSong(world.game.StoryCharacter);
            }
            Debug.Log($"[Echo Extender : GWPCtor] Set Song: {self.songName}");
            Debug.Log($"[Echo Extender : GWPCtor] Set Room {self.ghostRoom.name}");
            
        }

        private void GhostOnCtor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost) {
            Debug.Log("OOGA BOOGA");
            orig(self, room, placedobject, worldghost);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            var settings = CRSEchoParser.EchoSettings[self.worldGhost.ghostID];
            self.scale = settings.GetSizeMultiplier(room.game.StoryCharacter) * 0.75f;
        }
    }
}