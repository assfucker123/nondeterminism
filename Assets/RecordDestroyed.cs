using UnityEngine;
using System.Collections;

/* Saves this object to Vars.currentNodeData when it gets time destroyed.
 * Will not work automatically if disabled. */
[RequireComponent(typeof(TimeUser))]
public class RecordDestroyed : MonoBehaviour {
    
    public bool isDestroyed {  get {
            if (Vars.currentNodeData == null) return false;
            return Vars.currentNodeData.objectDestroyed(Vars.currentLevel, gameObject.name);
    } }

    public void destroyObject() {
        if (Vars.currentNodeData == null) return;
        Vars.currentNodeData.objectDestroy(Vars.currentLevel, gameObject.name);
    }
    public void destroyObjectUndo() {
        if (Vars.currentNodeData == null) return;
        Vars.currentNodeData.objectDestroyUndo(Vars.currentLevel, gameObject.name);
    }

    void Awake() {
        if (enabled && isDestroyed) {
            GameObject.Destroy(gameObject);
        }
    }

    void OnRevertExist() {
        if (enabled)
            destroyObjectUndo();
    }

    void OnTimeDestroy() {
        if (enabled)
            destroyObject();
    }

}
