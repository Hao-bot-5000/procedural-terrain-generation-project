using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float movementSpeed = 16f;
    public float jumpHeight = 4f;
    public float slideSpeed = 16f;
    [Range (0, 1)]
    public float friction = 0.875f;

    public float jumpCooldown = 1f;

    public Transform perspective;
    public float sensitivity = 2f;

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

    void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        playerController = GetComponent<CharacterController>();
    }

    void Update() {
        isGrounded = playerController.isGrounded;
        HandleCooldownTimers();

        MovePlayer();
        RotatePlayer();
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

    [HideInInspector] private float antiFriction;
    private void MovePlayer() {
        antiFriction = 1f - friction;

        UpdateHorizontalMovement();
        UpdateVerticalMovement();

        playerController.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayer() {
        yaw = ((yaw + Input.GetAxis("Mouse X") * sensitivity) + 360) % 360;
        pitch = Mathf.Clamp(pitch + Input.GetAxis("Mouse Y") * sensitivity, -45f, 90f);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        perspective.localEulerAngles = new Vector3(-pitch, 0f, 0f);
    }

    [HideInInspector] private Vector3 directionalVelocity;
    [HideInInspector] private float slidingAntiFriction;
    private void UpdateHorizontalMovement() {
        Vector3 targetDirection = (Input.GetAxisRaw("Horizontal") * transform.right) + (Input.GetAxisRaw("Vertical") * transform.forward).normalized;
        directionalInput = Vector3.SmoothDamp(directionalInput, targetDirection, ref directionalVelocity, antiFriction);

        /* Handles sliding mechanic off of steep surfaces -- balance of numbers is key here, any changes in movementSpeed, slideSpeed, or friction 
         * (current values: 16, 16, 0.875 -- TODO: figure out relationship formula between the three) will mess up the equilibrium
         */
        if (latestSurfaceNormalAngle > playerController.slopeLimit) {
            slidingAntiFriction = antiFriction * 0.125f;

            velocity.x = (slideDirection.x * slideSpeed) + (directionalInput.x * movementSpeed * slidingAntiFriction);
            velocity.z = (slideDirection.z * slideSpeed) + (directionalInput.z * movementSpeed * slidingAntiFriction);
        }
        else {
            velocity.x = directionalInput.x * movementSpeed;
            velocity.z = directionalInput.z * movementSpeed;
        }
    }

    [HideInInspector] private float jumpForce;
    private void UpdateVerticalMovement() {
        // TODO: allow players to jump while sliding under certain circumstances
        if (latestSurfaceNormalAngle > playerController.slopeLimit) {
            if (isGrounded) velocity.y = 0f;
            else velocity.y += Physics.gravity.y * Time.deltaTime;
        }
        else {
            if (isGrounded) {
                if (Input.GetButton("Jump") && jumpCooldownRemaining == 0f) {
                    jumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                    velocity.y = jumpForce;
                    jumpCooldownRemaining = jumpCooldown;
                }
                else {
                    velocity.y = -movementSpeed;
                }
            }
            else {
                velocity.y += Physics.gravity.y * Time.deltaTime;
            }
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

    private void UpdateSlideDirection() {
        slideDirection = Vector3.Cross(latestSurfaceNormal, Vector3.Cross(latestSurfaceNormal, Vector3.up));

        Debug.DrawRay(transform.position, slideDirection * 3, Color.green);
    }

    private Vector3 CalculateNormalBelow(Vector3 position, float rayLength) {
        RaycastHit hit;

        Ray ray = new Ray(position, Vector3.down);
        Debug.DrawRay(position, Vector3.down * rayLength, Color.red);

        bool foundSurface = Physics.Raycast(ray, out hit, rayLength);
        // Debug.Log(foundSurface + " | " + Vector3.Angle(Vector3.up, hit.normal));

        return foundSurface ? hit.normal : Vector3.up;
    }
}
