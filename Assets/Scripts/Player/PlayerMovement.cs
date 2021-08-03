using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    public float movementSpeed = 16f;
    public float jumpHeight = 4f;
    public float slideSpeed = 25f;

    public Transform perspective;
    public float sensitivity = 2f;

    public int chunkLayer;

    CharacterController playerController;
    Vector3 velocity;
    Vector3 surfaceNormal;

    GameObject currentChunk;

    float yaw = 0f;
    float pitch = 0f;

    void Start() {
        playerController = GetComponent<CharacterController>();
        velocity = Vector3.zero;
        surfaceNormal = Vector3.up;

        // currentChunk = GetCurrentChunk();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // Debug.Log("Grounded?: " + playerController.isGrounded);
        MovePlayer();
        RotatePlayer();
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        if (hit.moveDirection == Vector3.down) {
            // Point on player controller where surface contacts player
            Vector3 surfaceContactPoint = hit.point;
            surfaceContactPoint.y += playerController.skinWidth;
            surfaceNormal = CalculateSurfaceNormal(surfaceContactPoint);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == chunkLayer) {
            currentChunk = other.gameObject;
        }
    }

    private void MovePlayer() {
        Vector3 localXMovement = Input.GetAxis("Horizontal") * transform.right * movementSpeed;
        Vector3 localYMovement = CalculateVerticalMovement();
        Vector3 localZMovement = Input.GetAxis("Vertical") * transform.forward * movementSpeed;

        velocity = localXMovement + localYMovement + localZMovement;

        /* 
         * FIXME: 
         *      1. fix jittery sliding when colliding with multiple planes of differing normals
         *      2. (FIXED!! yay.. i think..) fix jittery sliding when sliding down VERY STEEP slopes (isGrounded becomes false)
         */
        if (playerController.isGrounded) {
            float normalAngle = Vector3.Angle(transform.up, surfaceNormal);
            // Debug.Log(normalAngle);

            if (normalAngle > playerController.slopeLimit) {
                velocity.x += (1f - surfaceNormal.y) * surfaceNormal.x * slideSpeed;
                velocity.z += (1f - surfaceNormal.y) * surfaceNormal.z * slideSpeed;
            }
        }

        playerController.Move(velocity * Time.deltaTime);
    }

    private void RotatePlayer() {
        yaw += Input.GetAxis("Mouse X") * sensitivity;
        pitch -= Input.GetAxis("Mouse Y") * sensitivity;
        
        float maxUpAngle = -90f;
        float maxDownAngle = 45f;
        pitch = Mathf.Clamp(pitch, maxUpAngle, maxDownAngle);

        transform.localEulerAngles = new Vector3(0f, yaw, 0f);
        perspective.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private Vector3 CalculateVerticalMovement() {
        // https://forum.unity.com/threads/controller-isgrounded-doesnt-work-reliably.91436/#post-592670
        // TODO: Figure out why grounded Y-velocity is set to -stepOffset / deltaTime?
        if (playerController.isGrounded) {
            if (Input.GetButtonDown("Jump")) {
                float jumpForce = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                velocity.y = jumpForce;
            }
            else {
                velocity.y = Physics.gravity.y * 4; // -playerController.stepOffset / Time.deltaTime;
            }
        }
        else {
            velocity.y += Physics.gravity.y * Time.deltaTime;
        }

        return transform.up * velocity.y;
    }

    private Vector3 CalculateSurfaceNormal(Vector3 surfaceContactPoint) {
        RaycastHit hit;

        Ray ray = new Ray(surfaceContactPoint, -transform.up);
        float rayLength = playerController.skinWidth + 0.1f;

        bool surfaceDetected = Physics.Raycast(ray, out hit, rayLength);
        Debug.DrawRay(ray.origin, ray.direction * rayLength, surfaceDetected ? Color.green : Color.red);
        // Debug.Log("Surface contact point: " + surfaceContactPoint + " | " + surfaceDetected);

        // If no surface detected, return vector that results in 0-degree angle
        return surfaceDetected ? hit.normal : transform.up;
    }
}
