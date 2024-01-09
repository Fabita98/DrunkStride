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
    [Header(" Agent ")]
    [SerializeField] private float speed = 1f;
    private Vector3 deltaMovement;
    private CharacterController chController;
    [NonSerialized] public bool centerFound;

    [Header(" Events ")]
    private bool isGrounded;
    public int changeTimerCounter = 0;
    [HideInInspector] public float changeInterval;

    [Header(" Circumference ")]
    private float angle = 0f;
    private Vector3 center, oldCircleCenter, currentCenter;
    public float radius;
    [HideInInspector] public Vector3 newCircleCenter;

    [Header(" Platform ")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;
    private Vector3[] sides;
    private Vector3 closestEdge;    
    private float minDistance;
    public bool rotatingPlatform;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        platformMeshCollider = platform.GetComponent<MeshCollider>();
        minDistance = Vector3.Magnitude(platformMeshCollider.bounds.max);
        Debug.Log($"Initial minDistance: {minDistance}");
        GetPlatformEdges();
        SetNewCenter();
    }

    void Update()
    {
        deltaMovement = chController.velocity;        
        changeInterval -= Time.deltaTime;
        
        if (changeInterval <= 0)
        {
            GetPlatformEdges();
            GetClosestEdge();
            SetNewCenter();
            SetNewChangeTime();
        }

        CheckIfGrounded();
        chController.transform.RotateAround(newCircleCenter, Vector3.up, angle);
    }

    private void SetNewChangeTime()
    {
        changeTimerCounter++;
        changeInterval = Random.Range(1f, 10f); // [1, 10]
    }

    public void ComputeCircumference()
    {
        radius = Random.Range(1f, minDistance);
        center = new Vector3(Random.Range(-platformMeshCollider.bounds.extents.x, platformMeshCollider.bounds.extents.x), 0, Random.Range(-platformMeshCollider.bounds.extents.z, platformMeshCollider.bounds.extents.z));
        angle += speed * Time.deltaTime;
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

                if (CoplanarityCheck(DistanceVector(oldCircleCenter, chController.transform.position), DistanceVector(newCircleCenter, chController.transform.position), deltaMovement))
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
                    Debug.Log($"OldCenter: {oldCircleCenter}  NewCenter: {newCircleCenter}  final radius: {radius} \n attempts: {attempts}");
                    // Draw the currentCenter 
                    Debug.DrawLine(newCircleCenter, newCircleCenter + Vector3.up * 5f, Color.red, 5f);
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
        Vector3 crossVA = Vector3.Cross(v, a);
        Vector3 crossVANormalized = Vector3.Normalize(crossVA);
        Vector3 crossVB = Vector3.Cross(v, b);
        Vector3 crossVBNormalized = Vector3.Normalize(crossVB);
        if (Vector3.Dot(crossVANormalized, crossVBNormalized) > 0) return true; // dot > 0 --> check non superato
        else return false; // dot < 0 --> check superato
    }

    private Vector3 DistanceVector(Vector3 a, Vector3 b)
    {
        return new Vector3(b.x - a.x, b.y - a.y, b.z - a.z);
    }

    public Vector3[] GetPlatformEdges()
    {
        sides = new Vector3[4];
        Vector3[] vertices = new Vector3[4];
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
                sides[0] = DistanceVector(vertices[0], vertices[3]); // bottom
                sides[1] = DistanceVector(vertices[0], vertices[2]); // left
                sides[2] = DistanceVector(vertices[1], vertices[2]); // top
                sides[3] = DistanceVector(vertices[1], vertices[3]); // right

                //Draws the platform edges
                Debug.DrawLine(vertices[0], vertices[3], Color.red, 40f);
                Debug.DrawLine(vertices[0], vertices[2], Color.blue, 40f);
                Debug.DrawLine(vertices[1], vertices[2], Color.yellow, 40f);
                Debug.DrawLine(vertices[1], vertices[3], Color.green, 40f);
                return sides;
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

    public void GetClosestEdge()
    {
        if (sides == null || sides.Length == 0)
        {
            Debug.LogError("Sides array is null or empty.");
            return;
        }

        minDistance = float.MaxValue;

        foreach (Vector3 edge in sides)
        {
            float squaredDistance = Vector3.Magnitude(chController.transform.position - edge);
            if (squaredDistance < minDistance)
            {
                minDistance = squaredDistance;
                closestEdge = edge;
                Debug.Log($"Closest edge: {closestEdge}  minDistance: {minDistance}");
            }
        }
    }

    private void PlaneRotation()
    {
        if (platform != null) platform.transform.Rotate(0, .2f, 0);
    }

    private void CheckIfGrounded()
    {
        if (chController.transform.position.x > platformMeshCollider.bounds.extents.x ||
            chController.transform.position.z > platformMeshCollider.bounds.extents.z)
        {
            isGrounded = false;
            chController.Move(9f * Time.deltaTime * Vector3.down);
        }
        else
        {
            isGrounded = true;
        }
    }

    private void CameraFollow()
    {        
        Camera.main.transform.LookAt(chController.transform.position);
    }
}