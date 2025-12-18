using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TabMenuController : MonoBehaviour
{
    [System.Serializable]
    public class TabItem
    {
        public Toggle toggle;       // Tab button (Toggle)
        public GameObject panel;    // Content panel for this tab
    }

    public ToggleGroup toggleGroup;
    public List<TabItem> tabs;

    void Awake()
    {
        // Ensure exactly one tab is on at start (if AllowSwitchOff = false)
        if (!toggleGroup.AnyTogglesOn() && tabs.Count > 0)
        {
            tabs[0].toggle.isOn = true;
        }

        foreach (var t in tabs)
        {
            // Capture local reference for closure
            var tab = t;
            t.toggle.onValueChanged.AddListener(isOn =>
            {
                if (isOn) ShowPanel(tab);
            });
        }

        // Initialize panels according to current on toggle
        var active = toggleGroup.ActiveToggles().FirstOrDefault();
        if (active != null)
        {
            var current = tabs.FirstOrDefault(x => x.toggle == active);
            if (current != null) ShowPanel(current);
        }
        else
        {
            // Fallback
            foreach (var t in tabs) t.panel.SetActive(false);
            if (tabs.Count > 0) ShowPanel(tabs[0]);
        }
    }

    private void ShowPanel(TabItem target)
    {
        foreach (var t in tabs)
            t.panel.SetActive(t == target);
    }
    public void ActivateByIndex(int index)
    {
        if (index < 0 || index >= tabs.Count) return;
        SetOn(tabs[index]);
    }
    private void SetOn(TabItem item)
    {
        // ToggleGroup 규칙을 따르기 위해 Toggle.isOn 으로만 상태를 바꿔준다
        if (!item.toggle.isOn)
            item.toggle.isOn = true; // onValueChanged에서 ShowOnly 호출됨
        else
            ShowPanel(item); // 이미 켜져 있으면 패널만 동기화
    }
}
