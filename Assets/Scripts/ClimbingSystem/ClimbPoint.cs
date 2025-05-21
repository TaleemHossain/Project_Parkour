using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ClimbPoint : MonoBehaviour
{
    [SerializeField] bool mountPoint;
    [SerializeField] List<Neighbour> neighbours;
    private void Awake()
    {
        var twoWayNeigbours = neighbours.Where(n => n.isTwoWay);
        foreach (var neighbour in twoWayNeigbours)
        {
            neighbour.point.CreateConnections(this, -neighbour.direction, neighbour.connectionType, neighbour.isTwoWay);
        }
    }
    public void Start()
    {
        foreach (var neighbour in neighbours)
        {
            if (neighbour.point.transform.parent == transform.parent)
            {
                neighbour.direction = neighbour.point.transform.localPosition - transform.localPosition;
            }
            else
            {
                neighbour.direction.y = neighbour.point.transform.position.y - transform.position.y;

                if (neighbour.point.transform.forward == Vector3.forward)
                {
                    neighbour.direction.x = neighbour.point.transform.position.x - transform.position.x;
                }
                else if (neighbour.point.transform.forward == Vector3.back)
                {
                    neighbour.direction.x = -(neighbour.point.transform.position.x - transform.position.x);
                }
                else if (neighbour.point.transform.forward == Vector3.right)
                {
                    neighbour.direction.x = -(neighbour.point.transform.position.z - transform.position.z);
                }
                else if (neighbour.point.transform.forward == Vector3.left)
                {
                    neighbour.direction.x = neighbour.point.transform.position.z - transform.position.z;
                }
            }
            neighbour.direction.z = 0;
            neighbour.direction = neighbour.direction.normalized;
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
    public bool MountPoint => mountPoint;
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