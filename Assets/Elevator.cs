using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(TimeUser))]
public class Elevator : MonoBehaviour {

    public float speed = 4;
    public float beginAccel = 10;
    public float endDecel = 10;
    public float bobDistance = .5f;
    public float bobPeriod = 1;
    public AudioClip moveSound;
    public AudioClip moveSoundEnd;
    
    public enum State {
        IDLE,
        BOBBING,
        MOVING
    }

    public State state { get; private set; }

    public Vector2 p0 {
        get { return pathPoints[0]; }
    }
    public Vector2 p1 {
        get { return pathPoints[1]; }
    }

    public bool playerIsOn {
        get {
            if (Player.instance == null) return false;
            if (!Player.instance.GetComponent<ColFinder>().hitBottom) return false;
            Vector2 plrPos = Player.instance.rb2d.position;
            Bounds bounds = pc2d.bounds;
            if (plrPos.y < bounds.max.y || plrPos.y > bounds.max.y + 2) return false;
            return (bounds.min.x < plrPos.x && plrPos.x < bounds.max.x);
        }
    }

    public bool playerIsReady {
        get {
            if (!playerIsOn) return false;
            Vector2 plrPos = Player.instance.rb2d.position;
            Bounds bounds = pc2d.bounds;
            if (plrPos.x < bounds.min.x + .6f) return false;
            if (plrPos.x > bounds.max.x - .6f) return false;

            return Player.instance.state == Player.State.GROUND;
        }
    }

    public void movePlatform(int pointIndex, bool playerOn) {
        // prevent player from moving if on platform
        if (playerOn) {
            Player.instance.receivePlayerInput = false;
            CutsceneKeys.allFalse();
            resumePlayerMovementOnMoveComplete = true;
        } else {
            resumePlayerMovementOnMoveComplete = false;
        }
        time = 0;
        startPos = rb2d.position;
        endIndex = pointIndex;
        state = State.MOVING;
        SoundManager.instance.playSFX(moveSound);
    }

    public void bob(Vector2 centerPosition) {
        if (state == State.MOVING) {
            SoundManager.instance.stopSFX(moveSound);
            //SoundManager.instance.playSFX(moveSoundEnd);
        }
        state = State.BOBBING;
        time = 0;
        startPos = centerPosition;
    }

    public void setStartFromCurrentPosition() {

        Vector2 currentPos = rb2d.position;

        if ((currentPos-pathPoints[0]).sqrMagnitude < (currentPos - pathPoints[1]).sqrMagnitude) {
            lastVisitedIndex = 0;
        } else {
            lastVisitedIndex = 1;
        }

        startPos = currentPos;

    }

    public void setPathIndexImmediately(int pathIndex) {
        if ((state == State.IDLE || state == State.BOBBING) &&
            lastVisitedIndex == pathIndex) {
            return;
        }

        rb2d.position = pathPoints[pathIndex];
        setStartFromCurrentPosition();
        bob(startPos);
    }

    void Awake() {
        rb2d = GetComponent<Rigidbody2D>();
        pc2d = GetComponent<PolygonCollider2D>();
        timeUser = GetComponent<TimeUser>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // get points
        EdgeCollider2D ec2d = transform.Find("Path").GetComponent<EdgeCollider2D>();
        if (ec2d.pointCount != 2) {
            Debug.LogError("Elevator path must exactly have 2 points");
            return;
        }
        for (int i=0; i<ec2d.pointCount; i++) {
            pathPoints.Add(transform.TransformPoint(ec2d.points[i]));
        }
        if (Mathf.Abs(pathPoints[0].x - pathPoints[1].x) < .1f) {
            float avg = (pathPoints[0].x + pathPoints[1].x) / 2;
            pathPoints[0] = new Vector2(avg, pathPoints[0].y);
            pathPoints[1] = new Vector2(avg, pathPoints[1].y);
        }
    }

    void Start() {
        setStartFromCurrentPosition();
        bob(startPos);
    }

    Vector2 playerDiff = new Vector2();

