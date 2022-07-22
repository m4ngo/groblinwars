using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public Vector3 MoveDirection => moveDirection;

    [SerializeField] private Player player;
    [SerializeField] private PlayerCombat combat;
    //[SerializeField] private CharacterController controller;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform camProxy;
    [SerializeField] private CapsuleCollider collider;

    [SerializeField] private float killFloor;

    [SerializeField] private float gravity;
    [SerializeField] private float maxFallSpeed;

    [SerializeField] private float movementSpeed;
    [SerializeField] private float slowDownSpeed;
    [SerializeField] private float speedUpSpeed;

    [SerializeField] private float jumpHeight;
    //[SerializeField] private float pushPower;

    private bool isGrounded;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float checkRadius;

    [SerializeField] private float crouchThreshold;
    [SerializeField] private Transform crouchChecker;
    [SerializeField] private float crouchCheckRadius;
    [SerializeField] private LayerMask crouchCheckMask;

    [SerializeField] private float crawlCheckOffset;

    private Vector3 moveDirection;
    private Vector3 knockbackDirection;
    private float cantMove = 0;

    private bool[] inputs;

    private float crouchTimer;
    private bool isCrouching;
    private bool isForcedCrouch;

    private float crawlTimer;
    private bool isCrawling;
    private bool isForcedCrawl;

    private float dead = 0;

    public void SetKnockback(Vector3 kb) { knockbackDirection = kb; }
    public Vector3 GetKnockback() { return knockbackDirection; }

    private void OnValidate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();

        inputs = new bool[10];
    }

    private void Update()
    {
        DeathHandler();
    }

    private void FixedUpdate()
    {
        Vector2 inputDirection = Vector2.zero;
        if (inputs[0])
            inputDirection.y += 1;
        if (inputs[1])
            inputDirection.y -= 1;
        if (inputs[2])
            inputDirection.x -= 1;
        if (inputs[3])
            inputDirection.x += 1;

        if (Physics.CheckSphere(crouchChecker.position, crouchCheckRadius, crouchCheckMask))
        {
            isCrouching = true;
            isForcedCrouch = true;
        }
        else
        {
            CrouchHandler();
            isForcedCrouch = false;
        }

        if (Physics.CheckSphere(new Vector3(crouchChecker.position.x, crouchChecker.position.y - crawlCheckOffset, crouchChecker.position.z), crouchCheckRadius, crouchCheckMask))
        {
            isCrawling = true;
            isForcedCrawl = true;
        }
        else
        {
            CrawlHandler();
            isForcedCrawl = false;
        }

        Move(inputDirection, inputs[4], isCrouching, isCrawling);
    }

    private void Initialize()
    {
        
    }

    private void DeathHandler()
    {
        if (transform.position.y <= killFloor)
        {
            dead = 5;
            rb.isKinematic = true;
            transform.position = new Vector3(0, 15, 0);
            SendDeath(false);
        }

        if (dead > 0)
        {
            if (dead - Time.deltaTime <= 0)
            {
                rb.isKinematic = false;
                transform.position = NetworkManager.Singleton.GetSpawnpoint();
                SendDeath(true);
            }
            dead -= Time.deltaTime;
        }
    }

    private void CrouchHandler()
    {
        if (inputs[5])
        {
            crouchTimer += Time.fixedDeltaTime;
            if (crouchTimer >= crouchThreshold)
                isCrouching = true;
        }
        else
        {
            crouchTimer -= Time.fixedDeltaTime;
            if (crouchTimer <= 0)
                isCrouching = false;
        }
        crouchTimer = Mathf.Clamp(crouchTimer, 0, crouchThreshold);
    }

    private void CrawlHandler()
    {
        if (inputs[6])
        {
            crawlTimer += Time.fixedDeltaTime;
            if (crawlTimer >= crouchThreshold)
                isCrawling = true;
        }
        else
        {
            crawlTimer -= Time.fixedDeltaTime;
            if (crawlTimer <= 0)
                isCrawling = false;
        }
        crawlTimer = Mathf.Clamp(crawlTimer, 0, crouchThreshold);
    }

    private void Move(Vector2 inputDirection, bool jump, bool crouch, bool crawl)
    {
        //CHANGE MOVEMENT LOGIC LATER
        moveDirection = Vector3.Normalize(camProxy.right * inputDirection.x + Vector3.Normalize(flattenVector3(camProxy.forward)) * inputDirection.y);
        moveDirection *= movementSpeed;

        isGrounded = Physics.CheckSphere(groundCheck.position, checkRadius, groundMask);
        if (!isGrounded && rb.velocity.y < 0.85f)
        {
            rb.velocity = new Vector3(rb.velocity.x, Mathf.Clamp(-Mathf.Abs(rb.velocity.y * gravity), maxFallSpeed, Mathf.Infinity), rb.velocity.z);
        }

        //HITBOX SIZE HANDLING
        if (crawl)
        {
            moveDirection *= 0.1f;
            collider.height = 1f;
            collider.center = new Vector3(0, -.5f, 0);
        }
        else if (crouch)
        {
            moveDirection *= 0.25f;
            collider.height = 1.3f;
            collider.center = new Vector3(0, -.35f, 0);
        } 
        else
        {
            if (combat.isGrabbing)
                moveDirection *= 0.75f;
            collider.height = 1.8f;
            collider.center = new Vector3(0, -.1f, 0);
        }

        if (isGrounded && cantMove <= 0 && jump)
            rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);

        //moveDirection.y = yVelocity;
        //controller.Move(moveDirection + knockbackDirection);
        Vector3 velocityNoY = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if(cantMove <= 0)
        {
            if (inputDirection != Vector2.zero)
                rb.AddForce(moveDirection * Time.fixedDeltaTime * 100f * (1 / Mathf.Clamp(velocityNoY.magnitude, 0.75f, 5f) * speedUpSpeed));
            else
                rb.AddForce(-velocityNoY.normalized * Time.fixedDeltaTime * 100f * slowDownSpeed * Mathf.Clamp(velocityNoY.sqrMagnitude, slowDownSpeed / 2, 20));
            rb.drag = 6f;
        } else
        {
            moveDirection = Vector3.zero;
            cantMove -= Time.fixedDeltaTime;
            rb.drag = 0.5f;
        }

        SendMovement();
    }
    /*
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        // no rigidbody
        if (rb == null || rb.isKinematic)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        if (rb.velocity.magnitude <= combat.GetGrabVelocityThreshold())
        {
            if(hit.moveDirection.y > -0.3f)
            {
                Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

                // If you know how fast your character is trying to move,
                // then you can also multiply the push velocity by that.

                // Apply the push
                rb.velocity = pushDir * pushPower;
            }
            return;
        }
    }*/

    private Vector3 flattenVector3(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public void SetInput(bool[] inputs, Vector3 forward)
    {
        this.inputs = inputs;
        camProxy.forward = forward;
    }

    private void SendMovement()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
            return;

        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.Id);
        message.AddUShort(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        message.AddBool(isCrouching);
        message.AddBool(isCrawling);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    private void SendDeath(bool alive)
    {
        Message message = Message.Create(MessageSendMode.reliable, ServerToClientId.playerDied);
        message.AddUShort(player.Id);
        message.AddBool(alive);
        NetworkManager.Singleton.Server.SendToAll(message);
    }

    public void StopMovement(float time)
    {
        cantMove = time;
    }
}
