using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open_Door_Script : NetworkBehaviour
{
    Animator anim;
    bool isClose = true;
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.SetBool("Close", isClose);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    [Command(requiresAuthority = false)]
    public void Open()
    {
        isClose = !isClose;
        Change(isClose);
    }
    [ClientRpc]
    public void Change(bool statusAnim)
    {
        anim.SetBool("Close", isClose);
    }
}
