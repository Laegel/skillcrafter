using System.Collections;
using System.Collections.Generic;

using Godot;

public partial class DamageManager : Node2D
{
    public static DamageManager current;
    // public GameObject prefab;
    public Color color = new Color(1, 1, 1);

    public override void _Ready()
    {
        current = this;
    }

    public override void _Process(double delta)
    {
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Create(Vector3.zero, Random.Range(0, 1000).ToString(), color);
        // }
    }

    public void Create(Vector3 position, string text, Color color)
    {
        // var popup = Instantiate(prefab, position, Quaternion.identity);
        // popup.transform.SetParent(gameObject.transform, false);
        // popup.GetComponent<Canvas>().sortingOrder = 1;
        // popup.GetComponent<Canvas>().sortingLayerName = "VFX";
        // var rt = popup.GetComponent<RectTransform>();

        // rt.anchoredPosition = new Vector3(0,0);
        // var temp = popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        // temp.text = text;
        // temp.faceColor = color;
        // temp.color = new Color(1, 1, 1,0);

        // Destroy(popup, 0.8f);
    }
}
