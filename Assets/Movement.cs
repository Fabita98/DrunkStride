using UnityEngine;
/* 
Creazione di un agente che si muove su una piattaforma senza mai percorrere una linea retta.

Configurazione
La piattaforma può avere qualsiasi dimensione, quadrata o rettangolare.
L'agente cambierà traiettoria a intervalli casuali: ogni volta percorrerà una circonferenza diversa, dirigendosi prima a destra e poi a sinistra continuamente.
L'agente si muove a una velocità costante di 1 metro al secondo.
Ad ogni intervallo, l'agente sceglierà un valore casuale per il tempo del prossimo cambio di traiettoria nell'intervallo (1, 10] secondi.
Successivamente, l'agente selezionerà una circonferenza casuale dirigendosi a destra o a sinistra, con un raggio compreso tra 1 e il massimo raggio che non faccia cadere l'agente dalla piattaforma.
La selezione del raggio è indipendente dal tempo del prossimo cambio di traiettoria.
L'agente non si ferma mai.
Vincoli
- Il sistema deve funzionare indipendentemente dalla forma o dalle dimensioni della piattaforma. 
- Raggio [1 : Max_radius]
- Intervallo [1 : 10]s / {0}
*/
public class Movement : MonoBehaviour
{
    const float speed = 1f;
    float changeDirTime, changeInterval, angle, radius, center;
    CharacterController chController;
    GameObject platform;

    private void Start()
    {
        chController = GetComponent<CharacterController>();
        platform = GameObject.FindGameObjectWithTag("Floor");
        center = Random.Range(-platform.transform.localScale.x / 2, platform.transform.localScale.x / 2);
    }

    void Update()
    {
        // Calcola la posizione sulla circonferenza
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        Vector3 newPosition = new(x, 0, z);

        chController.Move(speed * Time.deltaTime * newPosition);
        if (Time.time > changeDirTime) SetNewDir(); 
        
    }

    private void SetNewDir() { 
        changeInterval = Random.Range(1f, 10f);
        changeDirTime = Time.time + changeInterval;
        Debug.Log("Next direction change in " + changeInterval + " seconds");
        angle = Mathf.Repeat(angle, 2 * Mathf.PI) * Time.deltaTime;
        radius = Random.Range(1f, 10f);    
    }
}