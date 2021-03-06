﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* uses EventHappener to record events so it's correctly handled when reverting */

public class ScriptRunner : EventHappener {

    public class Instruction {

        public enum ID {
            NONE,
            STOP_PLAYER,                // stopPlayer
            TALK,                       // t-NAME(PROFILE): TEXT // profile is optional and not implemented yet
            CLOSE_TEXT,                 // closeText
            WAIT,                       // wait 1.0
            RESUME_PLAYER,              // resumePlayer
            CUTSCENE_BARS_ON,           // cbarsOn [immediately] [block]
            CUTSCENE_BARS_OFF,          // cbarsOff [immediately] [block]
            CAMERA_FOLLOW_PLAYER,       // camFollowPlayer [1.0] [block]
            CAMERA_SET_POSITION,        // camSetPosition(5, 6) [1.0] [block]
            CAMERA_CUSTOM,              // camCustom
            CAMERA_ENABLE_BOUNDS,       // camEnableBounds
            CAMERA_DISABLE_BOUNDS,      // camDisableBounds
            LABEL,                      // lbl: 1 or lbl 1
            GOTO,                       // goto 1
            INFO_HAPPEN,                // infoHappen(4) where 4 is (int) of some AdventureEvent.Info
            PHYS_HAPPEN,                // physHappen(4) where 4 is (int) of some AdventureEvent.Physical
            JUMP_INFO,                  // jmpInfo(4) 3.  if (AdventureEvent.Info)4 has happened, then jumps to lbl 3
            JUMP_PHYS,                  // jmpPhys(4) 3.  if (AdventureEvent.Physical)4 has happened, then jumps to lbl 3
            SEND_MESSAGE,               // sendMessage thingObject Action [stringParam]
            SPAWN_CONTROL_MESSAGE,      // spawnControlMessage 1
            TAKE_DOWN_CONTROL_MESSAGE,  // takeDownControlMessage 1
            KEY_DOWN,                   // keyDown right
            KEY_UP,                     // keyUp right
            PLAYER_MOVE_X,              // playerMoveX 15 [block]
            PLAYER_FLIPPED_HORIZ,       // playerFlippedHoriz true
            DEBUGLOG,                   // DebugLog any text
            DESTROY_BOSS_HEALTH_BAR,    // destroyBossHealthBar
            SET_CURRENT_OBJECTIVE,      // setCurrentObjective co_newObjectiveFile
            TITLE,                      // title: Oracle's Flashbacks // title for conversations, doens't actually do anything here
            FREEZE_COUNTDOWN_TIMER,     // freezeTimer // freezes the countdown timer, so CountdownTimer.instance.time doesn't update
            UNFREEZE_COUNTDOWN_TIMER,   // unfreezeTimer // unfreezes the countdown timer
            PLAY_MUSIC,                 // playMusic songName [1.0] // brackets is fade in duration, default is .5 I think
            STOP_MUSIC,                 // stopMusic
            FADE_OUT_MUSIC,             // fadeOutMusic [1.0] // brackets is fade out duration, default is .5 I think
            LEVEL_PREVENT_MUSIC,        // levelPreventMusic // sets Level.doNotStartBGMusic to true, which means the next level that loads will not automatically start background music
        }

        public ID id = ID.NONE;

        public string text = "";
        public string name = "";
        public int int0 = 0;
        public int int1 = 0;
        public float float0 = 0;
        public float float1 = 0;
        public float float2 = 0;
        public string str0 = "";
        public bool blockToggle = false;

        public void clear() {
            id = ID.NONE;
            text = "";
            name = "";
            int0 = 0;
            int1 = 0;
            float0 = 0;
            float1 = 0;
            float2 = 0;
            str0 = "";
            blockToggle = false;
        }

