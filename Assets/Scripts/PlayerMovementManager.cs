using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    public static PlayerMovementManager Instance { get; private set; }

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float playerRadius = 0.5f;

    private PlayerMovementActions playerMovementActions;
    private CharacterController characterController;

    private Vector2 inputMoveVector;
    
    private float playerHeight = 1.3f;
    private Vector3 velocity = new(0f, -2f, 0f); 

    private void Awake()
    {
        SetInstance();
        characterController = GetComponent<CharacterController>();
        playerMovementActions = new();
        playerMovementActions.PlayerMovementMap.Enable();
    }

    private void Start()
    {
        playerMovementActions.PlayerMovementMap.Move.performed += OnMovementPerformed;
        playerMovementActions.PlayerMovementMap.Move.canceled += OnMovementCancelled;
    }

    void Update()
    {
        Debug.Log("is grounded:" + characterController.isGrounded);
        MovePlayer();
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        inputMoveVector = playerMovementActions.PlayerMovementMap.Move.ReadValue<Vector2>();
    }

    private void MovePlayer()
    {
        float xMoveInput = inputMoveVector.x;
        float yMoveInput = inputMoveVector.y;

        Vector3 movementInput = new(xMoveInput, 0f, yMoveInput);

        if (movementInput != Vector3.zero)
        {
            List<RaycastHit> hits = GetHitRaycasts(movementInput);
            bool isHit = hits.Count > 0;

            if (!isHit)
            {
                characterController.Move(Time.deltaTime * moveSpeed * movementInput);
                
                if (characterController.isGrounded && velocity.y < 0)
                {
                    velocity.y = -2f;
                }

                velocity.y += gravity * Time.deltaTime;
                characterController.Move(velocity * Time.deltaTime);
            }
        }
    }

    private List<RaycastHit> GetHitRaycasts(Vector3 movementInput)
    {
        RaycastHit[] raycastsHit = Physics.CapsuleCastAll(
            transform.position,
            transform.position + Vector3.up * playerHeight,
            playerRadius,
            movementInput,
            0.1f
        );

        List<RaycastHit> filteredRaycastsHit = new();

        foreach (RaycastHit raycastHit in raycastsHit)
        {
            if (IsColliderBlockingMovements(raycastHit.collider))
                filteredRaycastsHit.Add(raycastHit);
        }

        return filteredRaycastsHit;
    }

    private bool IsColliderBlockingMovements(Collider collider) 
    {
        //to be written later
        return false;
    }

    private void OnMovementCancelled(InputAction.CallbackContext context)
    {
        inputMoveVector = Vector3.zero;
    }

    private void SetInstance()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
}
