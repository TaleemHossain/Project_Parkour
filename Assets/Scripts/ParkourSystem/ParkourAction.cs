using UnityEngine;
[CreateAssetMenu(menuName = "ParkourSystem/ParkourAction")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] string animName;
    [SerializeField] string obstacleTag;
    [SerializeField] bool obstacleRequired;
    [SerializeField] bool rotateToObstacle;
    [SerializeField] float minHeight;
    [SerializeField] float maxHeight;
    [SerializeField] float postActionDelay = 0f;
    [Header("Target Matching")]
    [SerializeField] bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] float matchStartTime;
    [SerializeField] float matchTargetTime;
    [SerializeField] Vector3 matchPosWeight = new Vector3(0, 1, 0);
    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPosition { get; set; }
    public bool Mirror { get; set; }
    public virtual bool CheckIfPossible(EnvironmentScanner.ObstackleHitData hitDataInfo, Transform player)
    {
        if (!hitDataInfo.forwardHitFound)
        {
            if (!obstacleRequired)
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
            if (!obstacleRequired)
            {
                return false;
            }
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
            if (enableTargetMatching)
            {
                MatchPosition = hitDataInfo.heightHitInfo.point;
            }
            return true;
        }
    }
    public string AnimName => animName;
    public bool RotateToObstackle => rotateToObstacle;
    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyPart => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
    public float PostActionDelay => postActionDelay;
}