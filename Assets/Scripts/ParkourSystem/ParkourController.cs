using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    EnvironmentScanner environmentScanner;
    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        var hitData = environmentScanner.ObstackleCheck();
        // if (hitData.forwardHitFound)
        // {
        //     Debug.Log("Obstacle Found: " + hitData.forwardHitInfo.collider.name + " at " + hitData.forwardHitInfo.point);
        // }
        // else
        // {
        //     Debug.Log("No Obstacle Found");
        // }
    }   
}
