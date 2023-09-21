using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Emit;
using Networking;
using Optimization;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class TestMovement : MonoBehaviour
{
    private Vector3 _movementDirection;
    private CharacterController _characterController;
    private float _jumpSpeed;
    private const float Gravity = -30f;
    private float _speedModifier = 1f;
    private float _speed = 5f;
    public GameObject player;
    private const int EmptyBlockCost = 1000000000;
    private IServer _server;
    private List<int> _path = new List<int>();
    private int pathLength = 0;
    private float _jumpMultiplier = 9;
    private int _width = 512;
    private int index = 0;

    private Vector3Int _targetBlock;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        player = GameObject.FindWithTag("Player");
    }

    public int GetVertexIndex(Vector3Int vertex)
    {
        return vertex.y * 512 * 512 + vertex.z * 512 + vertex.x;
    }
    
    private (int x, int y, int z) GetVertexCoordinates(int index)
    {
        var y = index >> 18;
        var z = (index - (y << 18)) >> 9;
        var x = index - (y << 18) - (z << 9);
        return (x, y, z);
    }
    
    private List<int> GetNeighborsByIndex(int index)
    {
        var res = new List<int>();
        for (var i = -1; i < 2; i++)
        {
            for (var j = -1; j < 2; j++)
            {
                for (var k = -1; k < 2; k++)
                {
                    if (!(i == 0 && j == 0 && k == 0))
                    {
                        res.Add(index + _width * _width * i + _width * j + k);
                    }
                }
            }
        }
        return res;
        
    }
    
    private Vector3Int GetVectorByIndex(int index)
    {
        var y = index >> 18;
        var z = (index - (y << 18)) >> 9;
        var x = index - (y << 18) - (z << 9);
        return new Vector3Int(x, y, z);
    }
    
    private double Heuristic(Vector3Int target, int neighbour)
    {
        var neighbourCoordinates = GetVertexCoordinates(neighbour);
            
        return (target.x - neighbourCoordinates.x) * (target.x - neighbourCoordinates.x)
               + (target.y - neighbourCoordinates.y) * (target.y - neighbourCoordinates.y)
               + (target.z - neighbourCoordinates.z) * (target.z - neighbourCoordinates.z);
    }
    
    public void AStar(int startVertex, int targetVertex)
    {
        var sw = new Stopwatch();
        sw.Start();
        var count = 0;
        var D = new Dictionary<int, int>();
        D[startVertex] = 0;
        var P = new Dictionary<int, int>();
        P[startVertex] = -1;
        var pq = new PriorityQueue<int, double>();
        pq.Enqueue(startVertex, Heuristic(GetVectorByIndex(targetVertex), startVertex));
        var counter = 0;
        while (pq.Count > 0)
        {
            counter++;
            if (counter > 100)
                break;
            var vertex = pq.Dequeue();
            if (GetVertexCoordinates(vertex) == GetVertexCoordinates(targetVertex))
            {
                break;
            }
            foreach (var neighbour in GetNeighborsByIndex(vertex))
            {
                var cost = _server.MapProvider.MapData._surface.Contains(neighbour) ? 1 : EmptyBlockCost;
                var newCost = D[vertex] + cost;
                if (!D.ContainsKey(neighbour) || newCost < D[neighbour])
                {
                    count++;
                    D[neighbour] = newCost;
                    P[neighbour] = vertex;
                    var priority = newCost + Heuristic(GetVectorByIndex(targetVertex), neighbour);
                    pq.Enqueue(neighbour, priority);
                }
            }
        }
        
        var path = new List<int>();
        while (targetVertex != -1)
        {
            path.Add(targetVertex);
            targetVertex = P[targetVertex];
        }
        path.Reverse();
        _path = path;
        pathLength = path.Count;
        sw.Stop();
        //.Log($"elapsed {sw.Elapsed}");
    }

    public void Construct(IServer server, Vector3Int targetBlock)
    {
        _server = server;
        _targetBlock = targetBlock;
    }

    // Update is called once per frame
    void Update()
    {
        var head = GetVertexIndex(_targetBlock);
        var pl = GetVertexIndex(new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y,
            (int)player.transform.position.z));
        if (pathLength == 0)
        {
            AStar(
                head,
                pl);
        }
        
        
        // Debug.Log(Mathf.Abs(Vector3.Distance(transform.position, GetVectorByIndex(_path[index]))) <= 0.01);
        // Debug.Log($"head {transform.position}");
        // Debug.Log($"point {GetVectorByIndex(_path[index])}");
        //Debug.Log(index);
        //Debug.Log(pathLength);
        var position = new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z);
        var target = GetVectorByIndex(_path[index]);
        // Debug.Log($"position {position}");
        // Debug.Log($"target {target}");
        // Debug.Log(target == position);
        if (Vector3.Distance(new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z), GetVectorByIndex(_path[index])) < 0.01f)
        {
            //Debug.Log("if");

            index++;
            if (index == _path.Count)
            {
                AStar(
                    GetVertexIndex(new Vector3Int((int)transform.position.x, (int)transform.position.y,
                        (int)transform.position.z)),
                    GetVertexIndex(new Vector3Int((int)player.transform.position.x, (int)player.transform.position.y,
                        (int)player.transform.position.z)));
                index = 0;
                //Debug.Log("start");
            }
        }
        else
        {
            //Debug.Log("else");
            // var x = (player.transform.position.x - playerTransform.position.x) > 0 ? 1f : -1f;
            // var y = (player.transform.position.y - playerTransform.position.y) > 0 ? 1f : -1f;
            _movementDirection = (GetVectorByIndex(_path[index]) + new Vector3(0.5f, 0.5f, 0.5f) - new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z));
            Debug.Log($"target {GetVectorByIndex(_path[index]) + new Vector3(0.5f, 0.5f, 0.5f)}");
            Debug.Log($"position {new Vector3Int((int)transform.position.x, (int)transform.position.y, (int)transform.position.z)}");
            Debug.Log(_movementDirection);
        }
        

        if (GetVectorByIndex(_path[index]).y > new Vector3Int((int)transform.position.x, (int)transform.position.y,
                (int)transform.position.z).y || GetVectorByIndex(_path[index]).y < new Vector3Int((int)transform.position.x, (int)transform.position.y,
                (int)transform.position.z).y)
        {
            if (_characterController.isGrounded)
            {
                _jumpSpeed = _jumpMultiplier;
            }
        }
        
        // if (index > 0)
        // {
        //     if (GetVectorByIndex(_path[index - 1]).y < GetVectorByIndex(_path[index]).y)
        //     {
        //         if (_characterController.isGrounded)
        //         {
        //             _jumpSpeed = _jumpMultiplier;
        //         }
        //     }
        // }
        
        
        _jumpSpeed += Gravity * Time.deltaTime;
        Vector3 direction = new Vector3(_movementDirection.x * _speed * _speedModifier * Time.deltaTime,
            _jumpSpeed * Time.deltaTime,
            _movementDirection.z * Time.deltaTime * _speed * _speedModifier);
        // Vector3 direction = new Vector3(_movementDirection.x * _speed * _speedModifier * Time.deltaTime,
        //     _movementDirection.y * Time.deltaTime * _speed * _speedModifier,
        //     _movementDirection.z * Time.deltaTime * _speed * _speedModifier);
        _characterController.Move(direction);
    }
}
