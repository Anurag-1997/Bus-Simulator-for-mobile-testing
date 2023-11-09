using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;



public class StairClimber : MonoBehaviour
{
    [SerializeField] Animator NPC_anim;
    [SerializeField] NavMeshAgent NPC_agent;
    [SerializeField] Transform busEntryPosition;
    [SerializeField] Transform busLookAtPosition;
    [SerializeField] Transform busStep1;
    [SerializeField] Transform busStep2;
    [SerializeField] Transform playerBus;
    private bool climbed = false;
    private float remainingDist;
    const float yPosition = 0.928f;

    private void Start()
    {
        
        
    }
    private void Update()
    {
        MoveToBus();
    }
    private void MoveToBus()
    {
        if(BusStop.busWaiting)
        {
            if(NPC_agent.enabled == true)
            {
                NPC_agent.SetDestination(busEntryPosition.position);
                remainingDist = NPC_agent.remainingDistance;
            }
            
            if(remainingDist <= 2f)
            {

                if(climbed == false)
                {
                    Transform busLookAt = new GameObject().transform;
                    Debug.Log("busLook_before " + busLookAtPosition.position.y);
                    busLookAt.position = new Vector3(busLookAtPosition.position.x, transform.position.y, busLookAtPosition.position.z);
                    Debug.Log("busLook_after " + busLookAtPosition.position.y);
                    transform.LookAt(busLookAt);
                    StartCoroutine(ClimbStairs());
                }
                
               
            }
        }
    }

    IEnumerator ClimbStairs()
    {
        //Time.timeScale = 0.2f;
        transform.parent = playerBus;
        NPC_agent.enabled = false;
        NPC_anim.SetBool("Climbing", true);
        yield return new WaitForSeconds(0.33f);
        transform.DOLocalMove(busStep1.localPosition , 0.33f);
        yield return new WaitForSeconds(0.66f);
        transform.DOLocalMove(busStep2.localPosition , 0.33f);
        yield return new WaitForSeconds(0.66f);
        transform.DOLocalMove(busLookAtPosition.localPosition , 0.33f);
        yield return new WaitForSeconds(0.33f);
        NPC_anim.SetBool("Climbing", false);
        climbed = true;

    }

    

}
