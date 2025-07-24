using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;


using UnityEngine;
using UnityEngine.UI;
public class PlayerControl : MonoBehaviour
{

    //To Identify Player
    public int PlayerNumber = 1;
    public Transform enemy;

    private Rigidbody2D rig2d;
    private Animator anim;

    float horizontal;
    float vertical;
    public float maxSpeed = 25;
    Vector3 movement;

    public float JumpForce = 20;
    public float jumpDuration = .1f;
    float jmpDuration;
    float jmpForce;
    public float speed = 5f;
    bool jumpKey = false;
    bool falling;
    bool onGround;
    bool crouch;
    public GameObject finishUI;
    public TMP_Text finishText;
    public float attackRate = 0.3f;
    bool[] attack = new bool[2]; //Array so we can add new attacks later
    float[] attackTimer = new float[2];
    int[] timesPressed = new int[2];

    public bool damage;
    public float invincibility = 1; //time it doesn't take damage after hurt
    float invincibilityTimer;

    public bool specialAttack;
    public GameObject projectile;

    public float health;
    public HealthbarManager healthUI; //thanh mau o ui

    // Use this for initialization
    void Start()
    {
        //init cac bien so can thiet khi bat dau script
        healthUI = GetComponent<HealthbarManager>();
        healthUI.maxHealth = health;
        healthUI.health = health;
        finishUI.SetActive(false);
        rig2d = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();

        jmpForce = JumpForce;
        Time.timeScale = 1f; //set thoi gian o toc do binh thuong

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject pl in players)
        {
            var ctrl = pl.GetComponent<PlayerControl>();
            if (ctrl != this && ctrl.PlayerNumber != this.PlayerNumber)
            {
                enemy = pl.transform;
                break;
            }
        }
    }

    void Update()
    {
        AttackInput();
        ScaleCheck();
        OnGroundCheck();
        Damage();
        SpecialAttack();
        UpdateAnimator();
        WinCheck();
    }

    //Fixed Update because we are using their physics
    void FixedUpdate()
    {

        //Reason why we do this is if we want to have more than 2 players it's easier for the future
        horizontal = Input.GetAxis("Horizontal" + PlayerNumber.ToString());
        vertical = Input.GetAxis("Vertical" + PlayerNumber.ToString());

        // Only horizontal movement. Vertical is crouch and jump
        Vector3 movement = new Vector3(horizontal * speed, 0, 0);

        // if vertical < -0.1f crouch = true/false
        crouch = (vertical < -0.1f);

        if (vertical > 0.1f)
        {
            if (!jumpKey)
            {
                jmpDuration += Time.deltaTime;
                jmpForce += Time.deltaTime;

                if (jmpDuration < jumpDuration)
                {
                    rig2d.linearVelocity = new Vector2(rig2d.linearVelocity.x, jmpForce);
                }
                else
                {
                    jumpKey = true;
                }
            }
        }
        //check if player's falling
        if (!onGround && vertical < 0.1f)
        {
            falling = true;
        }
        //ktra neu player dang attack thi cancel movement
        if ((attack[0] && !jumpKey) || (attack[1] && !jumpKey))
            movement = Vector3.zero; // cancel movement
        //tang toc do chay neu horizontal < maxspeed
        if (!crouch)
        {

            if (Mathf.Abs(horizontal) < maxSpeed)
            {
                rig2d.AddForce(movement * maxSpeed);
            }
        }
        else if (crouch && onGround)
            rig2d.linearVelocity = Vector3.zero; // No sprites so we will not allow movement for now **Change if adding features
    }

    void WinCheck() //check neu player enemy chet (health <= 0) thi show win screen
    {
        if (enemy.GetComponent<PlayerControl>().health <= 0)
        {
            StartCoroutine(WinGame());
            
            string title = $"Player {PlayerNumber} Won!";
            finishText.SetText(title); //set title thanh "player 1 won" hoac "player 2 won"
            finishUI.SetActive(true); // show win screen
        }

    }
    IEnumerator WinGame()
    {
        yield return new WaitForSeconds(1.5f); //cho 1.5s de xong animation player trong game
        Time.timeScale = 0f; //dung thoi gian lai
    }

    void ScaleCheck() //flip player theo chieu ngang (xoay trai/phai) khi di chuyen
    {
        if (transform.position.x < enemy.position.x)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
    }

    void AttackInput() //thuc hien attack o input system
    {
        if (Input.GetButtonDown("Attack1" + PlayerNumber.ToString()))
        {
            Debug.Log($"{PlayerNumber.ToString()} Attack 1");
            attack[0] = true;
            attackTimer[0] = 0;
            timesPressed[0]++;
        }
        if (attack[0])
        {
            attackTimer[0] += Time.deltaTime;

            if (attackTimer[0] > attackRate || timesPressed[0] >= 4)
            {
                attackTimer[0] = 0;
                attack[0] = false;
                timesPressed[0] = 0;
            }
        }
        if (Input.GetButtonDown("Attack2" + PlayerNumber.ToString()))
        {
            Debug.Log($"{PlayerNumber.ToString()} Attack 2");
            attack[1] = true;
            attackTimer[1] = 0;
            timesPressed[1]++;
        }
        if (attack[1])
        {
            attackTimer[1] += Time.deltaTime;

            if (attackTimer[1] > attackRate || timesPressed[1] >= 4)
            {
                attackTimer[1] = 0;
                attack[1] = false;
                timesPressed[1] = 0;
            }
        }
    }

    void Damage()
    {
        if (damage)
        {
            //khi an dmg thi player se bat tu trong thoi gian ngan
            invincibilityTimer += Time.deltaTime;

            if (invincibilityTimer > invincibility)
            {
                damage = false;
                invincibilityTimer = 0;
            }
        }

    }

    void SpecialAttack()
    {
        if (specialAttack)
        {
            Vector3 pos;
            if (enemy.position.x - transform.position.x > 0)
            {
                pos = transform.position + new Vector3(1, 0, 0);
            }
            else
            {
                pos = transform.position + new Vector3(-2, 0, 0);
            }

            GameObject pr = Instantiate(projectile, pos, Quaternion.identity) as GameObject;
            Vector3 nrDir = new Vector3(enemy.position.x, transform.position.y, 0);
            Vector3 dir = nrDir - transform.position;
            if (transform.position.x > enemy.position.x)
            {
                pr.transform.localScale = new Vector3(-1, 1, 1);
            }

            pr.GetComponent<Rigidbody2D>().AddForce(dir * 3, ForceMode2D.Impulse);

            specialAttack = false;
            Destroy(pr, 2);
        }

    }

    void OnGroundCheck() //apply trong luc khac nhau khi player o mat dat hoac khong de tang trai nghiem choi
    {
        if (!onGround)
            rig2d.gravityScale = 3;
        else
            rig2d.gravityScale = 1;
    }

    void UpdateAnimator() //update animation
    {
        anim.SetBool("Crouch", this.crouch);
        anim.SetBool("OnGround", this.onGround);
        anim.SetBool("Falling", this.falling);
        anim.SetFloat("Movement", Mathf.Abs(horizontal));
        anim.SetBool("Attack1", this.attack[0]);
        anim.SetBool("Attack2", this.attack[1]);
    }

    //void OnTriggerEnter2D(Collider2D col)
    //{
    //    if (col.gameObject.tag == "Projectile")
    //    {
    //        Destroy(col.gameObject);
    //    }
    //}

    //Enter collision with ground - on ground
    void OnCollisionEnter2D(Collision2D col)
    {

        if (col.collider.tag == "Ground")
        {
            onGround = true;

            //Change later
            rig2d.linearVelocity = new Vector3(.1f, 0, 0);

            jumpKey = false;
            jmpDuration = 0;
            jmpForce = JumpForce;
            falling = false;
        }
    }

    //Exit collision with ground - air
    void OnCollisionExit2D(Collision2D col)
    {
        if (col.collider.tag == "Ground")
        {
            onGround = false;
        }
    }

    

}