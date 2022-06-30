using RiptideNetworking;
using RiptideNetworking.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Vector3 MoveDirection => moveDirection;

    public bool isForcedCrouch { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private PlayerCombat combat;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform camProxy;

    [SerializeField] private float gravity;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float pushPower;

    [SerializeField] private float crouchThreshold;
    [SerializeField] private Transform crouchChecker;
    [SerializeField] private float crouchCheckRadius;
    [SerializeField] private LayerMask crouchCheckMask;

    private float gravityAcceleration;
    private Vector3 moveDirection;
    private float moveSpeed;
    private float jumpSpeed;

    private bool[] inputs;
    private float yVelocity;

    private float crouchTimer;
    private bool isCrouching;

    private void OnValidate()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();
        if (player == null)
            player = GetComponent<Player>();

        Initialize();
    }

    private void Start()
    {
        Initialize();

        inputs = new bool[10];
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
        Move(inputDirection, inputs[4], isCrouching);
    }

    private void Initialize()
    {
        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
        jumpSpeed = Mathf.Sqrt(jumpHeight * -2f * gravityAcceleration);
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

    private void Move(Vector2 inputDirection, bool jump, bool crouch)
    {
        //CHANGE MOVEMENT LOGIC LATER
        moveDirection = Vector3.Normalize(camProxy.right * inputDirection.x + Vector3.Normalize(flattenVector3(camProxy.forward)) * inputDirection.y);
        moveDirection *= moveSpeed;


        //HITBOX SIZE HANDLING
        if (crouch)
        {
            moveDirection *= 0.25f;
            controller.height = 1.3f;
            controller.center = new Vector3(0, -.35f, 0);
        } 
        else
        {
            if (combat.isGrabbing)
                moveDirection *= 0.75f;
            controller.height = 1.8f;
            controller.center = new Vector3(0, -.1f, 0);
        }

        if (controller.isGrounded)
        {
            yVelocity = 0f;
            if (jump)
                yVelocity = jumpSpeed;
        }
        yVelocity += gravityAcceleration;

        moveDirection.y = yVelocity;
        controller.Move(moveDirection);

        SendMovement();
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
            return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f)
            return;

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }

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
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
