using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    //public float moveSpeed; KAMERA

    public Transform orientation;

    public Transform cameraHolder;

    // Player settings
    [SerializeField] private float cameraSensitivity;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float moveInputDeadZone;

    // Touch detection
    private int leftFingerId, rightFingerId;
    private float halfScreenWidth;

    // Camera control
    private Vector2 lookInput;
    private float cameraPitch;

    // Player movement
    private Vector2 moveTouchStartPosition;
    private Vector2 moveInput;

    //float horizontalInput;   KAMERA
    //float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    AudioSource audioSource;


    [Header("Others")]

    // gun raycast
    public Transform cameraP;
    //public LayerMask enemyLayer;
    string hitName;
    GameObject tankr;
    float timer = 0;
    public GameObject hitEffect;

    public int recoil;



    //level and ui
    public int selectedLevel;

    public GameObject uiLevels;
    public GameObject pauseB;
    public GameObject pauseMenu;
    public GameObject crosshair;
    public GameObject lvlfailsc;
    public Button contButton;
    public GameObject lvlcomptxt;

    public GameObject loadingsc;
    public GameObject gate;

    public int health;
    public TMP_Text healthText;
    private string healthstr;
    public TMP_Text levelText;
    private string levelstr;

    public GameObject enemyTank;

    //if totalEnemy is 0, level is completed
    private int totalEnemy = 1;
    public TMP_Text enemyText;
    private string enemystr;

    private int lvlComp;
    private int progress;
    private int goNextLevel;
    private int retryLevel;

    public GameObject nextlvlbut;

    public GameObject tutorialsc;

    [Header("Map")]
    //map objects
    public GameObject base1;
    public GameObject base2;
    public GameObject plane1;
    public GameObject plane2;
    public GameObject hill1;
    public GameObject hill2;
    public GameObject hill3;
    public GameObject hole1;

    [Header("Level Buttons")]
    public Button[] buttons;

    [Header("Sound")]

    public AudioClip enemyFire;
    public AudioClip playerFire;
    public AudioClip buttonSound;



    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        audioSource = GetComponent<AudioSource>();

        health = 4;

        lvlComp = PlayerPrefs.GetInt("LvlFinished");

        for( int i = 0; i < lvlComp + 1; i++)
        {
            buttons[i].interactable = true;
        }

        goNextLevel = PlayerPrefs.GetInt("GoNextlvl");

        if (goNextLevel >= 1)
        {
            buttons[goNextLevel].onClick.Invoke();
        }


        PlayerPrefs.SetInt("GoNextlvl", 0);



        retryLevel = PlayerPrefs.GetInt("Retrylvl");

        if (retryLevel >= 1)
        {
            buttons[retryLevel - 1].onClick.Invoke();
        }
        PlayerPrefs.SetInt("Retrylvl", 0);


        //PlayerPrefs.SetInt("LvlFinished", 0);


        // id = -1 means the finger is not being tracked
        leftFingerId = -1;
        rightFingerId = -1;

        // only calculate once
        halfScreenWidth = Screen.width / 2;

        // calculate the movement input dead zone
        moveInputDeadZone = 10;

    }

    // Update is called once per frame
    void Update()
    {
        //MyInput();  KAMERA
        SpeedControl();

        GetTouchInput();


        if (rightFingerId != -1)
        {
            // Ony look around if the right finger is being tracked
            Debug.Log("Rotating");
            LookAround();
        }
    }

    void FixedUpdate()
    {
        //MovePlayer(); KAMERA
        RaycastGun();

        if (leftFingerId != -1)
        {
            // Ony move if the left finger is being tracked
            Debug.Log("Moving");
            MovePlayer();
        }
    }

    //private void MyInput() KAMERA
    //{
    //    horizontalInput = Input.GetAxisRaw("Horizontal");
    //    verticalInput = Input.GetAxisRaw("Vertical");
    //}

    private void MovePlayer()
    {
        // calculate movement direction
        //moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;  KAMERA

        //rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);


        // Don't move if the touch delta is shorter than the designated dead zone
        if (moveInput.sqrMagnitude <= moveInputDeadZone) return;

        // Multiply the normalized direction by the speed
        Vector2 movementDirection = moveInput.normalized * moveSpeed * Time.deltaTime;
        // Move relatively to the local transform's direction
        //characterController.Move(transform.right * movementDirection.x + transform.forward * movementDirection.y);
        rb.AddRelativeForce(movementDirection * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void RaycastGun()
    {
        if (timer <= 0.05)
        {
            timer += Time.deltaTime;
        }
        else {
            RaycastHit hit;
            if (Physics.Raycast(cameraP.position, cameraP.TransformDirection(Vector3.forward), out hit, 500))
            {
                //Debug.DrawRay(cameraP.position, cameraP.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                //Debug.Log(hit.collider.gameObject.name + " shot down");
                hitName = hit.collider.gameObject.name;

                if (hitName == "tankrr")
                {
                    //EnemyHitbox enemyHitbox = hit.collider.gameObject.GetComponent<EnemyHitbox>();
                    tankr = hit.collider.gameObject;
                    EnemyHitbox enemyHitbox = tankr.GetComponent<EnemyHitbox>();
                    enemyHitbox.LaserHit2();
                    //Debug.Log("tanký vuruyom");

                    audioSource.PlayOneShot(playerFire, 1.0F);

                    rb.AddForce(orientation.forward * recoil * -100f, ForceMode.Impulse);

                    Instantiate(hitEffect, hit.point, Quaternion.identity);
                }
                else
                {
                    //Debug.Log("boþa sýkýyom");
                }
            }
            timer = 0;
        }

    }

    public void LaserHit()
    {
        health -= 1;

        audioSource.PlayOneShot(enemyFire, 1.0F);

        rb.AddForce(orientation.forward * recoil * -100f, ForceMode.Impulse);

        Healthcontrol();
        healthstr = health.ToString();
        healthText.text = "Armor: " + healthstr;
    }


    //level and ui voids
    public void Level1()
    {
        selectedLevel = 1;
        totalEnemy = 3;

        Stage1();

        Instantiate(enemyTank, new Vector3(100, 2, 120), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(256, 2, 74), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(256, 2, -8), Quaternion.identity);

        Invoke("Tutorialon", 1.6f);


    }

    public void Level2()
    {
        selectedLevel = 2;
        totalEnemy = 5;

        Stage1();

        Instantiate(enemyTank, new Vector3(150, 2, 200), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(100, 2, 176), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 2, 70), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 2, -4), Quaternion.identity);
    }

    public void Level3()
    {
        selectedLevel = 3;
        totalEnemy = 7;

        Stage1();

        Instantiate(enemyTank, new Vector3(204, 18, 200), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(204, 18, 154), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(270, 2, 70), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(270, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(220, 2, -4), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(444, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(444, 18, 166), Quaternion.identity);
    }

    public void Level4()
    {
        selectedLevel = 4;
        totalEnemy = 10;

        Stage1();

        Instantiate(enemyTank, new Vector3(204, 18, 200), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(204, 18, 154), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(264, 34, 178), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(303, 34, 138), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(370, 18, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(366, -14, 46), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(348, -14, 8), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(72, 18, 14), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(444, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(434, 18, 138), Quaternion.identity);
    }

    public void Level5()
    {
        selectedLevel = 5;
        totalEnemy = 7;

        Stage2();

        Instantiate(enemyTank, new Vector3(186, 2, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(230, 2, 160), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, 2, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(330, 18, 46), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(324, 18, 18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, -14, 50), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(206, -14, 12), Quaternion.identity);
    }

    public void Level6()
    {
        selectedLevel = 6;
        totalEnemy = 10;

        Stage2();

        Instantiate(enemyTank, new Vector3(250, 2, 200), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(264, 2, 160), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(264, 2, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(294, 18, 176), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(300, 34, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(460, 2, 146), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(450, 2, 194), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(446, 2, 0), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, -14, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(206, -14, 12), Quaternion.identity);
    }

    public void Level7()
    {
        selectedLevel = 7;
        totalEnemy = 13;

        Stage2();

        Instantiate(enemyTank, new Vector3(460, 2, 180), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(424, 2, 194), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(360, 18, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(360, 18, 104), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(378, 18, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(452, 2, -6), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(422, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(466, 2, 22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(330, 18, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, -14, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(206, -14, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(224, -14, 4), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, -14, 50), Quaternion.identity);
    }

    public void Level8()
    {
        selectedLevel = 8;
        totalEnemy = 15;

        Stage2();

        Instantiate(enemyTank, new Vector3(294, 18, 186), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(300, 18, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(296, 34, 152), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(304, 34, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(300, 34, 104), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(310, 34, 120), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(334, 18, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(342, 18, 38), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(348, 18, 58), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(326, 18, 8), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, -14, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(206, -14, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(224, -14, 4), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, -14, 50), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(276, 2, 172), Quaternion.identity);
    }

    public void Level9()
    {
        selectedLevel = 9;
        totalEnemy = 15;

        Stage3();

        Instantiate(enemyTank, new Vector3(200, 18, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(210, 18, 162), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(202, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 18, 178), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(196, 18, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(218, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(200, 34, 76), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 34, 90), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, 34, 116), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(374, 2, 204), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(400, 2, 164), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(442, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(438, 18, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(370, -14, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(342, -14, 50), Quaternion.identity);
    }

    public void Level10()
    {
        selectedLevel = 10;
        totalEnemy = 20;

        Stage3();

        Instantiate(enemyTank, new Vector3(200, 18, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(210, 18, 162), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(202, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 18, 178), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(196, 18, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(218, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(200, 34, 76), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 34, 90), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, 34, 116), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(374, 2, 204), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(400, 2, 164), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(442, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(438, 18, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(370, -14, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(342, -14, 50), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(120, 2, 202), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(140, 2, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(144, 2, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(132, 2, 112), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 2, 162), Quaternion.identity);
    }

    public void Level11()
    {
        selectedLevel = 11;
        totalEnemy = 25;

        Stage3();

        Instantiate(enemyTank, new Vector3(200, 18, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(210, 18, 162), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(202, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 18, 178), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(196, 18, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(218, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(200, 34, 76), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 34, 90), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, 34, 116), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(374, 2, 204), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(400, 2, 164), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(442, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(438, 18, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(370, -14, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(342, -14, 50), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(120, 2, 202), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(140, 2, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(144, 2, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(132, 2, 112), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 2, 162), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(324, 2, 210), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(348, 2, 150), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(454, 2, 82), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(464, 2, -14), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(430, 18, 184), Quaternion.identity);
    }

    public void Level12()
    {
        selectedLevel = 12;
        totalEnemy = 30;

        Stage3();

        Instantiate(enemyTank, new Vector3(200, 18, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(210, 18, 162), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(202, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(246, 18, 178), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(196, 18, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(218, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(200, 34, 76), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 34, 90), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(244, 34, 116), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(374, 2, 204), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(400, 2, 164), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(442, 2, 32), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(438, 18, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(370, -14, 10), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(342, -14, 50), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(120, 2, 202), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(140, 2, 184), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(144, 2, 148), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(132, 2, 112), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 2, 162), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(324, 2, 210), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(348, 2, 150), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(454, 2, 82), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(464, 2, -14), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(430, 18, 184), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(350, 2, 210), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(402, 2, 190), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(404, 2, 126), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(450, 2, 56), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(300, 2, 172), Quaternion.identity);
    }

    public void Level13()
    {
        selectedLevel = 13;
        totalEnemy = 35;

        Stage4();

        Instantiate(enemyTank, new Vector3(322, 18, 108), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(334, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(336, 18, 152), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(332, 18, 168), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(326, 18, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(248, 2, 208), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(230, 2, 214), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, 2, 218), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(268, 2, 196), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 188), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 74), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(276, 2, 60), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(266, 2, -8), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(234, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(216, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(198, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(186, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(250, 2, -18), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(334, 34, 80), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(328, 34, 92), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(358, 34, 106), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(356, 34, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(322, 34, 70), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(112, 18, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 40), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(108, 18, 54), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 18, 62), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(108, -14, 132), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(122, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(118, -14, 186), Quaternion.identity);

    }

    public void Level14()
    {
        selectedLevel = 14;
        totalEnemy = 40;

        Stage4();

        Instantiate(enemyTank, new Vector3(322, 18, 108), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(334, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(336, 18, 152), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(332, 18, 168), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(326, 18, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(248, 2, 208), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(230, 2, 214), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, 2, 218), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(268, 2, 196), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 188), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 74), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(276, 2, 60), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(266, 2, -8), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(234, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(216, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(198, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(186, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(250, 2, -18), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(334, 34, 80), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(328, 34, 92), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(358, 34, 106), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(356, 34, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(322, 34, 70), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(112, 18, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 40), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(108, 18, 54), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 18, 62), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(108, -14, 132), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(122, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(118, -14, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 174), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 102), Quaternion.identity);
    }

    public void Level15()
    {
        selectedLevel = 15;
        totalEnemy = 45;

        Stage4();

        Instantiate(enemyTank, new Vector3(322, 18, 108), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(334, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(336, 18, 152), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(332, 18, 168), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(326, 18, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(248, 2, 208), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(230, 2, 214), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, 2, 218), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(268, 2, 196), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 188), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 74), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(276, 2, 60), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(266, 2, -8), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(234, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(216, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(198, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(186, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(250, 2, -18), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(334, 34, 80), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(328, 34, 92), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(358, 34, 106), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(356, 34, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(322, 34, 70), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(112, 18, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 40), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(108, 18, 54), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 18, 62), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(108, -14, 132), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(122, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(118, -14, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 174), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 102), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(88, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(80, -14, 186), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(88, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(88, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(100, -14, 188), Quaternion.identity);
    }

    public void Level16()
    {
        selectedLevel = 16;
        totalEnemy = 50;

        Stage4();

        Instantiate(enemyTank, new Vector3(322, 18, 108), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(334, 18, 128), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(336, 18, 152), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(332, 18, 168), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(326, 18, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(248, 2, 208), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(230, 2, 214), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(214, 2, 218), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(268, 2, 196), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 188), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 74), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(276, 2, 60), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 42), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 20), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(266, 2, -8), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(234, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(216, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(198, 2, -22), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(186, 2, -18), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(250, 2, -18), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(334, 34, 80), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(328, 34, 92), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(358, 34, 106), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(356, 34, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(322, 34, 70), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(112, 18, 12), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(112, 18, 40), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(108, 18, 54), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(96, 18, 62), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(108, -14, 132), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(122, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(124, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(118, -14, 186), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(278, 2, 174), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 140), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(282, 2, 122), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(278, 2, 102), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(88, -14, 172), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(80, -14, 186), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(88, -14, 158), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(88, -14, 142), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(100, -14, 188), Quaternion.identity);

        Instantiate(enemyTank, new Vector3(234, 2, 170), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 2, 144), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 2, 70), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(220, 2, 26), Quaternion.identity);
        Instantiate(enemyTank, new Vector3(234, 2, 106), Quaternion.identity);
    }


    //Game starts. Crosshair, health bar activation etc.
    public void Uilevelsfalse()
    {
        uiLevels.SetActive(false);
        pauseB.SetActive(true);
        crosshair.SetActive(true);
        Leveltext();
        Enemytext();

        audioSource.PlayOneShot(buttonSound, 1.0F);

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        Loading();
    }

    public void Pause()
    {
        pauseB.SetActive(false);
        pauseMenu.SetActive(true);
        Time.timeScale = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        audioSource.PlayOneShot(buttonSound, 1.0F);
    }

    public void Continue()
    {
        pauseB.SetActive(true);
        pauseMenu.SetActive(false);
        Time.timeScale = 1;

        audioSource.PlayOneShot(buttonSound, 1.0F);
    }

    public void MainMenu()
    {
        audioSource.PlayOneShot(buttonSound, 1.0F);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        //Time.timeScale = 1;
    }

    public void NextLevel()
    {
        audioSource.PlayOneShot(buttonSound, 1.0F);

        PlayerPrefs.SetInt("GoNextlvl", selectedLevel);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RetryLevel()
    {
        audioSource.PlayOneShot(buttonSound, 1.0F);

        PlayerPrefs.SetInt("Retrylvl", selectedLevel);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Exit()
    {
        audioSource.PlayOneShot(buttonSound, 1.0F);
        Application.Quit();
    }

    private void Playerdied()
    {
        Pause();
        contButton.interactable = false;
        lvlfailsc.SetActive(true);
    }

    private void Levelcompleted()
    {
        //Pause();
        progress = PlayerPrefs.GetInt("LvlFinished");
        if (progress < selectedLevel)
        {
            PlayerPrefs.SetInt("LvlFinished", selectedLevel);
        }

        lvlcomptxt.SetActive(true);
        nextlvlbut.SetActive(true);

        Invoke("Pause", 2.0f);
    }

    private void Healthcontrol()
    {
        if(health <= 0)
        {
            Playerdied();
        }
    }

    //incrase health +1 and some queries
    public void Healthup()
    {
        health += 1;
        healthstr = health.ToString();
        healthText.text = "Armor: " + healthstr;

        totalEnemy -= 1;
        Enemytext();
        if (totalEnemy <= 0)
        {
            Levelcompleted();
        }

    }

    private void Leveltext()
    {
        levelstr = selectedLevel.ToString();
        levelText.text = "Level " + levelstr;
    }

    private void Enemytext()
    {
        enemystr = totalEnemy.ToString();
        enemyText.text = "Targets: " + enemystr;
    }

    private void Loading()
    {
        loadingsc.SetActive(true);
        Invoke("Loadingend", 1.0f);
    }

    private void Loadingend()
    {
        loadingsc.SetActive(false);
        gate.SetActive(false);

        audioSource.PlayOneShot(buttonSound, 1.0F);

    }

    private void Stage1()
    {
        base1.transform.position = new Vector3(0, 0, 0);
        base1.transform.eulerAngles = new Vector3(0, 0, 0);

        base2.transform.position = new Vector3(448, 0, 0);
        base2.transform.eulerAngles = new Vector3(0, 0, 0);

        plane1.transform.position = new Vector3(96, 0, 160);
        plane1.transform.eulerAngles = new Vector3(0, 0, 0);

        plane2.transform.position = new Vector3(224, 0, 32);
        plane2.transform.eulerAngles = new Vector3(0, 0, 0);

        hill1.transform.position = new Vector3(96, 0, 32);
        hill1.transform.eulerAngles = new Vector3(0, 0, 0);

        hill2.transform.position = new Vector3(224, 0, 160);
        hill2.transform.eulerAngles = new Vector3(0, 0, 0);

        hill3.transform.position = new Vector3(352, 0, 160);
        hill3.transform.eulerAngles = new Vector3(0, 0, 0);

        hole1.transform.position = new Vector3(352, 0, 32);
        hole1.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void Stage2()
    {
        base1.transform.position = new Vector3(448, 0, 0);
        base1.transform.eulerAngles = new Vector3(0, 0, 0);

        base2.transform.position = new Vector3(0, 0, 0);
        base2.transform.eulerAngles = new Vector3(0, 0, 0);

        plane1.transform.position = new Vector3(96, 0, 160);
        plane1.transform.eulerAngles = new Vector3(0, 0, 0);

        plane2.transform.position = new Vector3(224, 0, 160);
        plane2.transform.eulerAngles = new Vector3(0, 0, 0);

        hill1.transform.position = new Vector3(352, 0, 32);
        hill1.transform.eulerAngles = new Vector3(0, 0, 0);

        hill2.transform.position = new Vector3(352, 0, 160);
        hill2.transform.eulerAngles = new Vector3(0, 180, 0);

        hill3.transform.position = new Vector3(96, 0, 32);
        hill3.transform.eulerAngles = new Vector3(0, 180, 0);

        hole1.transform.position = new Vector3(224, 0, 32);
        hole1.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void Stage3()
    {
        base1.transform.position = new Vector3(0, 0, 0);
        base1.transform.eulerAngles = new Vector3(0, 0, 0);

        base2.transform.position = new Vector3(448, 0, 0);
        base2.transform.eulerAngles = new Vector3(0, 0, 0);

        plane1.transform.position = new Vector3(96, 0, 160);
        plane1.transform.eulerAngles = new Vector3(0, 0, 0);

        plane2.transform.position = new Vector3(352, 0, 160);
        plane2.transform.eulerAngles = new Vector3(0, 0, 0);

        hill1.transform.position = new Vector3(96, 0, 32);
        hill1.transform.eulerAngles = new Vector3(0, 0, 0);

        hill2.transform.position = new Vector3(224, 0, 160);
        hill2.transform.eulerAngles = new Vector3(0, 90, 0);

        hill3.transform.position = new Vector3(224, 0, 32);
        hill3.transform.eulerAngles = new Vector3(0, 90, 0);

        hole1.transform.position = new Vector3(352, 0, 32);
        hole1.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void Stage4()
    {
        base1.transform.position = new Vector3(0, 0, 0);
        base1.transform.eulerAngles = new Vector3(0, 0, 0);

        base2.transform.position = new Vector3(448, 0, 0);
        base2.transform.eulerAngles = new Vector3(0, 0, 0);

        plane1.transform.position = new Vector3(224, 0, 32);
        plane1.transform.eulerAngles = new Vector3(0, 0, 0);

        plane2.transform.position = new Vector3(224, 0, 160);
        plane2.transform.eulerAngles = new Vector3(0, 0, 0);

        hill1.transform.position = new Vector3(96, 0, 32);
        hill1.transform.eulerAngles = new Vector3(0, 0, 0);

        hill2.transform.position = new Vector3(352, 0, 160);
        hill2.transform.eulerAngles = new Vector3(0, 90, 0);

        hill3.transform.position = new Vector3(352, 0, 32);
        hill3.transform.eulerAngles = new Vector3(0, 90, 0);

        hole1.transform.position = new Vector3(96, 0, 160);
        hole1.transform.eulerAngles = new Vector3(0, 0, 0);
    }

    private void GetTouchInput()
    {
        // Iterate through all the detected touches
        for (int i = 0; i < Input.touchCount; i++)
        {

            Touch t = Input.GetTouch(i);

            // Check each touch's phase
            switch (t.phase)
            {
                case TouchPhase.Began:

                    if (t.position.x < halfScreenWidth && leftFingerId == -1)
                    {
                        // Start tracking the left finger if it was not previously being tracked
                        leftFingerId = t.fingerId;

                        // Set the start position for the movement control finger
                        moveTouchStartPosition = t.position;
                    }
                    else if (t.position.x > halfScreenWidth && rightFingerId == -1)
                    {
                        // Start tracking the rightfinger if it was not previously being tracked
                        rightFingerId = t.fingerId;
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    if (t.fingerId == leftFingerId)
                    {
                        // Stop tracking the left finger
                        leftFingerId = -1;
                        Debug.Log("Stopped tracking left finger");
                    }
                    else if (t.fingerId == rightFingerId)
                    {
                        // Stop tracking the right finger
                        rightFingerId = -1;
                        Debug.Log("Stopped tracking right finger");
                    }

                    break;
                case TouchPhase.Moved:

                    // Get input for looking around
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = t.deltaPosition * cameraSensitivity * Time.deltaTime;
                    }
                    else if (t.fingerId == leftFingerId)
                    {

                        // calculating the position delta from the start position
                        moveInput = t.position - moveTouchStartPosition;
                    }

                    break;
                case TouchPhase.Stationary:
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = Vector2.zero;
                    }
                    break;
            }
        }
    }

    private void LookAround()
    {

        // vertical (pitch) rotation
        cameraPitch = Mathf.Clamp(cameraPitch - lookInput.y, -90f, 90f);
        cameraP.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

        // horizontal (yaw) rotation
        transform.Rotate(0, lookInput.x, 0, Space.World);
      //orientation.Rotate(0, lookInput.x, 0, Space.World);
        cameraHolder.Rotate(0, lookInput.x, 0, Space.World);
    }

    public void Tutorialon()
    {
        tutorialsc.SetActive(true);
    }

    public void Tutorialoff()
    {
        tutorialsc.SetActive(false);
        audioSource.PlayOneShot(buttonSound, 1.0F);
    }

}