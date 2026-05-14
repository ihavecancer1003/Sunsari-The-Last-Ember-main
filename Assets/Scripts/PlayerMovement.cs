using UnityEngine;
using System.Collections;
using UnityEngine.UI;
// !!! end update comment
// 1. PlayerCombat script hiisen bgaa, tgeed tulaantai holbootoi coduudiig tiishee hiitsen
// 2. input buffer hiisen bgaa tgeed dairah bolon usreh functsuudiig ter nohtsol shalgaltandaa hiitsen bgaa
// 3. detectioniig agaar deer usreed bhaar in bagasgatsan 0.5f --> 0.1f bolgoson bgaa
// 4. water hiisen bgaa ghdee dutuu
// 5. !!! HAMGIIN TOM UPDATE: baruun zuun tiish hudluh logikiig oorchilson. zuun tiish -1 baruun tiish 1 bsan bolhoor zereg darahad zogsood bsan. odoo hamgiin suuld daragdsan tovchiig dagna. Jishee n "a" daraad zuun tiish yavj baital "d" darval baruun tiish yavna "a" daragdsan heveer bsan ch
public class PlayerMovement : MonoBehaviour
{
    [Header("Animation Setup")]
    public Animator anim; 

    [Header("Movement & Dash")]
    public float speed = 8f;
    public float dashForce = 20f;
    public float dashDuration = 0.2f;
    public bool isDashing, canDash = true, isFacingRight = true;

    [Header("Jump Settings")]
    public int extraJumpsValue = 1; 
    private int extraJumps;
    private float lastRightTime;
    private float lastLeftTime;
    public float jumpForce = 12f;

    [Header("Detection")]
    public Transform groundCheck;
    public float checkRadius = 0.1f; //agaar deer usreed bsan bolhoor bagasgasan 0.5 --> 0.1
    public LayerMask groundLayer;
    private bool isGrounded;
    private Rigidbody2D rb;
    private Collider2D playerCollider;
    public LayerMask platformLayer;
    private GameObject currentPlatform;
    private bool isTouchingWall;

    [Header("Input Buffering")]
    public float bufferWindow = 0.07f; // 0.12 sekunded hiih uildel sanana
    private float jumpBufferTimer;

    [Header("WallJump")]
    public float wallJumpForce = 10f;
    public float wallJumpWindow = 0.5f;
    private bool isWallJumping = false;

    private PlayerCombat combat;
    //togloom ehelehiin omno ajildag function. unity built-in 
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        combat = GetComponent<PlayerCombat>();
        playerCollider = GetComponent<Collider2D>();
    }
    void Start() {
        extraJumps = extraJumpsValue;
    }

    void Update() {

        jumpBufferTimer -= Time.deltaTime; // usreh buffer timer

        if (isDashing) return;
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer | platformLayer);

        if (isGrounded) {
            extraJumps = extraJumpsValue;
        } else if (isTouchingWall)
        {
            extraJumps = extraJumpsValue; // wall jump hiisen daraa extra jumps reset hiih
        }
        // jump function iishee oruulsiishuu
        // --- input hadgalah ---
        if (Input.GetKeyDown(KeyCode.W)) jumpBufferTimer = bufferWindow;

        // --- gazar buusan eseh ---
        if (jumpBufferTimer > 0)
        {
            if(isTouchingWall && !isGrounded)
            {
                Walljump();
                if (!isGrounded && !isTouchingWall) extraJumps--;
                isWallJumping = true;
                jumpBufferTimer = 0; // uildel hiisnii daraa buffer arilgana
            }

            if (isGrounded || extraJumps > 0)
            {
                Jump();
                if (!isGrounded) extraJumps--;
                jumpBufferTimer = 0; // uildel hiisnii daraa buffer arilgana
            }
        }
        // jump logic soligdson
        // --- Baruun zuun tiish hudluhud hamgiin suuld daragdsan tovchiig dagna ---
        if (Input.GetKeyDown(KeyCode.D)) lastRightTime = Time.time;
        if (Input.GetKeyDown(KeyCode.A)) lastLeftTime = Time.time;

        float moveInput = 0;
        bool d = Input.GetKey(KeyCode.D);
        bool a = Input.GetKey(KeyCode.A);

        if (d && a)
        {
            // suuld daragdsan tovch
            moveInput = (lastRightTime > lastLeftTime) ? 1 : -1;
        }
        else if (d)
        {
            moveInput = 1;
        }
        else if (a)
        {
            moveInput = -1;
        }

        if (moveInput != 0) {
            // Товчлуур дарсан үед хурд өгнө
            rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isRunning", true);
        } else {
            // ТОВЧЛУУРЫГ ТАВИХАД ХУРДЫГ ШУУД 0 БОЛГОЖ ГУЛГАЛТЫГ ЗОГСООНО
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("isRunning", false);
        }

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();

        // Dash Logic
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash) {
            if (Input.GetKey(KeyCode.S)) {
                StartCoroutine(DashDown());
            } else {
                StartCoroutine(Dash());
            }
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            // Check if we are standing on a platform layer
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.2f, platformLayer);
            if (hit.collider != null)
            {
                // If the object we hit is a platform, start the drop routine
                StartCoroutine(DisableCollision(hit.collider));
            }
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingWall = true;
            Debug.Log("Touching the wall!");
        }
    }

    // This runs the moment you pull away from the wall
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isTouchingWall = false;
            Debug.Log("No longer touching the wall.");
        }
    }
    IEnumerator Dash() {
        canDash = false; 
        isDashing = true; 
        float grad = rb.gravityScale; 
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2((isFacingRight ? 1 : -1) * dashForce * 2f, 0);
        yield return new WaitForSeconds(dashDuration);
        rb.gravityScale = grad; 
        isDashing = false;
        yield return new WaitForSeconds(0.5f); 
        canDash = true;
    }

    IEnumerator DashDown() {
        canDash = false;
        isDashing = true;
        rb.linearVelocity = new Vector2(0, -dashForce * 2f); 
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(0.5f);
        canDash = true;
    }
    IEnumerator DisableCollision(Collider2D platformCollider)
    {
        // Temporarily ignore collision between player and THIS specific platform
        Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
        yield return new WaitForSeconds(0.3f); // Time it takes to fall through
        Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
    }

    void Jump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    void Walljump() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, wallJumpForce);
    
    void Flip() { 
        isFacingRight = !isFacingRight; 
        transform.localScale = new Vector3(isFacingRight ? 1 : -1, 1, 1); 
    }
}