        public void parse(string line) {
            // trim comment
            int index = line.IndexOf("//");
            int index2 = 0;
            int index3 = 0;
            string str = "";
            char[] trimChars = {' '};
            if (index != -1) {
                line = line.Substring(0, index);
            }
            // trim everything
            line = line.Trim();
            if (line == "") {
                id = ID.NONE;
                return;
            }
            // get first word
            index = line.IndexOf(' ');
            string word;
            if (index == -1) {
                word = line;
            } else {
                word = line.Substring(0, index);
            }
            word = word.ToLower();

            if (word == "stopplayer") { // stopPlayer
                id = ID.STOP_PLAYER;
            } else if (word.IndexOf("t-") == 0) { // t-NAME(PROFILE): TEXT // profile is optional and not implemented yet
                id = ID.TALK;
                index2 = word.IndexOf(":");
                name = line.Substring(2, index2 - 2);
                index3 = name.IndexOf("(");
                if (index3 == -1) {
                    str0 = "";
                } else {
                    str0 = name.Substring(index3 + 1, name.IndexOf(")") - index3 - 1);
                    name = name.Substring(0, index3);
                }
                if (name.ToLower() == "o")
                    name = "Oracle";
                else if (name.ToLower() == "w")
                    name = "Wally";
                text = line.Substring(index2 + 1).TrimStart(trimChars);
            } else if (word == "closetext") { // closeText
                id = ID.CLOSE_TEXT;
                index2 = line.LastIndexOf("block");
                if (index2 != -1)
                    blockToggle = true;
            } else if (word == "wait") { // wait 1.0
                id = ID.WAIT;
                float0 = float.Parse(line.Substring(5));
            } else if (word == "resumeplayer") { // resumePlayer
                id = ID.RESUME_PLAYER;
            } else if (word == "cbarson") { // cbarsOn
                id = ID.CUTSCENE_BARS_ON;
                index2 = line.LastIndexOf("block");
                if (index2 != -1)
                    blockToggle = true;
                index2 = line.LastIndexOf("immediately");
                if (index2 != -1) {
                    int0 = 1;
                    blockToggle = false;
                }
            } else if (word == "cbarsoff") { // cbarsOff
                id = ID.CUTSCENE_BARS_OFF;
                index2 = line.LastIndexOf("block");
                if (index2 != -1)
                    blockToggle = true;
                index2 = line.LastIndexOf("immediately");
                if (index2 != -1) {
                    int0 = 1;
                    blockToggle = false;
                }
            } else if (word == "camfollowplayer") { // camFollowPlayer 1.0
                id = ID.CAMERA_FOLLOW_PLAYER;
                index2 = line.LastIndexOf("block");
                index3 = line.Length; // index3 will be the "right" of the line, where the duration will attempt to be parsed
                if (index2 != -1) {
                    index3 = index2;
                    blockToggle = true;
                }
                str = line.Substring(16, index3 - 16).Trim();
                if (str != "") {
                    float0 = float.Parse(str);
                }
                if (float0 == 0 || float.IsNaN(float0)) {
                    float0 = 0;
                    blockToggle = false;
                }
            } else if (word.IndexOf("camsetposition") == 0) { // camSetPosition(5, 6) 1.0
                id = ID.CAMERA_SET_POSITION;

                index2 = line.IndexOf("(");
                index3 = line.IndexOf(")", index2);
                str = line.Substring(index2 + 1, index3 - index2 - 1); // getting coordinate string
                index2 = str.IndexOf(",");
                if (index2 == -1) {
                    // no comma, so coordinates invalid.  But could this be the name of a GameObject?  possible implementation?
                } else {
                    float1 = float.Parse(str.Substring(0, index2).Trim()); // x coordinate
                    float2 = float.Parse(str.Substring(index2 + 1).Trim()); // y coordinate
                }
                
                index2 = line.LastIndexOf("block");
                index3 = line.Length; // index3 will be the "right" of the line, where the duration will attempt to be parsed
                if (index2 != -1) {
                    index3 = index2;
                    blockToggle = true;
                }

                index2 = line.IndexOf(")") + 1;
                str = line.Substring(index2, index3 - index2).Trim();
                if (str != "") {
                    float0 = float.Parse(str);
                }
                if (float0 == 0 || float.IsNaN(float0)) {
                    float0 = 0;
                    blockToggle = false;
                }
            } else if (word == "camcustom") { // camCustom
                id = ID.CAMERA_CUSTOM;
            } else if (word == "camenablebounds") { // camEnableBounds
                id = ID.CAMERA_ENABLE_BOUNDS;
            } else if (word == "camdisablebounds") { // camDisableBounds
                id = ID.CAMERA_DISABLE_BOUNDS;
            } else if (word.IndexOf("lbl") == 0) { // lbl: 1 or lbl 1
                id = ID.LABEL;
                index2 = line.IndexOf(":");
                if (index2 == -1) {
                    str0 = line.Substring(4).Trim();
                } else {
                    str0 = line.Substring(index2 + 1).Trim();
                }
            } else if (word.IndexOf("goto") == 0) { // goto 1
                id = ID.GOTO;
                index2 = line.IndexOf(":");
                str0 = line.Substring(5).Trim();
            } else if (word.IndexOf("infohappen") == 0) { // infoHappen(4) where 4 is (int) of some AdventureEvent.Info
                id = ID.INFO_HAPPEN;
                index2 = line.IndexOf("(") + 1;
                index3 = line.IndexOf(")");
                int0 = int.Parse(line.Substring(index2, index3 - index2).Trim());
            } else if (word.IndexOf("physhappen") == 0) { // physHappen(4) where 4 is (int) of some AdventureEvent.Physical
                id = ID.PHYS_HAPPEN;
                index2 = line.IndexOf("(") + 1;
                index3 = line.IndexOf(")");
                int0 = int.Parse(line.Substring(index2, index3 - index2).Trim());
            } else if (word.IndexOf("jmpinfo") == 0) { // jmpInfo(4) 3.  if (AdventureEvent.Info)4 has happened, then jumps to lbl 3
                id = ID.JUMP_INFO;
                index2 = line.IndexOf("(") + 1;
                index3 = line.IndexOf(")");
                int0 = int.Parse(line.Substring(index2, index3 - index2).Trim());
                str0 = line.Substring(index3 + 1).Trim();
            } else if (word.IndexOf("jmpphys") == 0) { // jmpPhys(4) 3.  if (AdventureEvent.Physical)4 has happened, then jumps to lbl 3
                id = ID.JUMP_PHYS;
                index2 = line.IndexOf("(") + 1;
                index3 = line.IndexOf(")");
                int0 = int.Parse(line.Substring(index2, index3 - index2).Trim());
                str0 = line.Substring(index3 + 1).Trim();
            } else if (word == "sendmessage") { // sendMessage thingObject Action stringParam (stringParam is optional)
                id = ID.SEND_MESSAGE;
                index2 = line.IndexOf(' ', index + 1);
                name = line.Substring(index + 1, index2 - index - 1).Trim();
                index3 = line.IndexOf(' ', index2 + 1);
                if (index3 == -1) {
                    // stringParam was not given
                    str0 = line.Substring(index2 + 1).Trim();
                    text = "";
                } else {
                    // stringParam is given
                    str0 = line.Substring(index2 + 1, index3 - index2 - 1).Trim();
                    text = line.Substring(index3 + 1).Trim();
                }
            } else if (word == "spawncontrolmessage") { // spawnControlMessage 1
                id = ID.SPAWN_CONTROL_MESSAGE;
                int0 = int.Parse(line.Substring(20));
            } else if (word == "takedowncontrolmessage") { // takeDownControlMessage 1
                id = ID.TAKE_DOWN_CONTROL_MESSAGE;
                int0 = int.Parse(line.Substring(23));
            } else if (word == "keydown") { // keyDown right
                id = ID.KEY_DOWN;
                str0 = line.Substring(8).ToLower();
            } else if (word == "keyup") { // keyUp right
                id = ID.KEY_UP;
                str0 = line.Substring(6).ToLower();
            } else if (word == "playermovex") { // playerMoveX 15 block (block is optional)
                id = ID.PLAYER_MOVE_X;
                index2 = line.LastIndexOf("block");
                if (index == -1) {
                    index3 = line.Length;
                } else {
                    blockToggle = true;
                    index3 = index2;
                }
                float0 = float.Parse(line.Substring(12, index3 - 12));
            } else if (word == "playerflippedhoriz") { // playerFlippedHoriz true
                id = ID.PLAYER_FLIPPED_HORIZ;
                if (line.IndexOf("true") != -1)
                    int0 = 1;
                else
                    int0 = 0;
            } else if (word == "debuglog" || word == "debug.log") { // DebugLog any text
                id = ID.DEBUGLOG;
                str0 = line.Substring(word.Length).Trim();
            } else if (word == "destroybosshealthbar") { // destroyBossHealthBar
                id = ID.DESTROY_BOSS_HEALTH_BAR;
            } else if (word == "setcurrentobjective") { // setCurrentObjective co_theObjective
                id = ID.SET_CURRENT_OBJECTIVE;
                str0 = line.Substring(word.Length).Trim();
            } else if (word == "title" || word == "title:") { // title
                id = ID.TITLE;
            } else if (word == "freezecountdowntimer" || word == "freezetimer") {
                id = ID.FREEZE_COUNTDOWN_TIMER;
            } else if (word == "unfreezecountdowntimer" || word == "unfreezetimer") {
                id = ID.UNFREEZE_COUNTDOWN_TIMER;
            } else if (word == "playmusic") {
                id = ID.PLAY_MUSIC;
                index2 = line.IndexOf(' ', index + 1);
                if (index2 == -1) { // no fadeDuration argument
                    str0 = line.Substring(index + 1).Trim();
                    float0 = -1;
                } else {
                    str0 = line.Substring(index + 1, index2 - index - 1).Trim();
                    float0 = float.Parse(line.Substring(index2 + 1).Trim());
                }
            } else if (word == "stopmusic") {
                id = ID.STOP_MUSIC;
            } else if (word == "fadeoutmusic") {
                id = ID.FADE_OUT_MUSIC;
                index2 = line.IndexOf(' ', index + 1);
                if (line.Length <= 13) {
                    float0 = -1;
                } else {
                    float0 = float.Parse(line.Substring(13).Trim());
                }
            } else if (word == "levelpreventmusic") {
                id = ID.LEVEL_PREVENT_MUSIC;
            }

        }

