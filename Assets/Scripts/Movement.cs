using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    [Header("Agent")]
    [SerializeField] private float speed = 1f;
    private Vector3 agentVelocity;
    private CapsuleCollider capsuleCollider;
    private CharacterController chController;
    [NonSerialized] public bool centerFound;
    [NonSerialized] public bool isMovingRight = true;

    [Header("Events")]
    [HideInInspector] public int changeTimerCounter = 0;
    [HideInInspector] public float changeInterval;

    [Header("Circumference")]
    [HideInInspector] public float radius;
    private Vector3 newCircleCenter;
    private Vector3[] circlePos;
    private float theta;
    private float[] radians;
    private int validPoints;
    private bool circleFound;
    [HideInInspector] public Vector3 currentCenter;

    [Header("Platform")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;
    private float minX, maxX, minZ, maxZ;
    private readonly Vector3[] vertices = new Vector3[4];
    private Bounds platformBounds;
    private float minDistance, maxDistance;
    private int groundMask;
    [HideInInspector]
    public Vector3 closestVertex, furthestVertex;
    public string closestVertexName, furthestVertexName;
    public string[] vertexNames = new string[4];

private void Start()
    {
        chController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        platformMeshCollider = platform.GetComponent<MeshCollider>();
        platformBounds = platformMeshCollider.bounds;
        groundMask = LayerMask.GetMask("groundMask");
        chController.transform.position = platformBounds.center;
        GetClosestAndFurthestVertex();
        Debug.Log($"Initial minDistance: {minDistance}");
    }

    void Update()
    {
        agentVelocity = chController.velocity;        
        changeInterval -= Time.deltaTime;
        
        if (changeInterval <= 0)
        {
            isMovingRight = !isMovingRight;
            GetClosestAndFurthestVertex();
            SetCirclePosition();
            SetNewChangeTime();
        }

        ChControllerMovement();
    }

    private void SetNewChangeTime()
    {
        changeTimerCounter++;
        changeInterval = Random.Range(1f, 10f); // [1, 10]
    }

    void SetCirclePosition()
    {
        circleFound = false;
        float tempRadius = 0;
        circlePos = new Vector3[4];

        if (circlePos == null || circlePos.Length == 0)
        {
            Debug.LogError("circlePos array is null or empty.");
            return;
        }

        try
        {
            while (circleFound.Equals(false)) 
            {
                tempRadius = Random.Range(1f, minDistance);
                radians = new float[4] { 0, Mathf.PI / 2, Mathf.PI, 3 * Mathf.PI / 2 };
                newCircleCenter = new (chController.transform.position.x + (isMovingRight ? tempRadius : -tempRadius), 
                                        platform.transform.position.y, 
                                        chController.transform.position.z + (isMovingRight ? tempRadius : -tempRadius));

                for (int i = 0; i < circlePos.Length; i++)
                {
                    theta = radians[i];
                    circlePos[i] = newCircleCenter + new Vector3(tempRadius * Mathf.Cos(theta), 0, tempRadius * Mathf.Sin(theta));
                    if (platformBounds.Contains(circlePos[i])) validPoints++;
                }
                if (validPoints == 4) circleFound = true;
                else validPoints = 0;
            }            
            radius = tempRadius;
            currentCenter = newCircleCenter;
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.LogError($"Index out of range in SetCirclePosition(): {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Unexpected error in SetCirclePosition(): {e.Message}");
        }
    }

    public void GetClosestAndFurthestVertex()
    {
        if (platformBounds == null)
        {
            Debug.LogError("platformBounds is null or empty.");
            return;
        }

        minX = platformBounds.min.x;
        maxX = platformBounds.max.x;
        minZ = platformBounds.min.z;
        maxZ = platformBounds.max.z;

        vertices[0] = new Vector3(platformBounds.min.x, platformBounds.min.y, platformBounds.min.z); // bottom left
        vertices[1] = new Vector3(platformBounds.min.x, platformBounds.min.y, platformBounds.max.z); // top left
        vertices[2] = new Vector3(platformBounds.max.x, platformBounds.min.y, platformBounds.min.z); // bottom right
        vertices[3] = new Vector3(platformBounds.max.x, platformBounds.min.y, platformBounds.max.z); // top right

        vertexNames = new string[] { "bottom left", "top left", "bottom right", "top right" };

        minDistance = float.MaxValue;
        maxDistance = float.MinValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Distance(chController.transform.position, vertices[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestVertex = vertices[i];
                closestVertexName = vertexNames[i];
            }
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestVertex = vertices[i];
                furthestVertexName = vertexNames[i];
            }
        }
    }

    private void ChControllerMovement()
    {
        float duration = 2f;
        float distanceToGround = capsuleCollider.height / 2;
        float maxDir = distanceToGround + 0.1f;
        Vector3 origin = capsuleCollider.bounds.center;
        if (Physics.Raycast(origin, Vector3.down, out _, maxDir, groundMask))
        {
            Vector3 dir = (currentCenter - chController.transform.position).normalized;
            Vector3 tangent = Vector3.Cross(dir, Vector3.up).normalized;
            Vector3 movementDirection = isMovingRight ? tangent : -tangent;
            chController.Move(speed * Time.deltaTime * movementDirection);
            DrawDebugRays(origin, maxDir, changeInterval, tangent, movementDirection);
        }
        else
        {
            Debug.DrawRay(origin, Vector3.down * maxDir, Color.red, duration);
            chController.Move(9.8f * Time.deltaTime * Vector3.down);
        }
    }

    private void DrawDebugRays(Vector3 origin, float maxDir, float duration, Vector3 tangent, Vector3 movementDirection)
    {
        Debug.DrawRay(chController.transform.position, agentVelocity, Color.magenta, changeInterval);
        Debug.DrawRay(origin, Vector3.down * maxDir, Color.green, duration); // raycast draw
        Debug.DrawRay(origin, tangent * radius, Color.yellow, duration); // Draw tangent
        Debug.DrawRay(origin, movementDirection * radius, Color.blue, duration); // Draw movementDirection
        Debug.DrawRay(currentCenter, Vector3.up, Color.red, changeInterval); // Draw current circle center
        for (int i = 0; i < circlePos.Length; i++)
        {
            // Draw the main 4 points of the currentCircle
            Debug.DrawRay(circlePos[i], Vector3.up, Color.yellow, changeInterval);
        }
    }
}