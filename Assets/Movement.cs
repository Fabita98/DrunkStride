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
    const float speed = 1f;
    Vector3 deltaMovement;
    CharacterController chController;
    private Vector3 velocity;
    GameObject platform;

    [Header(" Events ")]
    float changeDirTime, changeInterval;

    [Header(" Circumference ")]
    float angle, radius;
    Vector3 center;

    LineRenderer lineRenderer;
    MeshRenderer platformMeshRenderer;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        platformMeshRenderer = platform.GetComponent<MeshRenderer>();
        velocity = Vector3.zero;
    }

    void FixedUpdate()
    {
        Vector3 currentCircle = Vector3.zero;
        changeDirTime -= Time.fixedDeltaTime;        

        if ( changeDirTime <= 0) 
        {
            SetNewChangeTime();
            currentCircle = ComputeCircumference();
            deltaMovement = distanceVector(chController.transform.position, chController.transform.position + currentCircle);
        }

        chController.transform.position = Vector3.SmoothDamp(transform.position, transform.position + currentCircle, ref velocity, changeInterval, speed);

        //if (CoplanarityCheck( , , deltaMovement).Equals(true)) {
        //    SetNewDir();
        //}
    }

    private void SetNewChangeTime() {         
        changeInterval = Random.Range(1f, 10f);
        changeDirTime = Time.time + changeInterval;
        Debug.Log("Next direction change in " + changeInterval + " seconds");
    }

    public Vector3 ComputeCircumference()
    {
        radius = Random.Range(1f, 10f); // da regolare entro i limiti
        center = new Vector3(Random.Range(-platformMeshRenderer.bounds.size.x, platformMeshRenderer.bounds.size.x), 0, Random.Range(-platformMeshRenderer.bounds.size.z, platformMeshRenderer.bounds.size.z));
        angle = Mathf.Repeat(angle, 360f);
        float x = center.x + radius * Mathf.Cos(angle);
        float z = center.z + radius * Mathf.Sin(angle);
        Vector3 circumference = new (x, 0, z);
        return circumference;
    }

    private bool CoplanarityCheck(Vector3 a, Vector3 b, Vector3 v) {
        Vector3 crossVA = Vector3.Cross(v, a);
        Vector3 crossVANormalized = Vector3.Normalize(crossVA);
        Vector3 crossVB = Vector3.Cross(v, b);
        Vector3 crossVBNormalized = Vector3.Normalize(crossVB);
        if (Vector3.Dot(crossVANormalized, crossVBNormalized) > 0) return true;
        else return false;
    }

    private Vector3 distanceVector(Vector3 a, Vector3 b)
    {
        return new Vector3(b.x - a.x, b.y - a.y, b.z - a.z);
    }
}    