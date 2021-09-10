using UnityEngine;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour {
    public float movementSpeed = 16f;
    public float jumpHeight = 4f;
    public float slideSpeed = 16f;
    // Resistance values remain the same regardless of material, may want to have per-material values instead?
    [Range (0, 1)]
    public float friction = 0.875f;
    [Range (0, 1)]
    public float viscosity = 0.5f;

    public float jumpCooldown = 1f;

    public Transform playerCamera;
    public float sensitivity = 2f;

    [Tooltip("Objects with positions which must be relative to the player")]
    public List<Transform> relativeObjects;

    PlayerStatus playerStatus;

    CharacterController playerController;
    Vector3 velocity;
    Vector3 directionalInput;

    bool isGrounded = false;

    Vector3 latestSurfaceContactPoint;
    Vector3 latestSurfaceNormal;
    float latestSurfaceNormalAngle;

    Vector3 slideDirection;

    float jumpCooldownRemaining;

    float yaw = 0f;
    float pitch = 0f;

    private Vector3 directionalVelocity;

    private float localMovementSpeed;
    private float localJumpForce;

    private float antiFriction;
    private float antiViscosity;
    // private float antiMovement;

    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerStatus = GetComponent<PlayerStatus>();
        playerController = GetComponent<CharacterController>();
    }

    void Update() {
        isGrounded = playerController.isGrounded;

        HandleCooldownTimers();

        antiFriction = 1f - friction;
        antiViscosity = 1f - viscosity;

        MovePlayer();
        RotatePlayer();
        MoveRelativeObjects();
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        // Debug.DrawRay(transform.position, hit.normal * 3, Color.green);
        // Debug.Log(hit.moveDirection);
        if (hit.moveDirection == Vector3.down) {
            // Debug.Log(newSurfaceContactPoint.y + " | " + hit.point.y);
            UpdateSurfaceData(hit.point);
        }
    }



    private void HandleCooldownTimers() {
        // Jumping
        if (jumpCooldownRemaining > 0f) jumpCooldownRemaining -= Time.deltaTime;
        if (jumpCooldownRemaining < 0f) jumpCooldownRemaining = 0f;
        
        // Other cooldown stuff goes here :)
    }

    private void MovePlayer() {
        switch (playerStatus.status) {
            case PlayerStatus.StatusType.Underwater:
                UpdateVelocityUnderwater();
                break;
            default:
                UpdateVelocityDefault();
                break;
        }

        playerController.Move(velocity * Time.deltaTime);
    }

    private void UpdateVelocityDefault() {
        Vector3 targetDirection = (Input.GetAxisRaw("Horizontal") * transform.right) + (Input.GetAxisRaw("Vertical") * transform.forward).normalized;
        directionalInput = Vector3.SmoothDamp(directionalInput, targetDirection, ref directionalVelocity, antiFriction);

        if (latestSurfaceNormalAngle <= playerController.slopeLimit) {
            localMovementSpeed = movementSpeed;

            // Horizontal basic movement
            velocity.x = directionalInput.x * localMovementSpeed;
            velocity.z = directionalInput.z * localMovementSpeed;

            // Vertical basic movement
            if (isGrounded) {
                if (Input.GetButton("Jump") && jumpCooldownRemaining == 0f) {
                    localJumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                    velocity.y = localJumpForce;
                    jumpCooldownRemaining = jumpCooldown;
                }
                else {
                    velocity.y = -localMovementSpeed;
                }
            }
            else {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
        }
        else {
            // Handles sliding mechanic off of steep surfaces -- balance of numbers is key here, any changes in movementSpeed, slideSpeed, or friction 
            // (current values: 16, 16, 0.875 -- TODO: figure out relationship formula between the three) will mess up the equilibrium
            localMovementSpeed = movementSpeed * antiFriction * antiFriction;

            // Horizontal sliding movement
            velocity.x = (slideDirection.x * slideSpeed) + (directionalInput.x * localMovementSpeed);
            velocity.z = (slideDirection.z * slideSpeed) + (directionalInput.z * localMovementSpeed);

            // Vertical sliding movement -- TODO: allow players to jump while sliding, under certain situations that don't allow them to counteract the slide
            if (isGrounded) {
                velocity.y = 0f;
            }
            else {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
        }
    }

    private void UpdateVelocityUnderwater() {
        localMovementSpeed = movementSpeed * antiViscosity;

        Vector3 targetDirection = (Input.GetAxisRaw("Horizontal") * transform.right) + (Input.GetAxisRaw("Vertical") * transform.forward).normalized;
        directionalInput = Vector3.SmoothDamp(directionalInput, targetDirection, ref directionalVelocity, antiViscosity);

        // Horizontal Movement
        velocity.x = directionalInput.x * localMovementSpeed;
        velocity.z = directionalInput.z * localMovementSpeed;

        // Vertical Movement
        if (Input.GetButton("Jump") && velocity.y >= -localMovementSpeed) {
            if (velocity.y < localMovementSpeed) {
                if (playerController.isGrounded) velocity.y = 0f;
                velocity.y -= (Physics.gravity.y * antiViscosity) * Time.deltaTime;
            }
            else {
                velocity.y = localMovementSpeed;
            }
        }
        else {
            if (velocity.y > -localMovementSpeed) {
                velocity.y += (Physics.gravity.y * antiViscosity) * Time.deltaTime;
                if (velocity.y < -localMovementSpeed) velocity.y = -localMovementSpeed;
            }
            else {
                velocity.y -= (Physics.gravity.y * movementSpeed * viscosity) * Time.deltaTime;
                if (velocity.y > -localMovementSpeed) velocity.y = -localMovementSpeed;
            }
        }
    }

    private void RotatePlayer() {
        yaw = ((yaw + Input.GetAxis("Mouse X") * sensitivity) + 360) % 360;
        pitch = Mathf.Clamp(pitch + Input.GetAxis("Mouse Y") * sensitivity, -45f, 90f);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        playerCamera.localEulerAngles = new Vector3(-pitch, 0f, 0f);
    }

    // Move objects whose positions must be relative to the player (i.e. stars & moon)
    private void MoveRelativeObjects() {
        foreach (Transform obj in relativeObjects) {
            obj.position = transform.position;
        }
    }

    private void UpdateSurfaceData(Vector3 newSurfaceContactPoint) {
        if (latestSurfaceContactPoint != newSurfaceContactPoint) {
            latestSurfaceContactPoint = newSurfaceContactPoint;

            newSurfaceContactPoint.y += playerController.skinWidth;
            Vector3 newSurfaceNormal = CalculateNormalBelow(newSurfaceContactPoint, playerController.height);
            float newNormalAngle = Vector3.Angle(Vector3.up, newSurfaceNormal);

            if (latestSurfaceNormal != newSurfaceNormal) {
                latestSurfaceNormal = newSurfaceNormal;
                UpdateSlideDirection();
            }

            if (latestSurfaceNormalAngle != newNormalAngle) {
                latestSurfaceNormalAngle = newNormalAngle;
            }
        }
    }

    private Vector3 CalculateNormalBelow(Vector3 position, float rayLength) {
        RaycastHit hit;

        Ray ray = new Ray(position, Vector3.down);
        Debug.DrawRay(position, Vector3.down * rayLength, Color.red);

        bool foundSurface = Physics.Raycast(ray, out hit, rayLength);
        // Debug.Log(foundSurface + " | " + Vector3.Angle(Vector3.up, hit.normal));

        return foundSurface ? hit.normal : Vector3.up;
    }

    private void UpdateSlideDirection() {
        slideDirection = Vector3.Cross(latestSurfaceNormal, Vector3.Cross(latestSurfaceNormal, Vector3.up));

        Debug.DrawRay(transform.position, slideDirection * 3, Color.green);
    }
}

// Vector3 targetDirection = (Input.GetAxisRaw("Horizontal") * transform.right) + (Input.GetAxisRaw("Vertical") * transform.forward).normalized;
// directionalInput = Vector3.SmoothDamp(directionalInput, targetDirection, ref directionalVelocity, antiFriction);

// if (latestSurfaceNormalAngle > playerController.slopeLimit) {
//     velocity.x = (slideDirection.x * slideSpeed) + (directionalInput.x * movementSpeed * antiMovement);
//     velocity.z = (slideDirection.z * slideSpeed) + (directionalInput.z * movementSpeed * antiMovement);
// }
// else {
//     velocity.x = directionalInput.x * movementSpeed;
//     velocity.z = directionalInput.z * movementSpeed;
// }

// velocity.x *= antiViscosity;
// velocity.z *= antiViscosity;

// float movementSpeedUnderwater = movementSpeed * antiViscosity;

// if (Input.GetButton("Jump") && velocity.y >= -movementSpeedUnderwater/* && jumpCooldownRemaining == 0f*/) {
//     // jumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
//     // float jumpForceUnderwater = jumpForce * antiViscosity;

//     // velocity.y = jumpForceUnderwater;
//     // jumpCooldownRemaining = jumpCooldown;
//     if (velocity.y < movementSpeedUnderwater) {
//         if (playerController.isGrounded) velocity.y = 0f;
//         velocity.y -= (Physics.gravity.y * antiViscosity) * Time.deltaTime;
//     }
//     else {
//         velocity.y = movementSpeedUnderwater;
//     }
// }
// else {
//     if (velocity.y > -movementSpeedUnderwater) {
//         velocity.y += (Physics.gravity.y * antiViscosity) * Time.deltaTime;
//         if (velocity.y < -movementSpeed0Underwater) velocity.y = -movementSpeedUnderwater;
//     }
//     else {
//         velocity.y -= (Physics.gravity.y * movementSpeed) * Time.deltaTime;
//         if (velocity.y > -movementSpeedUnderwater) velocity.y = -movementSpeedUnderwater;
//     }
// }
