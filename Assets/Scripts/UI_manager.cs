using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text circleCenterText;
    public Text timeRemainingText;
    private Movement movement;

    void Start()
    {
        movement = FindObjectOfType<Movement>();        
    }

    void Update()
    {
        if (movement.centerFound)
        {
            circleCenterText.text = $"Circle Center: {movement.newCircleCenter} \nRadius: {movement.radius}";
        }

        timeRemainingText.text = $"Time Remaining: {movement.changeInterval}\nTimer changed {movement.changeTimerCounter} times";
    }
}