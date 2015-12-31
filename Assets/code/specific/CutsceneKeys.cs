using UnityEngine;
using System.Collections;

/* Will help control Player movement during a cutscene, e.g. talking.
 * These values can be changed by anything.
 * During normal gameplay, call updateFromKeys() at the beginning of Player() to have controls play like normal.
 * Goal: try to make the flashback button always available, so a cutscene can play in reverse. */

public class CutsceneKeys {

    public static bool leftPressed = false;
    public static bool leftHeld = false;
    public static bool leftReleased = false;
    public static bool rightPressed = false;
    public static bool rightHeld = false;
    public static bool rightReleased = false;
    public static bool upPressed = false;
    public static bool upHeld = false;
    public static bool upReleased = false;
    public static bool downPressed = false;
    public static bool downHeld = false;
    public static bool downReleased = false;
    public static bool jumpPressed = false;
    public static bool jumpHeld = false;
    public static bool jumpReleased = false;
    public static bool shootPressed = false;
    public static bool shootHeld = false;
    public static bool shootReleased = false;
    public static bool bombPressed = false;
    public static bool bombHeld = false;
    public static bool bombReleased = false;
    public static bool dodgePressed = false;
    public static bool dodgeHeld = false;
    public static bool dodgeReleased = false;

    public static void updateFromKeys() {
        if (Keys.instance == null) return;

        Keys instance = Keys.instance;

        leftPressed = instance.leftPressed;
        leftHeld = instance.leftHeld;
        leftReleased = instance.leftReleased;
        rightPressed = instance.rightPressed;
        rightHeld = instance.rightHeld;
        rightReleased = instance.rightReleased;
        upPressed = instance.upPressed;
        upHeld = instance.upHeld;
        upReleased = instance.upReleased;
        downPressed = instance.downPressed;
        downHeld = instance.downHeld;
        downReleased = instance.downReleased;
        jumpPressed = instance.jumpPressed;
        jumpHeld = instance.jumpHeld;
        jumpReleased = instance.jumpReleased;
        shootPressed = instance.shootPressed;
        shootHeld = instance.shootHeld;
        shootReleased = instance.shootReleased;
        bombPressed = instance.bombPressed;
        bombHeld = instance.bombHeld;
        bombReleased = instance.bombReleased;
        dodgePressed = instance.dodgePressed;
        dodgeHeld = instance.dodgeHeld;
        dodgeReleased = instance.dodgeReleased;
    }

    public static void allFalse() {
        leftPressed = false;
        leftHeld = false;
        leftReleased = false;
        rightPressed = false;
        rightHeld = false;
        rightReleased = false;
        upPressed = false;
        upHeld = false;
        upReleased = false;
        downPressed = false;
        downHeld = false;
        downReleased = false;
        jumpPressed = false;
        jumpHeld = false;
        jumpReleased = false;
        shootPressed = false;
        shootHeld = false;
        shootReleased = false;
        bombPressed = false;
        bombHeld = false;
        bombReleased = false;
        dodgePressed = false;
        dodgeHeld = false;
        dodgeReleased = false;
    }

}
