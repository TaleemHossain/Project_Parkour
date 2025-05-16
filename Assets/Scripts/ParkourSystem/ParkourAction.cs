using UnityEngine;
[CreateAssetMenu(menuName = "ParkourSystem/ParkourAction")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] bool ObstacleRequired;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    public bool CheckIfPossible(EnvironmentScanner.ObstackleHitData hitDataInfo, Transform player)
    {
        if (!hitDataInfo.forwardHitFound)
        {
            if (!ObstacleRequired)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            if (!ObstacleRequired)
            {
                return false;
            }
            float height = hitDataInfo.heightHitInfo.point.y - player.position.y;
            if (height < minHeight || height > maxHeight)
            {
                return false;
            }
            return true;
        }
    }
    public string AnimName => animName;
}
