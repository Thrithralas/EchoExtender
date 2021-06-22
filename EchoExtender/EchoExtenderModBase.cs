﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using BepInEx;
using RWCustom;
using UnityEngine;

namespace EchoExtender {
    

    [BepInPlugin("com.rainworldgame.echoextender.plugin", "Echo Extender", "0.1")]
    public class EchoExtenderModBase : BaseUnityPlugin {


        public static RainWorldGame GameInstance;
        public EchoExtenderModBase() {
            
            On.GhostWorldPresence.ctor += GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += GhostWorldPresenceOnGetGhostID;
            On.Ghost.ctor += GhostOnCtor;
            On.Ghost.StartConversation += GhostOnStartConversation;
            On.GhostConversation.AddEvents += GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresenceOnSpawnGhost;
            On.GhostWorldPresence.GhostMode_AbstractRoom_Vector2 += GhostWorldPresenceOnGhostMode;
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgressionOnGetOrInitiateSaveState;
            On.RainWorld.Start += RainWorldOnStart;

        }

        private float GhostWorldPresenceOnGhostMode(On.GhostWorldPresence.orig_GhostMode_AbstractRoom_Vector2 orig, GhostWorldPresence self, AbstractRoom testRoom, Vector2 worldPos) {
            var result = orig(self, testRoom, worldPos);
            if (!CRSEchoParser.EchoSettings.TryGetValue(self.ghostID, out var settings)) return result;
            var echoEffectLimit = settings.EffectRadius * 1000f; //I think 1 screen is like a 1000 so I'm going with that
            Vector2 globalDistance = Custom.RestrictInRect(worldPos, FloatRect.MakeFromVector2(self.world.RoomToWorldPos(new Vector2(), self.ghostRoom.index), self.world.RoomToWorldPos(self.ghostRoom.size.ToVector2() * 20f, self.ghostRoom.index)));
            if (!Custom.DistLess(worldPos, globalDistance, echoEffectLimit)) return 0;
            var someValue = self.DegreesOfSeparation(testRoom); //No clue what this number does
            return someValue == -1 
                ? 0.0f 
                : (float) (Mathf.Pow(Mathf.InverseLerp(echoEffectLimit, echoEffectLimit / 8f, Vector2.Distance(worldPos, globalDistance)), 2f) * (double) Custom.LerpMap(someValue, 1f, 3f, 0.6f, 0.15f) * (testRoom.layer != self.ghostRoom.layer ? 0.600000023841858 : 1.0));
        }


        private void RainWorldOnStart(On.RainWorld.orig_Start orig, RainWorld self) {
            On.WorldLoader.ctor += WorldLoaderOnCtor;
            orig(self);
        }

        private void WorldLoaderOnCtor(On.WorldLoader.orig_ctor orig, object self, RainWorldGame game, int playercharacter, bool singleroomworld, string worldname, Region region, RainWorldGame.SetupValues setupvalues) {
            orig(self, game, playercharacter, singleroomworld, worldname, region, setupvalues);
            CRSEchoParser.GetEchoLocationInRegion(region.name);
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
            Debug.Log("[Echo Extender : Info] Difficulty Condition : " + (settings.SpawnOnDifficulty.Contains(GameInstance.StoryCharacter) ? "Met" : "Not Met"));
            Debug.Log("[Echo Extender : Info] Karma Cap Condition : " + (karmacap >= settings.MinimumKarmaCap - 1 ? "Met" : "Not Met"));
            Debug.Log("[Echo Extender : Info] Karma Condition : " + (karma >= settings.MinimumKarma - 1 ? "Met" : "Not Met"));
            Debug.Log("[Echo Extender : Info] Priming Condition : " + (ghostpreviouslyencountered >= (settings.RequirePriming ? 1 : 0) ? "Met" : "Not Met"));
            Debug.Log("[Echo Extender : Info] Echo Visit Count : " + ghostpreviouslyencountered);
            return settings.SpawnOnDifficulty.Contains(GameInstance.StoryCharacter) && karmacap >= settings.MinimumKarmaCap - 1 && karma >= settings.MinimumKarma - 1 && ghostpreviouslyencountered >= (settings.RequirePriming ? 1 : 0) && ghostpreviouslyencountered < 2;
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
            string echoRegionString = self.worldGhost.ghostID.ToString();
            self.currentConversation = new GhostConversation(CRSEchoParser.GetConversationID(echoRegionString), self, self.room.game.cameras[0].hud.dialogBox);
        }

        private GhostWorldPresence.GhostID GhostWorldPresenceOnGetGhostID(On.GhostWorldPresence.orig_GetGhostID orig, string regionname) {
            var origResult = orig(regionname);
            return CRSEchoParser.EchoIDExists(regionname) ? CRSEchoParser.GetEchoID(regionname) : origResult;
        }

        private void GhostWorldPresenceOnCtor(On.GhostWorldPresence.orig_ctor orig, GhostWorldPresence self, World world, GhostWorldPresence.GhostID ghostid) {
            orig(self, world, ghostid);
            if (self.ghostRoom is null) {
                string region = ghostid.ToString();
                self.ghostRoom = CRSEchoParser.EchoLocations.ContainsKey(region) ? world.GetAbstractRoom(CRSEchoParser.EchoLocations[region]) : world.abstractRooms[0];
                self.songName = CRSEchoParser.EchoSettings.ContainsKey(ghostid) ? CRSEchoParser.EchoSettings[ghostid].EchoSong : EchoSettings.Default.EchoSong;
            }
        }

        private void GhostOnCtor(On.Ghost.orig_ctor orig, Ghost self, Room room, PlacedObject placedobject, GhostWorldPresence worldghost) {
            orig(self, room, placedobject, worldghost);
            if (!CRSEchoParser.ExtendedEchoIDs.Contains(self.worldGhost.ghostID)) return;
            var settings = CRSEchoParser.EchoSettings[self.worldGhost.ghostID];
            self.scale = settings.EchoSizeMultiplier * 0.75f;
        }
    }
}