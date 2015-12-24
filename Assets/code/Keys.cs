using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Keys : MonoBehaviour {

    public static Keys instance { get { return _instance; } }

    /* These must be the name of axises in the Input Manager.
     * In the Input Manager, make all these axises.
     * Gravity = 0, Dead = 0.19, Sensitivity = 1,
     * Axis = Name = whatever the axis is. */
    public static string[] axises = {
        "X axis",
        "Y axis",
        "3rd axis",
        "4th axis",
        "5th axis",
        "6th axis",
        "7th axis",
        "8th axis",
        "9th axis",
        "10th axis",
    };

    public static string[] buttons = {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "up", "down", "left", "right",
        "[0]", "[1]", "[2]", "[3]", "[4]", "[5]", "[6]", "[7]", "[8]", "[9]",
        "`", "-", "=", "[", "]", "\\", ";", "'", ",", ".", "/",
        "backspace", "tab", "return", "escape", "space", "delete", "enter", "insert", "home", "end", "page up", "page down", "caps lock",
        "right shift", "left shift", "right ctrl", "left ctrl", "right alt", "left alt", "right cmd", "left cmd",
        "f1", "f2", "f3", "f4", "f5", "f6", "f7", "f8", "f9", "f10", "f11", "f12", "f13", "f14", "f15",
        "joystick button 0", "joystick button 1", "joystick button 2", "joystick button 3", "joystick button 4", "joystick button 5",
        "joystick button 6", "joystick button 7", "joystick button 8", "joystick button 9", "joystick button 10", "joystick button 11",
        "joystick button 12", "joystick button 13", "joystick button 14", "joystick button 15", "joystick button 16", "joystick button 17",
        "joystick button 18", "joystick button 19"
    };

    void Awake() {
        if (instance != null) {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        // set up
        setUp();
    }

    void setUp() {

        if (setUpCalled) return;

        setActiveDevice(InControl.InputManager.ActiveDevice);
        
        // handing controllers being attached
        InControl.InputManager.OnDeviceAttached += InputManager_OnDeviceAttached;
        InControl.InputManager.OnDeviceDetached += InputManager_OnDeviceDetached;

        setUpCalled = true;
        
    }

    void setActiveDevice(InControl.InputDevice inputDevice) {
        activeDevice = inputDevice;

        startControls.Clear();
        InControl.InputControl[] controls = activeDevice.Controls;
        for (int i = 0; i < controls.Length; i++) {
            if (controls[i] != null) {
                switch (controls[i].Target) {
                case InControl.InputControlType.Back:
                case InControl.InputControlType.Start:
                case InControl.InputControlType.Select:
                case InControl.InputControlType.Pause:
                case InControl.InputControlType.Menu:
                case InControl.InputControlType.Options:
                case InControl.InputControlType.TouchPadTap:
                    startControls.Add(controls[i]);
                    break;
                }
            }
        }
    }

    // handling controllers being attached
    void InputManager_OnDeviceAttached(InControl.InputDevice inputDevice) {
        setActiveDevice(inputDevice);
    }
    void InputManager_OnDeviceDetached(InControl.InputDevice inputDevice) {
        // device detached
    }

    public static float DEAD_ZONE = 0;

    public bool leftPressed { get {
            if (Input.GetKeyDown(LEFT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadLeft.WasPressed) return true;
            if (activeDevice.LeftStickX.WasPressed && activeDevice.LeftStickX.Value < -DEAD_ZONE) return true;
            return false;
    } }
    public bool leftHeld { get {
            if (Input.GetKey(LEFT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadLeft.IsPressed) return true;
            if (activeDevice.LeftStickX.Value < -DEAD_ZONE) return true;
            return false;
    } }
    public bool leftReleased { get {
            if (Input.GetKeyUp(LEFT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadLeft.WasReleased) return true;
            if (activeDevice.LeftStickX.WasReleased) return true;
            return false;
    } }

    public bool rightPressed { get {
            if (Input.GetKeyDown(RIGHT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadRight.WasPressed) return true;
            if (activeDevice.LeftStickX.WasPressed && activeDevice.LeftStickX.Value > DEAD_ZONE) return true;
            return false;
    } }
    public bool rightHeld { get {
            if (Input.GetKey(RIGHT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadRight.IsPressed) return true;
            if (activeDevice.LeftStickX.Value > DEAD_ZONE) return true;
            return false;
    } }
    public bool rightReleased { get {
            if (Input.GetKeyUp(RIGHT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadRight.WasReleased) return true;
            if (activeDevice.LeftStickX.WasReleased) return true;
            return false;
    } }

    public bool upPressed { get {
            if (Input.GetKeyDown(UP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadUp.WasPressed) return true;
            if (activeDevice.LeftStickY.WasPressed && activeDevice.LeftStickY.Value > DEAD_ZONE) return true;
            return false;
    } }
    public bool upHeld { get {
            if (Input.GetKey(UP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadUp.IsPressed) return true;
            if (activeDevice.LeftStickY.Value > DEAD_ZONE) return true;
            return false;
    } }
    public bool upReleased { get {
            if (Input.GetKeyUp(UP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadUp.WasReleased) return true;
            if (activeDevice.LeftStickY.WasReleased) return true;
            return false;
    } }

    public bool downPressed { get {
            if (Input.GetKeyDown(DOWN_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadDown.WasPressed) return true;
            if (activeDevice.LeftStickY.WasPressed && activeDevice.LeftStickY.Value < DEAD_ZONE) return true;
            return false;
    } }
    public bool downHeld { get {
            if (Input.GetKey(DOWN_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadDown.IsPressed) return true;
            if (activeDevice.LeftStickY.Value < DEAD_ZONE) return true;
            return false;
    } }
    public bool downReleased { get {
            if (Input.GetKeyUp(DOWN_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.DPadDown.WasReleased) return true;
            if (activeDevice.LeftStickY.WasReleased) return true;
            return false;
    } }

    public bool jumpPressed { get {
            if (Input.GetKeyDown(JUMP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action1.WasPressed) return true;
            return false;
    } }
    public bool jumpHeld { get {
            if (Input.GetKey(JUMP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action1.IsPressed) return true;
            return false;
    } }
    public bool jumpReleased { get {
            if (Input.GetKeyUp(JUMP_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action1.WasReleased) return true;
            return false;
    } }

    public bool shootPressed { get {
            if (Input.GetKeyDown(SHOOT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action3.WasPressed) return true;
            return false;
    } }
    public bool shootHeld { get {
            if (Input.GetKey(SHOOT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action3.IsPressed) return true;
            return false;
    } }
    public bool shootReleased { get {
            if (Input.GetKeyUp(SHOOT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action3.WasReleased) return true;
            return false;
    } }

    public bool bombPressed { get {
            if (Input.GetKeyDown(BOMB_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action4.WasPressed) return true;
            return false;
    } }
    public bool bombHeld { get {
            if (Input.GetKey(BOMB_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action4.IsPressed) return true;
            return false;
    } }
    public bool bombReleased { get {
            if (Input.GetKeyUp(BOMB_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.Action4.WasReleased) return true;
            return false;
    } }

    public bool flashbackPressed { get {
            if (Input.GetKeyDown(FLASHBACK_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.LeftBumper.WasPressed) return true;
            if (activeDevice.LeftTrigger.WasPressed) return true;
            return false;
    } }
    public bool flashbackHeld { get {
            if (Input.GetKey(FLASHBACK_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.LeftBumper.IsPressed) return true;
            if (activeDevice.LeftTrigger.IsPressed) return true;
            return false;
    } }
    public bool flashbackReleased { get {
            if (Input.GetKeyUp(FLASHBACK_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.LeftBumper.WasReleased) return true;
            if (activeDevice.LeftTrigger.WasReleased) return true;
            return false;
    } }

    public bool dodgePressed { get {
            if (Input.GetKeyDown(DODGE_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.RightBumper.WasPressed) return true;
            if (activeDevice.RightTrigger.WasPressed) return true;
            return false;
    } }
    public bool dodgeHeld { get {
            if (Input.GetKey(DODGE_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.RightBumper.IsPressed) return true;
            if (activeDevice.RightTrigger.IsPressed) return true;
            return false;
    } }
    public bool dodgeReleased { get {
            if (Input.GetKeyUp(DODGE_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.RightBumper.WasReleased) return true;
            if (activeDevice.RightTrigger.WasReleased) return true;
            return false;
    } }

    public bool startPressed { get {
            if (Input.GetKeyDown(START_KEY)) return true;
            foreach (InControl.InputControl control in startControls) {
                if (control.WasPressed) return true;
            }
            return false;
    } }

    public bool confirmPressed { get {
            return jumpPressed;
    } }

    public bool backPressed { get {
            return shootPressed;
    } }

    public bool pageLeftPressed { get {
            if (Input.GetKeyDown(PAGE_LEFT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.LeftBumper.WasPressed) return true;
            if (activeDevice.LeftTrigger.WasPressed) return true;
            return false;
    } }

    public bool pageRightPressed { get {
            if (Input.GetKeyDown(PAGE_RIGHT_KEY)) return true;
            if (activeDevice == null) return false;
            if (activeDevice.RightBumper.WasPressed) return true;
            if (activeDevice.RightTrigger.WasPressed) return true;
            return false;
        }
    }
    
    public bool escapePressed { get {
        return Input.GetKeyDown(KeyCode.Escape);
    } }

    public static KeyCode DEFAULT_LEFT_KEY = KeyCode.LeftArrow;
    public static KeyCode DEFAULT_RIGHT_KEY = KeyCode.RightArrow;
    public static KeyCode DEFAULT_UP_KEY = KeyCode.UpArrow;
    public static KeyCode DEFAULT_DOWN_KEY = KeyCode.DownArrow;
    public static KeyCode DEFAULT_JUMP_KEY = KeyCode.Z;
    public static KeyCode DEFAULT_SHOOT_KEY = KeyCode.X;
    public static KeyCode DEFAULT_BOMB_KEY = KeyCode.C;
    public static KeyCode DEFAULT_FLASHBACK_KEY = KeyCode.LeftControl;
    public static KeyCode DEFAULT_DODGE_KEY = KeyCode.LeftShift;
    public static KeyCode DEFAULT_START_KEY = KeyCode.Return;
    public static KeyCode DEFAULT_PAGE_LEFT_KEY = KeyCode.PageDown;
    public static KeyCode DEFAULT_PAGE_RIGHT_KEY = KeyCode.PageUp;
    
    public static KeyCode LEFT_KEY = DEFAULT_LEFT_KEY;
    public static KeyCode RIGHT_KEY = DEFAULT_RIGHT_KEY;
    public static KeyCode UP_KEY = DEFAULT_UP_KEY;
    public static KeyCode DOWN_KEY = DEFAULT_DOWN_KEY;
    public static KeyCode JUMP_KEY = DEFAULT_JUMP_KEY;
    public static KeyCode SHOOT_KEY = DEFAULT_SHOOT_KEY;
    public static KeyCode BOMB_KEY = DEFAULT_BOMB_KEY;
    public static KeyCode FLASHBACK_KEY = DEFAULT_FLASHBACK_KEY;
    public static KeyCode DODGE_KEY = DEFAULT_DODGE_KEY;
    public static KeyCode START_KEY = DEFAULT_START_KEY;
    public static KeyCode PAGE_LEFT_KEY = DEFAULT_PAGE_LEFT_KEY;
    public static KeyCode PAGE_RIGHT_KEY = DEFAULT_PAGE_RIGHT_KEY;
    

    private static Keys _instance = null;
    private bool setUpCalled = false;

    InControl.InputDevice activeDevice = null;
    List<InControl.InputControl> startControls = new List<InControl.InputControl>();

    
	
}
