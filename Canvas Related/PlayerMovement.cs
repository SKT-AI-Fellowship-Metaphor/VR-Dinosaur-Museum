using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
 
    public float speed = 12f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
 
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
 
    Vector3 velocity;
 
    bool isGrounded;


    // PositionDisplay 컴포넌트 참조 추가
    private PositionDisplay positionDisplay;
 
    private void Start()
    {
        // PositionDisplay 컴포넌트 찾기
        positionDisplay = FindObjectOfType<PositionDisplay>();
        
        // PositionDisplay가 존재하면 플레이어 Transform 설정
        if (positionDisplay != null)
        {
            positionDisplay.playerTransform = this.transform;
        }
        else
        {
            Debug.LogWarning("PositionDisplay component not found in the scene.");
        }
    }

 
    // Update is called once per frame
    void Update()
    {
        // Input Manager
        if (!InputManager.Instance.IsInputAllowed())
        {
            return;
        }

        //checking if we hit the ground to reset our falling velocity, otherwise we will fall faster the next time
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
 
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
 
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
 
        //right is the red Axis, foward is the blue axis
        Vector3 move = transform.right * x + transform.forward * z;
 
        controller.Move(move * speed * Time.deltaTime);
 
        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //the equation for jumping
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
 
        velocity.y += gravity * Time.deltaTime;
        velocity.y = Mathf.Max(velocity.y, -40f); // 최대 낙하 속도 제한
 
        controller.Move(velocity * Time.deltaTime);
    }
}