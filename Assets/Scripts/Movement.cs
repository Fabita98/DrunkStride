using System;
using UnityEngine;
using Random = UnityEngine.Random;
/* 
Creazione di un agente che si muove su una piattaforma senza mai percorrere una linea retta.

Configurazione
La piattaforma può avere qualsiasi dimensione, quadrata o rettangolare.
L'agente cambierà traiettoria a intervalli casuali: ogni volta percorrerà una circonferenza diversa, 
dirigendosi prima a destra e poi a sinistra continuamente.
L'agente si muove a una velocità costante di 1 m/s.
Ad ogni intervallo, l'agente sceglierà un valore casuale per il tempo del prossimo cambio di traiettoria nell'intervallo [1, 10] secondi.
Indipendentemente dal cambio del nuovo intervallo temporale, 
l'agente selezionerà una circonferenza casuale dirigendosi a destra o a sinistra,
con un raggio compreso tra 1 e il massimo raggio che non faccia cadere l'agente dalla piattaforma.
La selezione della nuova direzione di movimento è indipendente dal tempo del prossimo cambio di traiettoria.
L'agente non si ferma mai.
Vincoli
- Il sistema deve funzionare indipendentemente dalla forma o dalle dimensioni della piattaforma. 
- Raggio [1 : Max_radius]
- Intervallo [1 : 10]s / {0}
*/

public class Movement : MonoBehaviour
{
    [Header("Agent")]
    [SerializeField] private float angularSpeed = 1f;
    private Vector3 deltaMovement;
    private Vector3 dir;
    private CapsuleCollider capsuleCollider;
    private CharacterController chController;
    [NonSerialized] public bool centerFound;

    [Header("Events")]
    [HideInInspector] public int changeTimerCounter = 0;
    [HideInInspector] public float changeInterval;

    [Header("Circumference")]
    private Vector3 center, oldCircleCenter, currentCenter;
    [HideInInspector] public float radius;
    [HideInInspector] public Vector3 newCircleCenter;
    private float angle;

    [Header("Platform")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;
    private Vector3[] vertices;
    private float minDistance;
    [HideInInspector] public Vector3 closestVertex;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        platformMeshCollider = platform.GetComponent<MeshCollider>();
        minDistance = Vector3.Magnitude(platformMeshCollider.bounds.max);
        Debug.Log($"Initial minDistance: {minDistance}");
        GetPlatformVertices();
        SetNewCenter();
    }

    void Update()
    {
        angle += Time.deltaTime * angularSpeed;
        deltaMovement = chController.velocity;        
        changeInterval -= Time.deltaTime;
        
        if (changeInterval <= 0)
        {
            GetPlatformVertices();
            GetClosestVertex();
            SetNewCenter();
            SetNewChangeTime();
        }

        ChControllerMovement();
    }

    private void SetNewChangeTime()
    {
        changeTimerCounter++;
        changeInterval = Random.Range(1f, 10f); // [1, 10]
    }

    public void ComputeCircumference()
    {
        radius = Random.Range(1f, minDistance);
        center = new Vector3(Random.Range(platformMeshCollider.bounds.min.x, platformMeshCollider.bounds.max.x), platformMeshCollider.transform.position.y, Random.Range(platformMeshCollider.bounds.min.z, platformMeshCollider.bounds.max.z));
    }

    private void SetNewCenter()
    {
        centerFound = false;
        int maxAttempts = 1000;
        int attempts = 0; 

        while (centerFound.Equals(false) && attempts < maxAttempts)
        {
            attempts++;
            try
            {
                oldCircleCenter = center;
                ComputeCircumference();
                newCircleCenter = center;

                if (CoplanarityCheck(oldCircleCenter, newCircleCenter, deltaMovement))
                {   
                    //Failure
                    currentCenter = Vector3.zero;
                    Debug.Log("<color=orange>New center is NOT on the other plan side! --> Lets try again... </color>");
                }
                else
                {
                    //Success
                    currentCenter = newCircleCenter;
                    centerFound = true;                                      
                    Debug.Log("<color=green>New center is on the OTHER plane side! New circumference SUCCESSFULLY found! </color>");
                    Debug.DrawLine(newCircleCenter, newCircleCenter + Vector3.up * 5f, Color.red, 30f);
                    break;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred: maxAttempts reached " + e.Message);
                break;
            }
        }
    }

    private bool CoplanarityCheck(Vector3 a, Vector3 b, Vector3 v)
    {
        return Vector3.Dot(Vector3.Cross(v, a), Vector3.Cross(v, b)) > 0;
    }

    public Vector3[] GetPlatformVertices()
    {
        vertices = new Vector3[4];
        try
        {
            if (platformMeshCollider != null && platformMeshCollider.sharedMesh != null)
            {
                Vector3 planeMin = platformMeshCollider.bounds.min;
                Vector3 planeMax = platformMeshCollider.bounds.max;
                vertices[0] = planeMin; // bottom left
                vertices[1] = planeMax; // top right
                vertices[2] = new Vector3(planeMin.x, planeMin.y, planeMax.z); // top left
                vertices[3] = new Vector3(planeMax.x, planeMax.y, planeMin.z); // bottom right

                //Draws the platform edges
                Debug.DrawLine(vertices[0], vertices[3], Color.red, 40f);
                Debug.DrawLine(vertices[0], vertices[2], Color.blue, 40f);
                Debug.DrawLine(vertices[1], vertices[2], Color.yellow, 40f);
                Debug.DrawLine(vertices[1], vertices[3], Color.green, 40f);
                return vertices;
            }
            else return null;
        }
        catch (Exception e)
        {
            Debug.LogError("An error occurred: " + e.Message);
            Debug.LogError("platformMeshCollider has not been found");
            return null;
        }
    }

    public void GetClosestVertex()
    {
        if (vertices == null || vertices.Length == 0)
        {
            Debug.LogError("Vertices array is null or empty.");
            return;
        }

        minDistance = float.MaxValue;

        for (int i = 0; i < vertices.Length; i++)
        {
            float distance = Vector3.Magnitude(chController.transform.position - vertices[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestVertex = vertices[i];
            }
        }
    }

    private void ChControllerMovement()
    {
        float distanceToGround = capsuleCollider.height / 2;
        if (Physics.Raycast(capsuleCollider.bounds.min, Vector3.down, out _, distanceToGround + 0.1f, LayerMask.GetMask("groundMask")))
        {
            Debug.DrawRay(capsuleCollider.bounds.min, Vector3.down * (distanceToGround + 0.1f), Color.green, 30f);
            float x = currentCenter.x + Mathf.Cos(angle) * radius;
            float z = currentCenter.z + Mathf.Sin(angle) * radius;
            dir = new(x, 0, z);
            chController.Move(Time.deltaTime * dir);
        }
        else
        {
            Debug.DrawRay(capsuleCollider.bounds.min, Vector3.down * (distanceToGround + 0.1f), Color.red, 30f);
            chController.Move(9.8f * Time.deltaTime * Vector3.down);
        }
    }
}