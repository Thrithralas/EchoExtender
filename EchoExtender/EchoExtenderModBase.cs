using System;
using BepInEx;
using UnityEngine;

namespace EchoExtender {
    

    [BepInPlugin("com.rainworldgame.echoextender.plugin", "Echo Extender", "0.1")]
    public class EchoExtenderModBase : BaseUnityPlugin {

        public EchoExtenderModBase() {
            
            On.GhostWorldPresence.ctor += GhostWorldPresenceOnCtor;
            On.GhostWorldPresence.GetGhostID += GhostWorldPresenceOnGetGhostID;
            On.Ghost.ctor += GhostOnCtor;
            On.Ghost.StartConversation += GhostOnStartConversation;
            On.GhostConversation.AddEvents += GhostConversationOnAddEvents;
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresenceOnSpawnGhost;
            On.DeathPersistentSaveData.ctor += DeathPersistentSaveDataOnCtor;
            On.PlayerProgression.GetOrInitiateSaveState += PlayerProgressionOnGetOrInitiateSaveState;
            On.RainWorld.Start += RainWorldOnStart;

            On.PoleMimic.Update += (orig, self, eu) => {
                if (self.room.abstractRoom.creatures.Count == 0) Debug.Log("EMPTY");
                try {
                    orig(self, eu);
                }
                catch (ArgumentOutOfRangeException) {
                    Debug.Log("AOORE - EMPTY");
                }
            };
        }

        private void RainWorldOnStart(On.RainWorld.orig_Start orig, RainWorld self) {
            On.WorldLoader.ctor += WorldLoaderOnCtor;
            orig(self);
        }

        private void WorldLoaderOnCtor(On.WorldLoader.orig_ctor orig, object self, RainWorldGame game, int playercharacter, bool singleroomworld, string worldname, Region region, RainWorldGame.SetupValues setupvalues) {
            orig(self, game, playercharacter, singleroomworld, worldname, region, setupvalues);
            CRSEchoParser.GetEchoLocationInRegion(region.name);
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
            //You can check this in a fucking truth table if you really wanna
            return (playingasred || !settings.RequireHunter) && (karmacap >= settings.MinimumKarmaCap) && (karma >= settings.MinimumKarma) && (ghostpreviouslyencountered >= (settings.RequirePriming ? 1 : 0));
        }

        private void GhostConversationOnAddEvents(On.GhostConversation.orig_AddEvents orig, GhostConversation self) {
            orig(self);
            if (CRSEchoParser.EchoConversations.ContainsKey(self.id)) {
                foreach (string line in CRSEchoParser.EchoConversations[self.id].Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)) {
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
                self.songName = "NA_34 - Else3";
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