using UnityEngine;

public class ClimbPointContainer : MonoBehaviour
{
    public ClimbPoint GetClimbPoint(Vector3 playerPosition)
    {
        ClimbPoint bestPoint = null;
        if (transform.childCount == 0)
        {
            return bestPoint;
        }
        float minDistance = 2.16f;

        foreach (Transform child in transform)
        {
            if (child.GetComponent<ClimbPoint>() == null)
            {
                continue;
            }
            if (Vector3.Distance(child.position, playerPosition) < minDistance)
            {
                bestPoint = child.GetComponent<ClimbPoint>();
                minDistance = Vector3.Distance(child.position, playerPosition);
            }
        }
        return bestPoint;
    }
}
