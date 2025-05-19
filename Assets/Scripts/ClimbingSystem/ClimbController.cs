using UnityEngine;

public class ClimbController : MonoBehaviour
{
    PlayerController playerController;
    EnvironmentScanner environmentScanner;
    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || playerController.freeRun) && environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
        {
            Debug.Log("climbable ledge found");
        }
        // Debug.Log("No climbable ledge found");
    }
}
