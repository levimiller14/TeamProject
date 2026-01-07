using UnityEngine;

public enum connectionDirection
{
    North,  // +Z
    South,  // -Z
    East,   // +X
    West    // -X
}

public class roomConnectionPoint : MonoBehaviour
{
    [Header("----Connection Info----")]
    public connectionDirection direction;

    [Header("----State----")]
    public bool isConnected;
    public roomConnectionPoint connectedTo;

    [Header("----Debug Gizmos----")]
    [SerializeField] float gizmoSize = 0.5f;
    [SerializeField] Color openColor = Color.green;
    [SerializeField] Color connectedColor = Color.red;

    public Vector3 getWorldPosition() => transform.position;
    public Vector3 getWorldDirection() => transform.forward;

    public static connectionDirection getOpposite(connectionDirection dir)
    {
        return dir switch
        {
            connectionDirection.North => connectionDirection.South,
            connectionDirection.South => connectionDirection.North,
            connectionDirection.East => connectionDirection.West,
            connectionDirection.West => connectionDirection.East,
            _ => connectionDirection.North
        };
    }

    public static Vector3 getDirectionVector(connectionDirection dir)
    {
        return dir switch
        {
            connectionDirection.North => Vector3.forward,
            connectionDirection.South => Vector3.back,
            connectionDirection.East => Vector3.right,
            connectionDirection.West => Vector3.left,
            _ => Vector3.forward
        };
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isConnected ? connectedColor : openColor;
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // draw direction arrow
        Vector3 arrowEnd = transform.position + transform.forward * gizmoSize * 2;
        Gizmos.DrawLine(transform.position, arrowEnd);

        // arrowhead
        Vector3 right = transform.right * gizmoSize * 0.3f;
        Vector3 back = -transform.forward * gizmoSize * 0.5f;
        Gizmos.DrawLine(arrowEnd, arrowEnd + back + right);
        Gizmos.DrawLine(arrowEnd, arrowEnd + back - right);
    }

    void OnDrawGizmosSelected()
    {
        // draw larger when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.5f);

        // label direction
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * gizmoSize * 2, direction.ToString());
        #endif
    }
}
