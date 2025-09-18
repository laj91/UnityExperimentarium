using UnityEngine;

public class CannonPivotController : MonoBehaviour
{
    [SerializeField] float maxX = 45f;
    [SerializeField] float minX = -10f;
    [SerializeField] float maxY = 60f;
    [SerializeField] float minY = -60f;

    private float currentX = 0f;
    private float currentY = 0f;

    public void RotatePivot(float deltaX, float deltaY)
    {
        currentX = Mathf.Clamp(currentX + deltaX, minX, maxX);
        currentY = Mathf.Clamp(currentY + deltaY, minY, maxY);
        transform.localEulerAngles = new Vector3(currentX, currentY, 0f);
    }
}