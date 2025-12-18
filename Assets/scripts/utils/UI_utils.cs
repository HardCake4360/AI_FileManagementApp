using UnityEngine;
using UnityEngine.UI;

public class UI_utils : MonoBehaviour
{
    [Header("Settings")]
    public bool StartActive;

    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
