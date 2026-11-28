using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Animtor_Monster : NetworkBehaviour
{
    Animator animator;
    public NavMeshAgent agent;
    public AudioSource audio_monster;
    bool isMoving;
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        audio_monster = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        if (agent == null) return;

        // 1. Проверяем, движется ли монстр
        bool currentlyMoving = agent.velocity.sqrMagnitude > 0.1f;

        // 2. Если состояние изменилось (был на месте -> пошел, или наоборот)
        if (currentlyMoving != isMoving)
        {
            isMoving = currentlyMoving;

            // Меняем анимацию (NetworkAnimator сам синхронизирует SetBool, 
            // если он у тебя есть. Если нет - нужен SyncVar или ClientRpc)
            animator.SetBool("Move", isMoving);
            if (isMoving) animator.speed = 1.25f;

            // Вызываем RPC ТОЛЬКО ОДИН РАЗ при смене состояния!
            SoundPlay(isMoving);
        }
    }
    [Server]
    void ChangeAnim()
    {
        if (agent == null) return;
        if (agent.velocity == Vector3.zero)
        {
            animator.SetBool("Move", false);
        }
        else
        {
            animator.SetBool("Move", true);
            animator.speed = 1.25f;
        }
    }

    [ClientRpc]
    void SoundPlay(bool playSound)
    {
        if (playSound)
        {
            if (!audio_monster.isPlaying)
            {
                audio_monster.Play();
            }
        }
        else
        { 
            if (audio_monster.isPlaying)
            {
                audio_monster.Stop();
            }
        }
    }
}
