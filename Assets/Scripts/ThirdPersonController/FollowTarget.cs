using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float bottomClamp = -40f;
    [SerializeField] private float topClamp = 70f;

    private float cinemachineTargetPitch;
    private float cinemachineTargetYaw;
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void LateUpdate()
    {
        CameraLogic();
    }
    private void CameraLogic()
    {
        float mouseX = GetMouseInput("Mouse X");
        float mouseY = GetMouseInput("Mouse Y");

        cinemachineTargetPitch = UpdateRotation(cinemachineTargetPitch, mouseY, bottomClamp, topClamp, true);
        cinemachineTargetYaw = UpdateRotation(cinemachineTargetYaw, mouseX, float.MinValue, float.MaxValue, false);

        ApplyRotation(cinemachineTargetPitch, cinemachineTargetYaw);
    }
    private void ApplyRotation(float Pitch, float Yaw)
    {
        followTarget.rotation = Quaternion.Euler(Pitch, Yaw, followTarget.eulerAngles.z);   
    }
    private float UpdateRotation(float currentRotation, float input, float min, float max, bool isXaxis)
    {
        currentRotation += isXaxis ? -input : input;
        return Mathf.Clamp(currentRotation, min, max);

    }
    private float GetMouseInput(string axis)
    {
        return Input.GetAxis(axis) * rotationSpeed * Time.deltaTime;
    }
}
