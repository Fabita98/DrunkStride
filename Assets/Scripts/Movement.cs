using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    [Header("Agent")]
    private readonly float speed = 1f;
    private bool isMovingRight = true;
    private bool isBelow, isRight;
    private CapsuleCollider capsuleCollider;
    internal CharacterController chController;

    [Header("Events")]
    internal int changeTimerCounter = 0;
    internal float changeInterval;

    [Header("Circumference")]
    private Vector3[] circlePos;
    private float theta = 0;
    private float[] radians;
    private int validPoints;
    internal bool circleFound;
    internal Vector3 currentCenter;
    internal float radius;

    [Header("Platform")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;
    private float minX, maxX, minZ, maxZ;
    private readonly Vector3[] vertices = new Vector3[4];
    internal Bounds platformBounds, resizedPlatformBounds;
    internal float minDistance, maxDistance;
    internal Vector3 closestVertex, furthestVertex;
    internal string closestVertexName, furthestVertexName;
    internal string[] vertexNames = new string[4];

    private void Awake()
    {
        InstantiateRandomizedPlane();
    }
    
    private void Start()
    {        
        chController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();        
        chController.transform.position = platformBounds.center;
        GetClosestAndFurthestVertex();
    }

    void Update()
    {
        changeInterval -= Time.deltaTime;
        
        if (changeInterval <= 0)
        {
            isMovingRight = !isMovingRight;
            GetClosestAndFurthestVertex();
            WhereIsAgent();
            SetCirclePosition();
            SetNewChangeTime();
        }

        ChControllerMovement();
    }

    private void SetNewChangeTime()
    {
        changeTimerCounter++;
        changeInterval = Random.Range(0.1f, 10f);
    }

    void SetCirclePosition()
    {
        int maxAttempts = 1000;
        float tempRadius = 0;
        Vector3 newCircleCenter = Vector3.zero;
        circleFound = false;
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
                if (maxAttempts <= 0)
                {
                    Debug.LogError("Max attempts reached! --> Moving along previous circle...");
                    return;
                }
                validPoints = 0;
                tempRadius = Random.Range(0.1f, minDistance);
                radians = new float[4] { 0, Mathf.PI / 2, Mathf.PI, 3 * Mathf.PI / 2 };
                if (isBelow && isRight)
                {
                    newCircleCenter = new Vector3(chController.transform.position.x - tempRadius,
                                                  chController.transform.position.y,
                                                  chController.transform.position.z + tempRadius);
                }
                else if (isBelow && !isRight)
                {
                    newCircleCenter = new Vector3(chController.transform.position.x + tempRadius,
                                                  chController.transform.position.y,
                                                  chController.transform.position.z + tempRadius);
                }
                else if (!isBelow && isRight)
                {
                    newCircleCenter = new Vector3(chController.transform.position.x - tempRadius,
                                                  chController.transform.position.y,
                                                  chController.transform.position.z - tempRadius);
                }
                else
                {
                    newCircleCenter = new Vector3(chController.transform.position.x + tempRadius,
                                                  chController.transform.position.y,
                                                  chController.transform.position.z - tempRadius);
                }

                for (int i = 0; i < circlePos.Length; i++)
                {
                    theta = radians[i];
                    circlePos[i] = newCircleCenter + new Vector3(tempRadius * Mathf.Cos(theta), 0, tempRadius * Mathf.Sin(theta));
                    if (resizedPlatformBounds.Contains(circlePos[i])) validPoints++;
                    if (validPoints == 4) circleFound = true;
                }
                maxAttempts--;
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

        minX = resizedPlatformBounds.min.x;
        maxX = resizedPlatformBounds.max.x;
        minZ = resizedPlatformBounds.min.z;
        maxZ = resizedPlatformBounds.max.z;

        vertices[0] = new Vector3(minX, resizedPlatformBounds.min.y, minZ); // bottom left
        vertices[1] = new Vector3(minX, resizedPlatformBounds.min.y, maxZ); // top left
        vertices[2] = new Vector3(maxX, resizedPlatformBounds.min.y, minZ); // bottom right
        vertices[3] = new Vector3(maxX, resizedPlatformBounds.min.y, maxZ); // top right

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

        Debug.DrawRay(vertices[0], vertices[1] - vertices[0], Color.red, 3f);
        Debug.DrawRay(vertices[0], vertices[2] - vertices[0], Color.red, 3f);
        Debug.DrawRay(vertices[1], vertices[3] - vertices[1], Color.red, 3f);
        Debug.DrawRay(vertices[2], vertices[3] - vertices[2], Color.red, 3f);
    }

    public void ChControllerMovement()
    {
        float duration = changeInterval;
        Vector3 origin = capsuleCollider.bounds.center;
        Vector3 dir = (currentCenter - chController.transform.position).normalized;
        Vector3 tangent = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 movementDirection = isMovingRight ? tangent : -tangent;
        chController.Move(speed * Time.deltaTime * movementDirection);
        DrawDebugRays(origin, duration, tangent, movementDirection);
    }

    private void DrawDebugRays(Vector3 origin, float duration, Vector3 tangent, Vector3 movementDirection)
    {
        Debug.DrawRay(origin, tangent * radius, Color.yellow, 2f); // Draw tangent
        Debug.DrawRay(origin, movementDirection * radius, Color.blue, 2f); // Draw movementDirection
        Debug.DrawRay(currentCenter, Vector3.up, Color.red, duration); // Draw current circle center
        for (int i = 0; i < circlePos.Length; i++)
        {
            Debug.DrawRay(circlePos[i], Vector3.up, Color.yellow, duration);
        }
        for (theta = 0; theta < 2 * MathF.PI; theta += 0.1f) 
        { 
        Debug.DrawRay(currentCenter + new Vector3(radius * Mathf.Cos(theta), 0, radius * Mathf.Sin(theta)), Vector3.up, Color.yellow, duration);
        }
    }

    private void WhereIsAgent() {
        isBelow = false;
        isRight = false;
        if (chController.transform.position.x > platformBounds.center.x) isRight = true;
        if (chController.transform.position.z < platformBounds.center.z) isBelow = true;
        Debug.Log($"Agent is {(isBelow ? "below" : "above")} and {(isRight ? "right" : "left")} the center of the platform.");
    }

    public void InstantiateRandomizedPlane()
    {
        // Instantiate a new plane
        GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        newPlane.name = "Platform";
        newPlane.tag = "Floor";
        newPlane.isStatic = true;

        // Add a MeshCollider to the plane
        MeshCollider planeMeshCollider = newPlane.GetComponent<MeshCollider>();

        // Add a MeshRenderer to the plane and assign the BlackFloor material
        MeshRenderer planeMeshRenderer = newPlane.GetComponent<MeshRenderer>();
        planeMeshRenderer.material.color = Color.black;

        // Randomize plane size
        float planeSize = Random.Range(3f, 8f);
        newPlane.transform.localScale = new Vector3(planeSize, newPlane.transform.localScale.y, planeSize);

        // Randomize plane position
        float planePosX = Random.Range(-10f, 10f);
        float planePosZ = Random.Range(-10f, 10f);
        newPlane.transform.position = new Vector3(planePosX, newPlane.transform.position.y, planePosZ);

        // Update plane reference
        platform = newPlane;
        platformMeshCollider = planeMeshCollider;
        platformBounds = platformMeshCollider.bounds;

        // Update plane bounds
        resizedPlatformBounds = new Bounds(platform.transform.position, platform.transform.localScale * 10);
    }
}