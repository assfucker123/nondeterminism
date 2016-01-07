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

    public static int saveToInt() {
        int ret = 0;
        if (leftPressed) ret |= 1;
        if (leftHeld) ret |= 1 << 1;
        if (leftReleased) ret |= 1 << 2;
        if (rightPressed) ret |= 1 << 3;
        if (rightHeld) ret |= 1 << 4;
        if (rightReleased) ret |= 1 << 5;
        if (upPressed) ret |= 1 << 6;
        if (upHeld) ret |= 1 << 7;
        if (upReleased) ret |= 1 << 8;
        if (downPressed) ret |= 1 << 9;
        if (downHeld) ret |= 1 << 10;
        if (downReleased) ret |= 1 << 11;
        if (jumpPressed) ret |= 1 << 12;
        if (jumpHeld) ret |= 1 << 13;
        if (jumpReleased) ret |= 1 << 14;
        if (shootPressed) ret |= 1 << 15;
        if (shootHeld) ret |= 1 << 16;
        if (shootReleased) ret |= 1 << 17;
        if (bombPressed) ret |= 1 << 18;
        if (bombHeld) ret |= 1 << 1 << 19;
        if (bombReleased) ret |= 1 << 20;
        if (dodgePressed) ret |= 1 << 21;
        if (dodgeHeld) ret |= 1 << 22;
        if (dodgeReleased) ret |= 1 << 23;
        return ret;
    }

    public static void loadFromInt(int i) {
        leftPressed = (i & 1) != 0;
        leftHeld = (i & 1 << 1) != 0;
        leftReleased = (i & 1 << 2) != 0;
        rightPressed = (i & 1 << 3) != 0;
        rightHeld = (i & 1 << 4) != 0;
        rightReleased = (i & 1 << 5) != 0;
        upPressed = (i & 1 << 6) != 0;
        upHeld = (i & 1 << 7) != 0;
        upReleased = (i & 1 << 8) != 0;
        downPressed = (i & 1 << 9) != 0;
        downHeld = (i & 1 << 10) != 0;
        downReleased = (i & 1 << 11) != 0;
        jumpPressed = (i & 1 << 12) != 0;
        jumpHeld = (i & 1 << 13) != 0;
        jumpReleased = (i & 1 << 14) != 0;
        shootPressed = (i & 1 << 15) != 0;
        shootHeld = (i & 1 << 16) != 0;
        shootReleased = (i & 1 << 17) != 0;
        bombPressed = (i & 1 << 18) != 0;
        bombHeld = (i & 1 << 19) != 0;
        bombReleased = (i & 1 << 20) != 0;
        dodgePressed = (i & 1 << 21) != 0;
        dodgeHeld = (i & 1 << 22) != 0;
        dodgeReleased = (i & 1 << 23) != 0;
    }

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

    /* Makes all the bools that should only happen for 1 frame false */
    public static void instantKeysFalse() {
        leftPressed = false;
        leftReleased = false;
        rightPressed = false;
        rightReleased = false;
        upPressed = false;
        upReleased = false;
        downPressed = false;
        downReleased = false;
        jumpPressed = false;
        jumpReleased = false;
        shootPressed = false;
        shootReleased = false;
        bombPressed = false;
        bombReleased = false;
        dodgePressed = false;
        dodgeReleased = false;
    }

}
