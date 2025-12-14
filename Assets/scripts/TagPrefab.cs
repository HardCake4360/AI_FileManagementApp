using UnityEngine;
using TMPro;

public class TagPrefab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    public void SetText(string txt)
    {
        text.text = txt;
    }
}
