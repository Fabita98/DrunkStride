using System;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Movement : MonoBehaviour
{
    [Header("Agent")]
    [SerializeField] private float speed = 1f;
    private CapsuleCollider capsuleCollider;
    private CharacterController chController;
    [NonSerialized] public bool isMovingRight = true;
    [NonSerialized] public bool isBelow, isRight;

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
    [HideInInspector] public bool circleFound;
    [HideInInspector] public Vector3 currentCenter;

    [Header("Platform")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;
    private float minX, maxX, minZ, maxZ;
    private readonly Vector3[] vertices = new Vector3[4];
    private Bounds platformBounds, resizedPlatformBounds;
    [HideInInspector] public float minDistance, maxDistance;
    private int groundMask;
    [HideInInspector]
    public Vector3 closestVertex, furthestVertex;
    public string closestVertexName, furthestVertexName;
    public string[] vertexNames = new string[4];

    private void Awake()
    {
        InstantiateRandomizedPlane();
    }
    
    private void Start()
    {        
        chController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();        
        chController.transform.position = resizedPlatformBounds.center;
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
                                                  platform.transform.position.y,
                                                  chController.transform.position.z + tempRadius);
                }
                else if (isBelow && !isRight)
                {
                    newCircleCenter = new Vector3(chController.transform.position.x + tempRadius,
                                                  platform.transform.position.y,
                                                  chController.transform.position.z + tempRadius);
                }
                else if (!isBelow && isRight)
                {
                    newCircleCenter = new Vector3(chController.transform.position.x - tempRadius,
                                                  platform.transform.position.y,
                                                  chController.transform.position.z - tempRadius);
                }
                else
                {
                    newCircleCenter = new Vector3(chController.transform.position.x + tempRadius,
                                                  platform.transform.position.y,
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

    private void ChControllerMovement()
    {
        //float duration = 2f;
        float distanceToGround = capsuleCollider.height / 2;
        float maxDir = distanceToGround + 0.1f;
        Vector3 origin = capsuleCollider.bounds.center;
        Vector3 dir = (currentCenter - chController.transform.position).normalized;
        Vector3 tangent = Vector3.Cross(dir, Vector3.up).normalized;
        Vector3 movementDirection = isMovingRight ? tangent : -tangent;
        chController.Move(speed * Time.deltaTime * movementDirection);
        DrawDebugRays(origin, maxDir, changeInterval, tangent, movementDirection);
    }

    private void DrawDebugRays(Vector3 origin, float maxDir, float duration, Vector3 tangent, Vector3 movementDirection)
    {
        //Debug.DrawRay(origin, Vector3.down * maxDir, Color.green, duration); // raycast draw
        Debug.DrawRay(origin, tangent * radius, Color.yellow, duration); // Draw tangent
        Debug.DrawRay(origin, movementDirection * radius, Color.blue, duration); // Draw movementDirection
        Debug.DrawRay(currentCenter, Vector3.up, Color.red, changeInterval); // Draw current circle center
        for (int i = 0; i < circlePos.Length; i++)
        {
            // Draw the main 4 points of the currentCircle
            Debug.DrawRay(circlePos[i], Vector3.up, Color.yellow, changeInterval);
        }
    }

    private void WhereIsAgent() {
        isBelow = false;
        isRight = false;
        if (chController.transform.position.z < resizedPlatformBounds.center.z) isBelow = true;
        else if (chController.transform.position.x > resizedPlatformBounds.center.x) isRight = true;
    }

    public void InstantiateRandomizedPlane()
    {
        // Instantiate a new plane
        GameObject newPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        newPlane.name = "Platform";
        newPlane.tag = "Floor";
        newPlane.layer = LayerMask.NameToLayer("groundMask");

        // Add a MeshCollider to the plane
        MeshCollider planeMeshCollider = newPlane.GetComponent<MeshCollider>();

        // Add a MeshRenderer to the plane and assign the BlackFloor material
        MeshRenderer planeMeshRenderer = newPlane.GetComponent<MeshRenderer>();
        planeMeshRenderer.material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/BlackFloor.mat");

        // Randomize plane size
        float planeSize = Random.Range(1f, 10f);
        newPlane.transform.localScale = new Vector3(planeSize, newPlane.transform.localScale.y, planeSize);

        // Randomize plane position
        float planePosX = Random.Range(-3f, 3f);
        float planePosZ = Random.Range(-5f, 5f);
        newPlane.transform.position = new Vector3(planePosX, newPlane.transform.position.y, planePosZ);

        // Update plane reference
        platform = newPlane;
        platformMeshCollider = planeMeshCollider;
        platformBounds = platformMeshCollider.bounds;

        // Update plane bounds
        resizedPlatformBounds = new Bounds(platform.transform.position, platform.transform.localScale * 10);
    }
}