using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public Image crosshairImage;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        // Opdater positionen baseret på musens position:
        crosshairImage.rectTransform.position = Input.mousePosition;
    }
}