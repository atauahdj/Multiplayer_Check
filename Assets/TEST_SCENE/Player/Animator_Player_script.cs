using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Animator_Player_script : NetworkBehaviour
{
    public Animator cam;
    public Animator player_animator;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //bool Walk = player_animator.GetBool("Walk");
        bool isWalking_Forward = Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed || Keyboard.current.dKey.isPressed;
        bool isWalking_Backward = Keyboard.current.sKey.isPressed;
        bool isRunning_Forward = Keyboard.current.leftShiftKey.isPressed;
        bool isRunning_Backward = Keyboard.current.leftShiftKey.isPressed && isWalking_Backward;
        if (!isLocalPlayer) return;
        cam.SetBool("Move", (isWalking_Forward || isRunning_Backward || isWalking_Backward || isRunning_Forward));
        if (isRunning_Backward || isRunning_Forward) cam.speed = 2;
        else cam.speed = 1;
        player_animator.SetBool("Run", isRunning_Forward);
        player_animator.SetBool("Walk", isWalking_Forward);
        player_animator.SetBool("Run_Backward", isRunning_Backward);
        player_animator.SetBool("Walk_Backward", isWalking_Backward);
    }
}
