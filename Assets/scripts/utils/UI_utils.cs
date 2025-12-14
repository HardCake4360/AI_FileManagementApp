using UnityEngine;

public class UI_utils : MonoBehaviour
{
    [Header("Settings")]
    public bool StartActive;

    private void Start()
    {
        gameObject.SetActive(StartActive);
    }

    public void ToggleActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
