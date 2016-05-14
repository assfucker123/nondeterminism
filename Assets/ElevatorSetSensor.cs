using UnityEngine;
using System.Collections;

public class ElevatorSetSensor : MonoBehaviour {

    public string elevatorName = ""; // leave blank to select first elevator in the room
    public int pathIndex = 0; 

    void Awake() {
        bc2d = GetComponent<BoxCollider2D>();
        // get elevator
        Elevator[] elevs = GameObject.FindObjectsOfType<Elevator>();
        foreach (Elevator elev in elevs) {
            if (elevatorName == "" || elev.gameObject.name == elevatorName) {
                elevator = elev;
                break;
            }
        }
        if (elevator == null) {
            Debug.LogWarning("WARNING: No elevator attached to ElevatorSetSensor");
        }
    }

    void Update() {

        if (elevator == null) return;
        if (Player.instance == null) return;
        Vector3 plrPos = Player.instance.rb2d.position;

        if (bc2d.bounds.Contains(plrPos)) {
            elevator.setPathIndexImmediately(pathIndex);
        }

    }
    

    BoxCollider2D bc2d;
    Elevator elevator;

}
