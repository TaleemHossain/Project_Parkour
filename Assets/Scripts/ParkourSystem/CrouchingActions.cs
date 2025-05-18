using UnityEngine;
[CreateAssetMenu(menuName = "ParkourSystem/CrouchAction")]
public class CrouchingActions : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] string obstacleTag;
    [SerializeField] bool rotateToObstacle;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    public Quaternion TargetRotation { get; set; }
    public virtual bool CheckIfPossible2(EnvironmentScanner.BarrierHitData hitDataInfo, Transform player)
    {
        if (hitDataInfo.forwardHitFound)
        {
            if (string.IsNullOrEmpty(obstacleTag))
            {
                return false;
            }
            if (hitDataInfo.forwardHitInfo.transform.tag != obstacleTag)
            {
                return false;
            }
            float height = hitDataInfo.heightHitInfo.point.y - player.position.y;
            if (height < minHeight || height > maxHeight)
            {
                return false;
            }
            if (rotateToObstacle)
            {
                TargetRotation = Quaternion.LookRotation(-hitDataInfo.forwardHitInfo.normal);
            }
            return true;
        }
        return false;
    }
    public string AnimName => animName;
    public bool RotateToObstackle => rotateToObstacle;
}
