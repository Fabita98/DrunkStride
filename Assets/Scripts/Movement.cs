using UnityEngine;
/* 
Creazione di un agente che si muove su una piattaforma senza mai percorrere una linea retta.

Configurazione
La piattaforma può avere qualsiasi dimensione, quadrata o rettangolare.
L'agente cambierà traiettoria a intervalli casuali: ogni volta percorrerà una circonferenza diversa, 
dirigendosi prima a destra e poi a sinistra continuamente.
L'agente si muove a una velocità costante di 1 metro al secondo.
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
    public float speed = .05f;
    private Vector3 deltaMovement;
    private CharacterController chController;
    private bool circleFound = false;

    [Header(" Events ")]
    float changeDirTime, changeInterval;

    [Header(" Circumference ")]
    float angle, radius;
    Vector3 center, currentCircle, oldCircleCenter, newCircleCenter;
    Vector3 circumference;

    [Header(" Platform ")]
    private GameObject platform;
    private MeshCollider platformMeshCollider;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        platformMeshCollider = platform.GetComponent<MeshCollider>();
        ComputeCircumference();
        currentCircle = circumference;
        SetNewChangeTime();
    }

    void Update()
    {
        changeDirTime -= Time.deltaTime;

        // Intervallo terminato --> nuovo cambio di direzione richiesto
        if (changeDirTime <= 0)
        {
            SetNewCircle();
            deltaMovement = chController.velocity;            
            SetNewChangeTime();
        }

        chController.Move(speed * Time.deltaTime * currentCircle);
    }

    private void SetNewChangeTime()
    {
        changeInterval = Random.Range(1f, 10f);
        changeDirTime = Time.time + changeInterval;
        Debug.Log("Next direction change in " + changeInterval + " seconds");
    }

    public void ComputeCircumference()
    {
        radius = Random.Range(1f, 3f); // da regolare entro i limiti
        center = new Vector3(Random.Range(-platformMeshCollider.bounds.size.x, platformMeshCollider.bounds.size.x), 0, Random.Range(-platformMeshCollider.bounds.size.z, platformMeshCollider.bounds.size.z));
        Debug.Log("Center: " + center);
        angle = Mathf.Repeat(angle, 360f);
        float x = center.x + radius * Mathf.Cos(angle);
        float z = center.z + radius * Mathf.Sin(angle);
        circumference = new(x, 0, z);
    }

    private void SetNewCircle() {        
        while (circleFound.Equals(false))
        {
            oldCircleCenter = center;
            ComputeCircumference();
            newCircleCenter = center;
            if (CoplanarityCheck(DistanceVector(oldCircleCenter, chController.transform.position), DistanceVector(newCircleCenter, chController.transform.position), deltaMovement).Equals(true))
            {
                currentCircle = Vector3.zero;
                Debug.Log("<color=orange>New center is NOT on the other plan side! --> Lets try again... </color>");
                ComputeCircumference();
            }
            else {
                currentCircle = circumference;
                circleFound = true;
                Debug.Log("<color=green>New center is on the OTHER plane side! New circumference SUCCESSFULLY found! </color>");
                return;
            }            
        }  
        circleFound = false;
    }

    private bool CoplanarityCheck(Vector3 a, Vector3 b, Vector3 v)
    {
        Vector3 crossVA = Vector3.Cross(v, a);
        Vector3 crossVANormalized = Vector3.Normalize(crossVA);
        Vector3 crossVB = Vector3.Cross(v, b);
        Vector3 crossVBNormalized = Vector3.Normalize(crossVB);
        if (Vector3.Dot(crossVANormalized, crossVBNormalized) > 0) return true; // coplanari --> non superato
        else return false; // dot < 0 --> check superato
    }

    private Vector3 DistanceVector(Vector3 a, Vector3 b)
    {
        return new Vector3(b.x - a.x, b.y - a.y, b.z - a.z);
    }

    private void GetPlatformEdges()
    {
        if (platformMeshCollider != null && platformMeshCollider.sharedMesh != null)
        {
            Vector3 planeCenter = platformMeshCollider.bounds.center;
            Vector3 planeNormal = transform.TransformDirection(platformMeshCollider.sharedMesh.normals[0]);
            
        }
    }    
}