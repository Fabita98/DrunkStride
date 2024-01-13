using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text circleInfoText, timeRemainingText, closestFurthestVerText;
    private Movement movement;

    void Start()
    {
        movement = FindObjectOfType<Movement>();
    }

    void Update()
    {
        if (movement.centerFound)
        {
            circleInfoText.text = $"Circle Center: {movement.currentCenter} \nRadius: {movement.radius}";
        }

        timeRemainingText.text = $"Time Remaining: {movement.changeInterval}\nTimer changed {movement.changeTimerCounter} times";       
        closestFurthestVerText.text = $"ClosestVer: {movement.closestVertexName}\nFurthestVer: {movement.furthestVertexName}";        
    }
}