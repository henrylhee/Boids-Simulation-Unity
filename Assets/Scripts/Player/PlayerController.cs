using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float startSpeed = 5;
    private float speedMulti = 1;
    [SerializeField]
    private float scrollDeltaSpeed = 0.04f;

    [SerializeField]
    private float rotateSpeed = 20f;

    private Vector2 moveVector;
    private bool mouseRightPressed;

    private Vector3 mousePosition;


    private void Update()
    {
        Vector3 allignedMoveVector = transform.right * moveVector.x + transform.forward * moveVector.y;
        transform.position = transform.position + allignedMoveVector * startSpeed * speedMulti * Time.deltaTime;

        if(mouseRightPressed)
        {
            Vector3 mousePosDelta = (mousePosition - Input.mousePosition).normalized * rotateSpeed * Time.deltaTime;
            transform.Rotate(new Vector3(mousePosDelta.y, -mousePosDelta.x, 0f));
            mousePosition = Input.mousePosition;
        }
    }

    public void UpdateMoveVector(Vector2 moveVector)
    {
        this.moveVector = moveVector;
    }

    public void MouseRightPressed()
    {
        mouseRightPressed = true;
        mousePosition = Input.mousePosition;
    }

    public void MouseRightReleased()
    {
        mouseRightPressed = false;
    }

    public void MouseScroll(float value)
    {
        if (mouseRightPressed)
        {
            Mathf.Clamp(speedMulti += value * scrollDeltaSpeed, 0f, 10f);
        }
    }
}