        public static Instruction createInstruction() {
            Instruction ret;
            if (recycledInstructions.Count > 0) {
                ret = recycledInstructions[recycledInstructions.Count - 1];
                recycledInstructions.RemoveAt(recycledInstructions.Count - 1);
            } else {
                ret = new Instruction();
            }
            ret.clear();
            return ret;
        }
        public static void destroyInstruction(Instruction instruction) {
            recycledInstructions.Add(instruction);
        }
        private static List<Instruction> recycledInstructions = new List<Instruction>();

    }

    



    // STATIC

    public static List<ScriptRunner> scriptRunners = new List<ScriptRunner>();
    public static bool scriptsPreventPausing {
        get {
            foreach (ScriptRunner sr in scriptRunners) {
                if (sr.preventsPausing)
                    return true;
            }
            return false;
        }
    }

    // PROPERTIES

    public TextAsset scriptAsset = null;
    public bool runWhenPaused = false; // true means it can only run while paused
    public bool preventsPausing {
        get {
            if (!runningScript)
                return false;
            return true;
        }
    }
    public int runIndex {  get { return _runIndex; } }
    public bool runningScript {  get { return _runningScript; } }
    public bool makeInstantKeysFalseWhenPlayerDoesNotReceiveInput = true;

    // FUNCTIONS

