using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStop : MonoBehaviour
{
    [HideInInspector] public static bool atBusStop = false;
    [HideInInspector] public static bool busWaiting = false;
    [SerializeField] SkinnedMeshRenderer busFrontGate;
    [SerializeField] SkinnedMeshRenderer busBackGate;
    [SerializeField] Animator Bus_Anim;
    [SerializeField] BusController busController;
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Bus")
        {
            atBusStop = true;
            Debug.Log("AT bus stop : " +atBusStop);

        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Bus")
        {
            atBusStop = true;
            Debug.Log("AT bus stop : " + atBusStop);
            Debug.Log("Front gate Blend Shape : " + busFrontGate.GetBlendShapeWeight(0));
            Debug.Log("Back gate Blend Shape : " + busBackGate.GetBlendShapeWeight(0));
            if(busController.currentSpeed < 4f)
            {
                busWaiting = true;
                Debug.Log("Bus waiting bool : " + busWaiting);
                Bus_Anim.SetBool("GatesOpen", true);
                Bus_Anim.SetBool("GatesClose", false);
            }
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Bus")
        {
            atBusStop = false;
            Bus_Anim.SetBool("GatesClose", true);
            Bus_Anim.SetBool("GatesOpen", false);
            StartCoroutine(busGatesCloseDelay());

        }
    }

    IEnumerator busGatesCloseDelay()
    {
        yield return new WaitForSeconds(2.5f);
        Bus_Anim.SetBool("GatesOpen", false);
        Bus_Anim.SetBool("GatesClose", false);
    }
}