    void Update() {

        if (timeUser.shouldNotUpdate)
            return;

        // detect control to move platform
        if (state == State.IDLE || state == State.BOBBING) {
            if (playerIsReady && (Keys.instance.upPressed || Keys.instance.downPressed)) {
                playerDiff = Player.instance.transform.localPosition - transform.localPosition;
                movePlatform((lastVisitedIndex == 0 ? 1 : 0), true);
            }
        }

        // updating player position to be with the movement of the platform
        if (state == State.MOVING && resumePlayerMovementOnMoveComplete) {
            Player.instance.transform.localPosition = transform.localPosition + new Vector3(playerDiff.x, playerDiff.y);
        }
        //http://docs.unity3d.com/uploads/Main/monobehaviour_flowchart.svg

    }

    void FixedUpdate() {

        if (timeUser.shouldNotUpdate)
            return;
        
        time += Time.fixedDeltaTime;
        Vector2 pos = rb2d.position;

        switch (state) {
        case State.MOVING:
            Vector2 endPos = pathPoints[endIndex];
            float duration = 0;
            float dist = Vector2.Distance(startPos, endPos);
            float accelDist = speed*speed / (2 * beginAccel);
            float decelDist = speed*speed / (2 * endDecel);
            if (dist > accelDist + decelDist) {
                float d = 0;
                float accelDuration = accelDist / speed;
                float midDist = dist - accelDist - decelDist;
                float midDuration = midDist / speed;
                float decelDuration = decelDist / speed;
                duration = accelDuration + midDuration + decelDuration;
                if (time < accelDuration) {
                    //d = .5f * beginAccel * time * time;
                    d = Utilities.easeInQuad(time, 0, accelDist, accelDuration);
                } else if (time < accelDuration + midDuration) {
                    d = accelDist + midDist * (time - accelDuration) / midDuration;
                } else {
                    //d = dist - (.5f * endDecel * (duration - time) * (duration - time));
                    d = accelDist + midDist + Utilities.easeOutQuad(time - accelDuration - midDuration, 0, decelDist, decelDuration);
                }
                pos = Utilities.easeInOutQuadClamp(d, startPos, endPos - startPos, dist);
            } else {
                duration = dist / speed;
                pos = Utilities.easeInOutQuadClamp(time, startPos, endPos - startPos, duration);
            }
            
            //Debug.Log("time: " + time + " duration: " + duration);
            if (time >= duration) {
                pos = endPos;
                lastVisitedIndex = endIndex;
                if (resumePlayerMovementOnMoveComplete) {
                    Player.instance.receivePlayerInput = true;
                    resumePlayerMovementOnMoveComplete = false;
                }
                bob(endPos);
            }

            //rb2d.MovePosition(pos);
            rb2d.velocity = (pos - rb2d.position) / Time.fixedDeltaTime;

            break;
        case State.BOBBING:
            pos = startPos;
            pos.y += bobDistance * Mathf.Sin(time / bobPeriod * Mathf.PI * 2);
            rb2d.MovePosition(pos);
            break;
        }

        //Utilities.inverseInterpolate()
        //Utilities.closestPointOnLineToPoint()

    }

    void OnSaveFrame(FrameInfo fi) {
        fi.state = (int)state;
        fi.floats["t"] = time;
        fi.ints["lvi"] = lastVisitedIndex;
        fi.floats["spx"] = startPos.x;
        fi.floats["spy"] = startPos.y;
        fi.ints["ei"] = endIndex;
        fi.bools["rpm"] = resumePlayerMovementOnMoveComplete;
    }

    void OnRevert(FrameInfo fi) {
        state = (State)fi.state;
        time = fi.floats["t"];
        lastVisitedIndex = fi.ints["lvi"];
        startPos.Set(fi.floats["spx"], fi.floats["spy"]);
        endIndex = fi.ints["ei"];
        resumePlayerMovementOnMoveComplete = fi.bools["rpm"];
    }

    Vector2 pathPosition(float inter) {
        return Utilities.easeLinear(inter, p0, p1 - p0, 1);
    }

    float time = 0;
    int lastVisitedIndex = 0;
    Vector2 startPos = new Vector2();
    int endIndex = 0;
    bool resumePlayerMovementOnMoveComplete = false;

    Rigidbody2D rb2d;
    PolygonCollider2D pc2d;
    TimeUser timeUser;

#pragma warning disable 414
    SpriteRenderer spriteRenderer;
#pragma warning restore 414

    List<Vector2> pathPoints = new List<Vector2>();

}
