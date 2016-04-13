using UnityEngine;
using System.Collections;
using System;

public class VarsLoadDefaultSaveData {
    
    public static void load() {

        // username
        Vars.username = "";
        // created date
        Vars.createdDate = DateTime.Now;
        // modified date
        Vars.modifiedDate = DateTime.Now;
        // play time
        Vars.playTime = 0;
        // all node data
        NodeData.clearAllNodes();
        // current node data
        Vars.currentNodeData = NodeData.createNodeData(null, true);
        Vars.currentNodeData.time = 0;
        Vars.currentNodeData.level = "tut_ship_1";
        Vars.currentNodeData.levelMapX = 1;
        Vars.currentNodeData.levelMapY = 0;
        Vars.currentNodeData.position.Set(33, 14);
        Vars.currentNodeData.orbs.Clear();
        Vars.currentNodeData.hasBooster = false;
        Vars.currentNodeData.healthUpgrades.Clear();
        Vars.currentNodeData.phaseReplacements = 0;
        Vars.currentNodeData.physicalEvents.Clear();
        Vars.currentNodeData.levelsAmbushesDefeated.Clear();
        Vars.currentNodeData.objectsDestroyed.Clear();
        // decryptors
        Vars.decryptors.Clear();
        // info events
        Vars.infoEvents.Clear();
        // current objective conversation
        TalkPage.setCurrentObjectiveFile("co_first_tutorial");
        // all talk conversations
        TalkPage.conversations.Clear();
        TalkPage.addConversationNoAlert("Standard Conversations", "c_finish_sentences", false, false);
        TalkPage.addConversationNoAlert("Oracle's Flashbacks", "c_oracle_vision", true, false);
        TalkPage.addConversationNoAlert("(help) Basic Controls", "help_basic_controls", false, true);
        TalkPage.addConversationNoAlert("(help) Visions", "help_vision", false, true);
        TalkPage.addConversationNoAlert("(help) Flashback", "help_flashback", false, true);
        // pause screen lastPageOpened, mode, countdown timer visible, mode
        PauseScreen.lastPageOpened = PauseScreen.Page.TALK;
        PauseScreen.mode = PauseScreen.Mode.TUTORIAL;
        CountdownTimer.staticVisible = false;
        CountdownTimer.staticMode = CountdownTimer.Mode.NORMAL;
        // orbs found
        Vars.orbsFound.Clear();
        // booster found
        Vars.boosterFound = false;
        // health upgrades found
        Vars.healthUpgradesFound.Clear();
        // creature cards found
        Vars.creatureCardsFound.Clear();
        // map and map icons
        if (MapUI.instance == null) {
            MapUI.tempGridString = "";
            MapUI.tempIconString = "";
        } else {
            MapUI.instance.gridFromString("");
            MapUI.instance.iconsFromString("");
        }
        

        // for testing
#if UNITY_EDITOR

        ///*
        //collectDecryptor(Decryptor.ID.CHARGE_SHOT);
        //collectDecryptor(Decryptor.ID.ALTERED_SHOT);
        //collectDecryptor(Decryptor.ID.ROOM_RESTART);

        /*
        Vars.currentNodeData.creatureCardCollect("Sealime");
        Vars.currentNodeData.creatureCardCollect("Ciurivy");
        Vars.currentNodeData.creatureCardCollect("Smosey");
        Vars.currentNodeData.creatureCardCollect("Magoom");
        Vars.currentNodeData.creatureCardCollect("Pengrunt");
        creatureCardFind("Vengemole");
        Vars.currentNodeData.creatureCardCollect("Toucade");
        Vars.currentNodeData.creatureCardCollect("Sherivice");
        */
        //eventHappen(AdventureEvent.Info.FOUND_CREATURE_CARD);
        //PauseScreen.mode = PauseScreen.Mode.NORMAL;

#endif


    }

}
