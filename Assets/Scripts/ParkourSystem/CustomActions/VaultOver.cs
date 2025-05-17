using Unity.Mathematics;
using UnityEngine;
[CreateAssetMenu(menuName = "ParkourSystem/CustomAction/VaultOver")]
public class VaultOver : ParkourAction
{
    public override bool CheckIfPossible(EnvironmentScanner.ObstackleHitData hitDataInfo, Transform player)
    {
        if (!base.CheckIfPossible(hitDataInfo, player))
        {
            return false;
        }
        var hitPoint = hitDataInfo.forwardHitInfo.transform.InverseTransformPoint(hitDataInfo.forwardHitInfo.point);
        if (hitPoint.z < 0 && hitPoint.x < 0 || hitPoint.z > 0 && hitPoint.x > 0) // 
        {
            Mirror = true;
            matchBodyPart = AvatarTarget.RightHand;
        }
        else
        {
            Mirror = false;
            matchBodyPart = AvatarTarget.LeftHand;
        }
        return true;
    }
}
