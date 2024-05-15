using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions playerInputBindings;
    private PlayerController playerController;

    private InputAction moveCam;
    private InputAction mouseRight;
    private InputAction mouseScroll;

    private Vector2 moveVector;
    private bool isMoving = false;


    private void Awake()
    {
        playerInputBindings = new PlayerInputActions();

        moveCam = playerInputBindings.PC.CameraMovement;
        moveCam.performed += OnStartMove;
        moveCam.canceled += OnStopMove;

        mouseRight = playerInputBindings.PC.MouseRight;
        mouseRight.started += OnMouseRightPressed;
        mouseRight.canceled += OnMouseRightReleased;

        mouseScroll = playerInputBindings.PC.MouseScroll;
        mouseScroll.performed += OnMouseScroll;
        mouseScroll.canceled += OnMouseScrollStop;
    }

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        moveCam.Enable();
        mouseRight.Enable();
        mouseScroll.Enable();
    }

    private void OnDisble()
    {
        moveCam.Disable();
        mouseRight.Disable();
        mouseScroll.Disable();
    }

    private void Update()
    {
        if(isMoving)
        {
            //Debug.Log("Cam moved: " + moveVector);

            playerController.UpdateMoveVector(moveVector);
        }
    }

    private void OnStartMove(InputAction.CallbackContext context)
    {
        moveVector = context.ReadValue<Vector2>();
        isMoving = true;
    }

    private void OnStopMove(InputAction.CallbackContext context)
    {
        moveVector = Vector2.zero;
        playerController.UpdateMoveVector(moveVector);
        isMoving = false;
    }

    private void OnMouseRightPressed(InputAction.CallbackContext context)
    {
        //Debug.Log("Mouse pressed");

        playerController.MouseRightPressed();
    }

    private void OnMouseRightReleased(InputAction.CallbackContext context)
    {
        //Debug.Log("Mouse released");

        playerController.MouseRightReleased();
    }

    private void OnMouseScroll(InputAction.CallbackContext context)
    {
        //Debug.Log("MouseScroll: " + context.ReadValue<Vector2>());
        playerController.MouseScroll(context.ReadValue<Vector2>().y/120f);
    }

    private void OnMouseScrollStop(InputAction.CallbackContext context)
    {
        playerController.MouseScroll(0f);
    }
}
