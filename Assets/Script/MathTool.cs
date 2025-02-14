using UnityEngine;

public static class MathTool
{
    // Rotate on the y-axis
    public static float GetAngelIn3D(Vector3 direction)
    {
        return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
    }

    public static Vector3 GetVectorFromYAxis(float angle)
    {
        float radian = angle * Mathf.Deg2Rad; // Convert degrees to radians
        return new Vector3(Mathf.Sin(radian), 0, Mathf.Cos(radian));
    }
}