    public void runScript() {
        runScript(scriptAsset);
    }
    public void runScript(TextAsset scriptAsset) {
        runScript(scriptAsset.text);
    }
    public void runScript(string script) {
        if (runningScript)
            return;
        char[] nlChars = {'\n'};
        parse(script.Split(nlChars)); // creates List of Instructions
        _runningScript = true;
        blocking = false;
        _runIndex = 0;
        waitTime = 0;
        waitDuration = 0;
    }
    public void stopScript() { // largely untested
        if (!runningScript) return;
        _runningScript = false;
    }

    // PRIVATE

	protected override void Awake() {
        timeUser = GetComponent<TimeUser>();

        scriptRunners.Add(this);

        ScriptRunner[] srs = GetComponents<ScriptRunner>();
        for (int i=0; i<srs.Length; i++) {
            if (srs[i] == this)
                idPrefix = "" + i;
        }
        
        base.Awake();
	}
	
	protected override void Update() {

        base.Update();
        
        if (!runningScript) return;
        if (runWhenPaused && !PauseScreen.paused) {
            Debug.LogError("ERROR: runWhenPaused is true, so it can only be run while paused.");
            return;
        } else if (!runWhenPaused) {
            if (timeUser.shouldNotUpdate)
                return;
        }
        // don't run if game over
        if (GameOverScreen.instance != null && GameOverScreen.instance.activated)
            return;

        if (makeInstantKeysFalseWhenPlayerDoesNotReceiveInput &&
            Player.instance != null && !Player.instance.receivePlayerInput) {
            CutsceneKeys.instantKeysFalse();
        }



        Instruction instr;
        
        while (runIndex < instructions.Count) {
            instr = instructions[runIndex];

            switch (instr.id) {
            case Instruction.ID.NONE:
                break;
            case Instruction.ID.STOP_PLAYER:
                if (Player.instance != null) {
                    Player.instance.receivePlayerInput = false;
                    CutsceneKeys.allFalse();
                }
                break;
            case Instruction.ID.TALK:
                if (blocking) {
                    if (waitTime == -1) {
                        // currently waiting for TextBox to open first
                        if (TextBox.instance.isOpen) {
                            waitTime = 0; // begin displaying text
                            TextBox.instance.displayText(instr.name, instr.text);

                            if (instr.str0 != "") {
                                // display profile (not implemented yet)
                                string profile = instr.name.ToLower() + "_" + instr.str0;
                                Debug.Log("Display profile " + profile);
                            }

                        }
                    } else {

                        // display text here
                        if (TextBox.instance.doneDisplaying) {
                            // done displaying.  Now just wait for player to input command to go to the next instruction
                            if (Keys.instance.confirmPressed) {
                                // talk instruction is over; set blocking to false and move to next instruction
                                blocking = false;
                            }
                        }

                    }

                } else {
                    /* bring up text box if needed and set text */
                    if (TextBox.instance == null) {
                        Debug.LogError("Error: TextBox has not been created yet");
                        blocking = false;
                    } else {
                        // open up TextBox, then display text
                        waitTime = -1;
                        TextBox.instance.open(true); // if TextBox is already open, this does nothing.
                        blocking = true;
                    }
                }
                break;
            case Instruction.ID.CLOSE_TEXT:
                if (TextBox.instance != null) {
                    if (blocking) {
                        if (TextBox.instance.isClosed) {
                            blocking = false;
                        }
                    } else {
                        TextBox.instance.close();
                        if (instr.blockToggle) {
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.WAIT:
                if (blocking) {
                    if (runWhenPaused) {
                        waitTime += Time.unscaledDeltaTime;
                    } else {
                        waitTime += Time.deltaTime;
                    }
                    if (waitTime >= waitDuration) {
                        // wait is over
                        blocking = false;
                    }
                } else {
                    // begin waiting
                    waitTime = 0;
                    waitDuration = instr.float0;
                    blocking = true;
                }
                break;
            case Instruction.ID.RESUME_PLAYER:
                if (Player.instance != null) {
                    Player.instance.receivePlayerInput = true;
                }
                break;
            case Instruction.ID.CUTSCENE_BARS_ON:
                if (CutsceneBars.instance != null) {
                    if (blocking) {
                        if (CutsceneBars.instance.areOn) {
                            blocking = false;
                        }
                    } else {
                        if (instr.int0 == 1) {
                            CutsceneBars.instance.moveOnImmediately();
                        } else {
                            CutsceneBars.instance.moveOn();
                        }
                        if (instr.blockToggle) {
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.CUTSCENE_BARS_OFF:
                if (CutsceneBars.instance != null) {
                    if (blocking) {
                        if (CutsceneBars.instance.areOff) {
                            blocking = false;
                        }
                    } else {
                        if (instr.int0 == 1) {
                            CutsceneBars.instance.moveOffImmediately();
                        } else {
                            CutsceneBars.instance.moveOff();
                        }
                        if (instr.blockToggle) {
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.CAMERA_FOLLOW_PLAYER:
                if (CameraControl.instance != null) {
                    if (blocking) {
                        if (runWhenPaused) {
                            waitTime += Time.unscaledDeltaTime;
                        } else {
                            waitTime += Time.deltaTime;
                        }
                        if (waitTime >= waitDuration) {
                            // wait is over
                            blocking = false;
                        }
                    } else {
                        // switch camera mode
                        CameraControl.instance.followPlayer(instr.float0);
                        if (instr.blockToggle) {
                            waitTime = 0;
                            waitDuration = instr.float0;
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.CAMERA_SET_POSITION:
                if (CameraControl.instance != null) {
                    if (blocking) {
                        if (runWhenPaused) {
                            waitTime += Time.unscaledDeltaTime;
                        } else {
                            waitTime += Time.deltaTime;
                        }
                        if (waitTime >= waitDuration) {
                            // wait is over
                            blocking = false;
                        }
                    } else {
                        // switch camera mode
                        CameraControl.instance.moveToPosition(new Vector2(instr.float1, instr.float2), instr.float0);
                        if (instr.blockToggle) {
                            waitTime = 0;
                            waitDuration = instr.float0;
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.CAMERA_CUSTOM:
                if (CameraControl.instance != null) {
                    CameraControl.instance.customPositionMode();
                }
                break;
            case Instruction.ID.CAMERA_ENABLE_BOUNDS:
                if (CameraControl.instance != null) {
                    CameraControl.instance.enableBounds();
                }
                break;
            case Instruction.ID.CAMERA_DISABLE_BOUNDS:
                if (CameraControl.instance != null) {
                    CameraControl.instance.disableBounds();
                }
                break;
            case Instruction.ID.LABEL:
                break;
            case Instruction.ID.GOTO:
                for (int i = 0; i < instructions.Count; i++) {
                    if (instructions[i].id == Instruction.ID.LABEL) {
                        if (instructions[i].str0 == instr.str0) {
                            _runIndex = i - 1; // because will be incremented at end of the loop
                            break;
                        }
                    }
                }
                break;
            case Instruction.ID.INFO_HAPPEN:
                this.infoHappen((AdventureEvent.Info)instr.int0);
                break;
            case Instruction.ID.PHYS_HAPPEN:
                this.physicalHappen((AdventureEvent.Physical)instr.int0);
                break;
            case Instruction.ID.JUMP_INFO:
                if (Vars.eventHappened((AdventureEvent.Info)instr.int0)) {
                    for (int i = 0; i < instructions.Count; i++) {
                        if (instructions[i].id == Instruction.ID.LABEL) {
                            if (instructions[i].str0 == instr.str0) {
                                _runIndex = i - 1; // because will be incremented at end of the loop
                                break;
                            }
                        }
                    }
                }
                break;
            case Instruction.ID.JUMP_PHYS:
                if (Vars.currentNodeData != null && Vars.currentNodeData.eventHappened((AdventureEvent.Physical)instr.int0)) {
                    for (int i = 0; i < instructions.Count; i++) {
                        if (instructions[i].id == Instruction.ID.LABEL) {
                            if (instructions[i].str0 == instr.str0) {
                                _runIndex = i - 1; // because will be incremented at end of the loop
                                break;
                            }
                        }
                    }
                }
                break;
            case Instruction.ID.SEND_MESSAGE:
                GameObject GO = GameObject.Find(instr.name);
                if (GO != null) {
                    if (instr.text == "") {
                        GO.SendMessage(instr.str0, SendMessageOptions.DontRequireReceiver);
                    } else {
                        GO.SendMessage(instr.str0, instr.text, SendMessageOptions.DontRequireReceiver);
                    }
                }
                break;
            case Instruction.ID.SPAWN_CONTROL_MESSAGE:
                ControlsMessageSpawner.instance.spawnMessage((ControlsMessage.Control)instr.int0);
                break;
            case Instruction.ID.TAKE_DOWN_CONTROL_MESSAGE:
                ControlsMessageSpawner.instance.takeDownMessage((ControlsMessage.Control)instr.int0);
                break;
            case Instruction.ID.KEY_DOWN:
                if (instr.str0 == "left") {
                    CutsceneKeys.leftPressed = true;
                    CutsceneKeys.leftHeld = true;
                } else if (instr.str0 == "right") {
                    CutsceneKeys.rightPressed = true;
                    CutsceneKeys.rightHeld = true;
                } else if (instr.str0 == "up") {
                    CutsceneKeys.upPressed = true;
                    CutsceneKeys.upHeld = true;
                } else if (instr.str0 == "down") {
                    CutsceneKeys.downPressed = true;
                    CutsceneKeys.downHeld = true;
                } else if (instr.str0 == "jump") {
                    CutsceneKeys.jumpPressed = true;
                    CutsceneKeys.jumpHeld = true;
                } else if (instr.str0 == "shoot") {
                    CutsceneKeys.shootPressed = true;
                    CutsceneKeys.shootHeld = true;
                } else if (instr.str0 == "bomb") {
                    CutsceneKeys.bombPressed = true;
                    CutsceneKeys.bombHeld = true;
                } else if (instr.str0 == "dodge") {
                    CutsceneKeys.dodgePressed = true;
                    CutsceneKeys.dodgeHeld = true;
                }
                break;
            case Instruction.ID.KEY_UP:
                if (instr.str0 == "left") {
                    CutsceneKeys.leftReleased = true;
                    CutsceneKeys.leftHeld = false;
                } else if (instr.str0 == "right") {
                    CutsceneKeys.rightReleased = true;
                    CutsceneKeys.rightHeld = false;
                } else if (instr.str0 == "up") {
                    CutsceneKeys.upReleased = true;
                    CutsceneKeys.upHeld = false;
                } else if (instr.str0 == "down") {
                    CutsceneKeys.downReleased = true;
                    CutsceneKeys.downHeld = false;
                } else if (instr.str0 == "jump") {
                    CutsceneKeys.jumpReleased = true;
                    CutsceneKeys.jumpHeld = false;
                } else if (instr.str0 == "shoot") {
                    CutsceneKeys.shootReleased = true;
                    CutsceneKeys.shootHeld = false;
                } else if (instr.str0 == "bomb") {
                    CutsceneKeys.bombReleased = true;
                    CutsceneKeys.bombHeld = false;
                } else if (instr.str0 == "dodge") {
                    CutsceneKeys.dodgeReleased = true;
                    CutsceneKeys.dodgeHeld = false;
                }
                break;
            case Instruction.ID.PLAYER_MOVE_X:
                if (Player.instance != null) {
                    if (blocking) {
                        // wait for player to get to x position
                        if (Player.instance.movedToX()) {
                            blocking = false;
                        }
                    } else {
                        // move player to x position
                        Player.instance.moveToX(instr.float0);
                        if (instr.blockToggle) {
                            blocking = true;
                        }
                    }
                }
                break;
            case Instruction.ID.PLAYER_FLIPPED_HORIZ:
                if (Player.instance != null) {
                    Player.instance.flippedHoriz = (instr.int0 == 1);
                }
                break;
            case Instruction.ID.DEBUGLOG:
                Debug.Log(instr.str0);
                break;
            case Instruction.ID.DESTROY_BOSS_HEALTH_BAR:
                if (HUD.instance != null && HUD.instance.bossHealthBar != null) {
                    HUD.instance.bossHealthBar.destroy();
                }
                break;
            case Instruction.ID.SET_CURRENT_OBJECTIVE:
                TalkPage.setCurrentObjectiveFile(instr.str0);
                break;
            case Instruction.ID.TITLE:
                // doesn't do anything
                break;
            case Instruction.ID.FREEZE_COUNTDOWN_TIMER:
                if (CountdownTimer.instance != null)
                    CountdownTimer.instance.frozen = true;
                break;
            case Instruction.ID.UNFREEZE_COUNTDOWN_TIMER:
                if (CountdownTimer.instance != null)
                    CountdownTimer.instance.frozen = false;
                break;
            case Instruction.ID.PLAY_MUSIC:
                if (SoundManager.instance != null) {
                    if (instr.float0 == -1) {
                        SoundManager.instance.playMusic(instr.str0);
                    } else {
                        SoundManager.instance.playMusic(instr.str0, instr.float0);
                    }
                }
                break;
            case Instruction.ID.STOP_MUSIC:
                if (SoundManager.instance != null) {
                    SoundManager.instance.stopMusic();
                }
                break;
            case Instruction.ID.FADE_OUT_MUSIC:
                if (SoundManager.instance != null) {
                    if (instr.float0 == -1) {
                        SoundManager.instance.fadeOutMusic();
                    } else {
                        SoundManager.instance.fadeOutMusic(instr.float0);
                    }
                }
                break;
            case Instruction.ID.LEVEL_PREVENT_MUSIC:
                Level.doNotStartBGMusic = true;
                break;
            }

            if (blocking) {
                break;
            }
            _runIndex++;
            if (runIndex >= instructions.Count) {
                // completed running script
                _runningScript = false;
            }
        }

        if (runIndex >= instructions.Count) {
            _runningScript = false;
        }
        
	}

    protected override void OnSaveFrame(FrameInfo fi) {

        base.OnSaveFrame(fi);

        if (runWhenPaused) return;
        
        // don't run if game over
        if (GameOverScreen.instance != null && GameOverScreen.instance.activated)
            return;

        fi.bools[idPrefix + "rs"] = runningScript;
        fi.ints[idPrefix + "ri"] = runIndex;
        fi.floats[idPrefix + "wt"] = waitTime;
        fi.floats[idPrefix + "wd"] = waitDuration;
        fi.bools[idPrefix + "blocking"] = blocking;
    }

    protected override void OnRevert(FrameInfo fi) {

        base.OnSaveFrame(fi);

        if (runWhenPaused) return;

        // don't run if game over
        if (GameOverScreen.instance != null && GameOverScreen.instance.activated)
            return;

        _runningScript = fi.bools[idPrefix + "rs"];
        _runIndex = fi.ints[idPrefix + "ri"];
        waitTime = fi.floats[idPrefix + "wt"];
        waitDuration = fi.floats[idPrefix + "wd"];
        blocking = fi.bools[idPrefix + "blocking"];
    }

    void OnDestroy() {
        scriptRunners.Remove(this);
    }

    void parse(string[] lines) {
        instructions.Clear();
        for (int i=0; i<lines.Length; i++) {
            string line = lines[i];
            if (line == "") continue;
            Instruction instr = Instruction.createInstruction();
            instr.parse(line);
            if (instr.id != Instruction.ID.NONE) {
                instructions.Add(instr);
            }
        }
    }

    bool _runningScript = false;
    int _runIndex = 0;
    float waitTime = 0;
    float waitDuration = 0;
    bool blocking = false;

    string idPrefix = "0";
    List<Instruction> instructions = new List<Instruction>();
}
