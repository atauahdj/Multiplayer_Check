using Mirror;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Monster_Script : NetworkBehaviour
{
    public Transform first_point, second_point;
    public Vector3 current_point;
    public GameObject[] allPlayers;
    public GameObject player;
    public NavMeshAgent agent;
    bool isChasing;
    NavMeshPath path;
    Vector3 LastPos;
    float remember_player;
    public GameObject[] Doors;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        current_point.x = UnityEngine.Random.Range(first_point.position.x, second_point.position.x + 1);
        current_point.z = UnityEngine.Random.Range(first_point.position.z, second_point.position.z + 1);
        path = new NavMeshPath();
        agent.CalculatePath(current_point, path);
    }

    float time;
    float stop_time;
    
    void Update()
    {
        #region CheckDistance
        if (time < 0.26f) time += Time.deltaTime;
        if (time >= 0.25f)
        {
            if(allPlayers.Length != GameObject.FindGameObjectsWithTag("Player").Length)
            {
                allPlayers = GameObject.FindGameObjectsWithTag("Player");
            }
            if(allPlayers.Length >= 2)
            {
                float minDist = Mathf.Infinity;
                for (int i = 0; i < allPlayers.Length; i++)
                {
                    if (allPlayers[i] == null) continue;
                    float Dist = Vector3.Distance(transform.position, allPlayers[i].transform.position);
                    if(Dist < minDist)
                    {
                        minDist = Dist;
                        player = allPlayers[i];
                    }
                }
            }
            else
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
        }
        if(player != null)
        {
            Vector3 direction = player.transform.position - transform.position;
            Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), direction.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit.collider.name);
                Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), direction, Color.red);
                if(hit.collider.tag == "Player")
                {
                    isChasing = true;
                }
                else
                {
                    isChasing = false;
                }
            }
        }
        #endregion

        if (isChasing &&  player != null && Vector3.Distance(transform.position, player.transform.position) <= 35f)
        {
            Chase();
        }
        else
        {
            Patrol();
        }
        if(Vector3.Distance(Doors[UnityEngine.Random.Range(0, Doors.Length)].transform.position, transform.position) <= 2f)
        {
            Doors[UnityEngine.Random.Range(0, Doors.Length)].GetComponent<Open_Door_Script>().Open();
        }
    }
    void Chase()
    {
        agent.isStopped = false;
        remember_player = 0.1f;
        if (player != null) agent.destination = player.transform.position;
        if(Vector3.Distance(transform.position, player.transform.position) < 3f)
        {
            Attack();
        }
    }
    bool need_check;
    void Patrol()
    {
        if(remember_player >= 0 && player != null)
        {
            remember_player -= Time.deltaTime;
            LastPos = player.transform.position;
            need_check = true;
        }
        if (need_check)
        {
            agent.destination = LastPos;
            if(Vector3.Distance(transform.position, LastPos) <= 3f)
            {
                need_check = false;
            }
        }
        else
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                agent.destination = current_point;
                if (Vector3.Distance(transform.position, current_point) <= 1f)
                {
                    stop_time += Time.deltaTime;
                    agent.isStopped = true;
                    if (stop_time > 5f)
                    {
                        current_point.x = UnityEngine.Random.Range(first_point.position.x, second_point.position.x + 1);
                        current_point.z = UnityEngine.Random.Range(first_point.position.z, second_point.position.z + 1);
                        agent.isStopped = false;
                        stop_time = 0;
                        agent.CalculatePath(current_point, path);
                    }
                }
            }
            else
            {
                current_point.x = UnityEngine.Random.Range(first_point.position.x, second_point.position.x + 1);
                current_point.z = UnityEngine.Random.Range(first_point.position.z, second_point.position.z + 1);
                agent.isStopped = false;
                agent.CalculatePath(current_point, path);
            }
        }
    }
    void Attack()
    {
        if (!isServer) return;
        PlayerController pl = player.GetComponent<PlayerController>();
        pl.Die();
        isChasing = false;
    }
}
