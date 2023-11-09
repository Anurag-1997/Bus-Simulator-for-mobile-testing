using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Controller : MonoBehaviour
{
    [SerializeField] NavMeshAgent NPC_Agent;
    [SerializeField] Animator NPC_anim;
    [SerializeField] GameObject path;
    private Transform[] pathPoints;
    private int index = 0;
    [SerializeField] private float minDistance = 5f;

    private void Start()
    {
        NPC_Agent = GetComponent<NavMeshAgent>();
        NPC_anim = GetComponent<Animator>();
        pathPoints = new Transform[path.transform.childCount];
        for(int i = 0; i < pathPoints.Length; i++)
        {
            pathPoints[i] = path.transform.GetChild(i);
        }
    }

    private void Update()
    {
        Roam();
    }

    void Roam()
    {
        if (Vector3.Distance(transform.position, pathPoints[index].position)<minDistance)
        {
            if (index + 1 != pathPoints.Length)
            {
                index++;
            }
            else
            {
                index = 0;
            }
        }
        NPC_Agent.SetDestination(pathPoints[index].position);
    }
}
