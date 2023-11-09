using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BusDestination : MonoBehaviour
{
    [SerializeField] private List<Transform> destinations = new List<Transform>();
    [SerializeField] private Transform firstStop;
    [SerializeField] private Transform lastStop;
    private Transform currentDestination;
    [SerializeField] TMP_Text destinationDistanceText;

    private void Start()
    {
        currentDestination = firstStop;
    }
    private void Update()
    {
        for(int i = 0; i < destinations.Count; i++)
        {
            if (Vector3.Distance(transform.position, destinations[i].position)<=10f && destinations[i]!=lastStop)
            {
                StartCoroutine(peoplePickUpDelay(i));
            }
        }
        destinationDistanceText.text = "Next Stop in : "+ Vector3.Distance(transform.position, currentDestination.position) + " meters";
        Debug.Log("current destination distance : " + Vector3.Distance(transform.position, currentDestination.position));
        
    }

    IEnumerator peoplePickUpDelay(int i)
    {
        yield return new WaitForSeconds(5f);
        currentDestination = destinations[i + 1];
    }
}
