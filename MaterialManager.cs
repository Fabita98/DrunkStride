using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public Material myMaterial;

    void Start()
    {
        myMaterial = new Material(Shader.Find("Standard"));
        myMaterial.color = Color.black;
    }

    public void ChangeColor(Color newColor)
    {
        myMaterial.color = newColor;
    }
}