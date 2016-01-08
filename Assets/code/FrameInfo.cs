using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrameInfo {

    // Will be set automatically in TimeUser:
    public float time = 0;
    public bool exists = true;
    public Vector2 position = new Vector2();
    public float rotation = 0;
    public Vector2 velocity = new Vector2();
    public float angularVelocity = 0;
    public float spriteRendererLocalScaleX = 1;
    public float spriteRendererLocalScaleY = 1;
    public float spriteRendererLocalRotation = 0;
    public int animatorFullPathHash = 0;
    public float animatorNormalizedTime = 0;
    public int randSeed = 0;
    // Will need to be set manually:
    public int state = 0;
    public Dictionary<string, int> ints = new Dictionary<string, int>();
    public Dictionary<string, float> floats = new Dictionary<string, float>();
    public Dictionary<string, bool> bools = new Dictionary<string, bool>();
    public Dictionary<string, string> strings = new Dictionary<string, string>();
    public Dictionary<string, GameObject> gameObjects = new Dictionary<string, GameObject>();

    public bool preserve = false; // if set to true, will not be deleted with MAX_TIME_DESTROY_AGE

    public bool thisFrameInfoDestroyed { get { return _thisFrameInfoDestroyed; } }
    
    public static void clear(FrameInfo fi) {
        fi.time = 0;
        fi.exists = true;
        fi.position.Set(0, 0);
        fi.rotation = 0;
        fi.velocity.Set(0, 0);
        fi.angularVelocity = 0;
        fi.spriteRendererLocalScaleX = 1;
        fi.spriteRendererLocalScaleY = 1;
        fi.spriteRendererLocalRotation = 0;
        fi.animatorFullPathHash = 0;
        fi.animatorNormalizedTime = 0;
        fi.randSeed = 0;
        fi.state = 0;
        fi.ints.Clear();
        fi.floats.Clear();
        fi.bools.Clear();
        fi.strings.Clear();
        fi.gameObjects.Clear();
        fi._thisFrameInfoDestroyed = false;
    }
    public static FrameInfo create() {
        FrameInfo fi;
        if (recycledFI.Count > 0) {
            fi = recycledFI[recycledFI.Count - 1];
            recycledFI.RemoveAt(recycledFI.Count - 1);
        } else {
            fi = new FrameInfo();
        }
        clear(fi);
        return fi;
    }
    public static void destroy(FrameInfo fi) {
        if (fi.thisFrameInfoDestroyed)
            return;
        fi._thisFrameInfoDestroyed = true;
        recycledFI.Add(fi);
    }
    private static List<FrameInfo> recycledFI = new List<FrameInfo>();
    private bool _thisFrameInfoDestroyed = false;
}

