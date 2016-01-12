using UnityEngine;
using System.Collections;

[RequireComponent(typeof(TimeUser))]
[RequireComponent(typeof(ScriptRunner))]
public class ScriptSensor : MonoBehaviour {

    public AdventureEvent.Info ignoreIfInfoEventHappened = AdventureEvent.Info.NONE;
    public AdventureEvent.Physical ignoreIfPhysicalEventHappened = AdventureEvent.Physical.NONE;
    public AdventureEvent.Info infoEventHappenOnRunningScript = AdventureEvent.Info.NONE;
    public AdventureEvent.Physical physicalEventHappenOnRunningScript = AdventureEvent.Physical.NONE;

    void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
        timeUser = GetComponent<TimeUser>();
        scriptRunner = GetComponent<ScriptRunner>();
    }

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        if (Player.instance == null) return;
        Vector3 plrPos = Player.instance.rb2d.position;

        if (bc2d.bounds.Contains(plrPos)) {
            if (!playerContained) {
                runScript();
                playerContained = true;
            }
        } else {
            playerContained = false;
        }

    }

    void LateUpdate() {

        if (TimeUser.reverting ||
            Time.timeScale < .0001f ||
            HUD.instance != null && HUD.instance.gameOverScreen != null && HUD.instance.gameOverScreen.activated)
            return;

    }

    void runScript() {
        if (scriptRunner.scriptAsset == null)
            return;
        if (ignoreIfInfoEventHappened != AdventureEvent.Info.NONE &&
            Vars.eventHappened(ignoreIfInfoEventHappened))
            return;
        if (ignoreIfPhysicalEventHappened != AdventureEvent.Physical.NONE &&
            Vars.currentNodeData != null &&
            Vars.currentNodeData.eventHappened(ignoreIfPhysicalEventHappened))
            return;

        scriptRunner.runScript();

        if (infoEventHappenOnRunningScript != AdventureEvent.Info.NONE) {
            scriptRunner.infoHappen(infoEventHappenOnRunningScript);
        }
        if (physicalEventHappenOnRunningScript != AdventureEvent.Physical.NONE) {
            scriptRunner.physicalHappen(physicalEventHappenOnRunningScript);
        }

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.bools["pc"] = playerContained;
    }

    void OnRevert(FrameInfo fi) {
        bool prevPlayerContained = playerContained;
        playerContained = fi.bools["pc"];
        if (playerContained && !prevPlayerContained) {
            runScript();
        }
    }

    TimeUser timeUser;
    BoxCollider2D bc2d;
    ScriptRunner scriptRunner;
    bool playerContained = false;
}
