using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptRunner : MonoBehaviour {

    public class Instruction {

        public enum ID {
            NONE,
            STOP_PLAYER,
            TALK,
            CLOSE_TEXT,
            WAIT,
            RESUME_PLAYER,
            CUTSCENE_BARS_ON,
            CUTSCENE_BARS_OFF,
        }

        public ID id = ID.NONE;

        public string text = "";
        public string name = "";
        public int int0 = 0;
        public int int1 = 0;
        public float float0 = 0;
        public string str0 = "";
        public bool blockToggle = false;

        public void clear() {
            id = ID.NONE;
            text = "";
            name = "";
            int0 = 0;
            int1 = 0;
            float0 = 0;
            str0 = "";
            blockToggle = false;
        }

        public void parse(string line) {
            // trim comment
            int index = line.IndexOf("//");
            int index2 = 0;
            int index3 = 0;
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
            } else if (word == "cbarsoff") { // cbarsOff
                id = ID.CUTSCENE_BARS_OFF;
                index2 = line.LastIndexOf("block");
                if (index2 != -1)
                    blockToggle = true;
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
        runningScript = true;
        blocking = false;
        _runIndex = 0;
        waitTime = 0;
        waitDuration = 0;
    }

    // PRIVATE

	void Awake() {
        timeUser = GetComponent<TimeUser>();

        scriptRunners.Add(this);
	}
	
	void Update() {

        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            runScript();
        }

        if (!runningScript) return;
        if (runWhenPaused && !PauseScreen.paused) {
            Debug.LogError("ERROR: runWhenPaused is true, so it can only be run while paused.");
            return;
        } else if (!runWhenPaused) {
            if (timeUser.shouldNotUpdate)
                return;
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
                        CutsceneBars.instance.moveOn();
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
                        CutsceneBars.instance.moveOff();
                        if (instr.blockToggle) {
                            blocking = true;
                        }
                    }
                }
                break;
            }

            if (blocking) {
                break;
            }
            _runIndex++;
            if (runIndex >= instructions.Count) {
                // completed running script
                runningScript = false;
            }
        }
        
	}

    void OnSaveFrame(FrameInfo fi) {
        if (runWhenPaused) return;
        fi.bools["rs"] = runningScript;
        fi.ints["ri"] = runIndex;
        fi.floats["wt"] = waitTime;
        fi.floats["wd"] = waitDuration;
        fi.bools["b"] = blocking;
    }

    void OnRevert(FrameInfo fi) {
        if (runWhenPaused) return;
        runningScript = fi.bools["rs"];
        _runIndex = fi.ints["ri"];
        waitTime = fi.floats["wt"];
        waitDuration = fi.floats["wd"];
        blocking = fi.bools["b"];
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

    TimeUser timeUser;

    bool runningScript = false;
    int _runIndex = 0;
    float waitTime = 0;
    float waitDuration = 0;
    bool blocking = false;

    List<Instruction> instructions = new List<Instruction>();
}
