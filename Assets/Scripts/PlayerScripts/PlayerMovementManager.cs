using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementManager : MonoBehaviour
{
    public static PlayerMovementManager Instance { get; private set; }

    public event EventHandler OnPlayerMoved;
    public event EventHandler OnPlayerStoppedMoving;

    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float playerRadius = 0.5f;

    private PlayerMovementActions playerMovementActions;
    private CharacterController characterController;
    private Camera mainCamera;

    private Vector2 inputMoveVector;

    private float playerHeight = 1.3f;
    private Vector3 velocity = new(0f, -2f, 0f);

    private bool canMove = true;

    private void Awake()
    {
        SetInstance();
        characterController = GetComponent<CharacterController>();
        playerMovementActions = new();
        playerMovementActions.PlayerMovementMap.Enable();
    }

    private void Start()
    {
        mainCamera = Camera.main;

        playerMovementActions.PlayerMovementMap.Move.performed += OnMovementPerformed;
        playerMovementActions.PlayerMovementMap.Move.canceled += OnMovementCancelled;
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState != GameState.Exploring)
        {
            OnPlayerStoppedMoving?.Invoke(this, null);
            return;
        }

        MovePlayer();
        ApplyGravity();
    }

    public void StopMovement()
    {
        canMove = false;
    }

    public void ResumeMovement()
    {
        canMove = true;
    }

    private void OnMovementPerformed(InputAction.CallbackContext context)
    {
        inputMoveVector = playerMovementActions.PlayerMovementMap.Move.ReadValue<Vector2>();
    }

    private void MovePlayer()
    {
        if (!canMove)
        {
            OnPlayerStoppedMoving?.Invoke(this, null);
            return;
        }

        float xMoveInput = inputMoveVector.x;
        float yMoveInput = inputMoveVector.y;

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 movementDir = (camForward * yMoveInput) + (camRight * xMoveInput);

        if (movementDir != Vector3.zero)
        {
            OnPlayerMoved?.Invoke(this, null);

            List<RaycastHit> hits = GetHitRaycasts(movementDir);
            bool isHit = hits.Count > 0;

            if (!isHit)
            {
                characterController.Move(Time.deltaTime * moveSpeed * movementDir);

                Quaternion targetRotation = Quaternion.LookRotation(movementDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            OnPlayerStoppedMoving?.Invoke(this, null);
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private List<RaycastHit> GetHitRaycasts(Vector3 movementDir)
    {
        RaycastHit[] raycastsHit = Physics.CapsuleCastAll(
            transform.position,
            transform.position + Vector3.up * playerHeight,
            playerRadius,
            movementDir,
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