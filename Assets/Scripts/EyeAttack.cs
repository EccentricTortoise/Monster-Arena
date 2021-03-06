using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeAttack : MonoBehaviour
{
    // (Lucas) Eye SFX
    public AudioSource charge;
    public AudioSource fire;

    public Transform target;
    public float speed = 1.0f;
    public float range = 1;
    public float distance = 0;
    private float targetDistance = 0;
    public Vector3 hitPos;
    public Vector3 hitPoint;
    private ContactFilter2D filter;

    // (Lucas) Raycast for aiming.
    private RaycastHit2D[] hits;
    private int hitCount;

    // (Lucas) Timer for attack delay.
    private float attackDelayTimer = 0;
    public float attackDelay;
    public bool attacking = false;

    // (Lucas) Timer for attack duration.
    private float attackDurationTimer = 0;
    public float attackDuration;
    public bool firing = false;

    // (Lucas) Timer for attack cooldown.
    private float cooldownTimer = 0;
    public float cooldown;
    public bool coolingDown = false;

    // (Lucas) Are we allowed to attack?
    public bool attackAllowed = false;

    // (Lucas) Reference to the game manager's health manager so we can hurt the player.
    private HealthManager hManager;
    public int damage = 1;

    // (Lucas) Show the attack.
    public LineRenderer line;
    private List<Vector3> pos;
    private Vector3[] norm = new Vector3[2];

    // (Lucas) Colours for the attack.
    public Color warnEnd = new Color(0, 255, 0, 170);
    public Color warnStart = new Color(255, 255, 255, 255);
    public Color fireEnd = new Color(255, 0, 0, 255);
    public Color fireStart = new Color(255, 0, 0, 255);

    void Awake()
    {
        GameObject manager = GameObject.Find("Game Manager");
        hManager = manager.GetComponent<HealthManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        filter = new ContactFilter2D();
        filter.useTriggers = false;
        line = gameObject.GetComponent<LineRenderer>();
        norm[0] = new Vector3(0, 0, 0);
        norm[1] = new Vector3(0, 0, 0);
    }

    void FixedUpdate()
    {
        // (Lucas) Where to aim and how far away the player is.
        targetDistance = Vector3.Distance(target.position, transform.position);
        Vector3 targetDirection = target.position - transform.position;
        float singleStep = speed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        hits = new RaycastHit2D[20];
        hitCount = Physics2D.Raycast(transform.position, newDirection, filter, hits);
        RaycastHit2D hit = hits[1];
        // (Lucas) Checking the raycast
        if (hit.collider != null) {
            hitPos = hit.transform.position;
            hitPoint = hit.point;
            Vector2 myPos = new Vector2(transform.position.x, transform.position.y);
            distance = Vector2.Distance(hit.point, myPos);
            // (Lucas) If the raycast is hitting the player
            if (hit.transform.gameObject.tag == "Player" && !coolingDown && attackAllowed && !attacking && !firing) {
                attacking = true;
                attackDelayTimer = attackDelay;
                pos = new List<Vector3>();
                pos.Add(transform.position);
                pos.Add(hitPoint);
                line.SetPositions(pos.ToArray());
                line.endColor = warnEnd;
                line.startColor = warnStart;
            }
            else if (hit.transform.gameObject.tag == "Player" && firing && attackAllowed) {
                hManager.DamagePlayer(damage);
            }
        }
        // (Lucas) Draw raycasts in the scene view
        //Debug.DrawRay(transform.position, newDirection * targetDistance, Color.green);
        //Debug.DrawRay(transform.position, newDirection * distance, Color.red);

        switch(attacking) {
            case false:
                // (Lucas) Not attacking, we can rotate
                transform.rotation = Quaternion.LookRotation(newDirection);
                switch(coolingDown) {
                    case true:
                        // (Lucas) In the cooldown phase
                        if (cooldownTimer > 0) {
                            cooldownTimer -= 1;
                        }
                        else {
                            coolingDown = false;
                        }
                        break;
                    case false:
                        break;
                }
                // (Lucas) Hide the laser
                line.SetPositions(norm);
                break;
            case true:
                switch(attackAllowed) {
                    case true:
                        // (Lucas) We're allowed to attack and are attacking
                        // (Lucas) Show the laser
                        pos = new List<Vector3>();
                        pos.Add(transform.position);
                        pos.Add(hitPoint);
                        line.SetPositions(pos.ToArray());

                        // (Lucas) Short pause before we properly fire
                        if (attackDelayTimer > 0) {
                            attackDelayTimer -= 1;
                            // (Lucas) play charging SFX
                            if (!charge.isPlaying) {
                                charge.Play();
                            }
                        }
                        else {
                            switch(firing) {
                                case true:
                                    // (Lucas) Firing
                                    Attack();
                                    if (charge.isPlaying) {
                                        charge.Stop();
                                    }
                                    break;
                                case false:
                                    attackDurationTimer = attackDuration;
                                    firing = true;
                                    break;
                            }
                        }
                        break;
                    case false:
                        // (Lucas) Hide the laser
                        line.SetPositions(norm);
                        break;
                }
                break;
        }
    }

    private void Attack()
    {
        // (Lucas) Attack laser will stay out for a short while
        if (attackDurationTimer > 0) {
            attackDurationTimer -= 1;
            if (!fire.isPlaying) {
                fire.Play();
            }

            // (Lucas) Show the attack using a line renderer
            line.endColor = fireEnd;
            line.startColor = fireStart;

            Debug.Log("Attacking the player");
        }
        else {
            // (Lucas) Attack is over, go into the cooldown state
            attacking = false;
            firing = false;
            fire.Stop();
            Cooldown();
        }
    }

    private void Cooldown()
    {
        // (Lucas) Start the cooldown phase
        cooldownTimer = cooldown;
        coolingDown = true;
    }
}
