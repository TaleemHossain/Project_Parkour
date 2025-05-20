using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] List<Neighbour> neighbours;
    private void Awake()
    {
        foreach (var neighbour in neighbours)
        {
            neighbour.direction = (neighbour.point.transform.position - this.transform.position).normalized;
        }
        var twoWayNeigbours = neighbours.Where(n => n.isTwoWay);
        foreach (var neighbour in twoWayNeigbours)
        {
            neighbour.point.CreateConnections(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
        }
    }
    public void CreateConnections(ClimbPoint point, Vector2 direction, ConnectionType connectionType, bool isTwoWay = true)
    {
        var neighbour = new Neighbour()
        {
            point = point,
            direction = direction,
            connectionType = connectionType,
            isTwoWay = isTwoWay
        };
        neighbours.Add(neighbour);
    }
    public Neighbour GetNeighbour(Vector3 direction, float maxAllowedAngle)
    {
        foreach (var neighbour in neighbours)
        {
            if (Vector3.Angle(neighbour.direction, direction) < maxAllowedAngle)
            {
                return neighbour;
            }
        }
        return null;
    }
    private void OnDrawGizmos()
    {
        Debug.DrawRay(transform.position, transform.forward, Color.blue);
        foreach (var neighbour in neighbours)
        {
            if (neighbour.point != null)
            {
                Debug.DrawLine(transform.position, neighbour.point.transform.position, neighbour.isTwoWay ? Color.green : Color.red);
            }
        }
    }
}
[System.Serializable]
public class Neighbour
{
    public ClimbPoint point;
    public Vector3 direction;
    public ConnectionType connectionType;
    public bool isTwoWay = true;

}
public enum ConnectionType {Jump, Move}