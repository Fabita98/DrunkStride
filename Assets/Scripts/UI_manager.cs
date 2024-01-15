using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button warningButton;
    public Text circleInfoText, timeRemainingText, closestFurthestVerText, warningText;
    private Movement movement;

    void Start()
    {
        movement = FindObjectOfType<Movement>();
    }

    void Update()
    {
        if (movement.circleFound)
        {
            circleInfoText.text = $"MinDistance: {movement.minDistance} \nRadius: {movement.radius}";
        }

        timeRemainingText.text = $"Time Remaining: {movement.changeInterval}\nTimer changed {movement.changeTimerCounter} times";       
        closestFurthestVerText.text = $"ClosestVer: {movement.closestVertexName}\nFurthestVer: {movement.furthestVertexName}";

        if (!movement.resizedPlatformBounds.Contains(movement.chController.transform.position))
        {
            warningButton.GetComponent<Image>().color = Color.black;
            warningButton.gameObject.SetActive(true);
            warningText.text = "Warning: The agent is outside the platform!";
        }
        else warningButton.gameObject.SetActive(false);        
    }
}