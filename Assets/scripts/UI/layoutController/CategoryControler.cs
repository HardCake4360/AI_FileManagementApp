using UnityEngine;
using System.Collections.Generic;


public class CategoryControler : MonoBehaviour
{
    public List<ReactiveButton> Buttons;

    [SerializeField] ReactiveButton curSelected;
    [SerializeField] ReactiveButton prevSelected;
    private void Start()
    {
        SetSelected(Buttons[0]);
    }

    // Update is called once per frame
    void Update()
    {
        if(curSelected != prevSelected)
        {
            if (prevSelected)
            {
                prevSelected.Deselect();
                prevSelected.SetColor();
            }
            prevSelected = curSelected;
            curSelected.Select();
            curSelected.SetColor();
        }
    }

    public void SetSelected(ReactiveButton b)
    {
        curSelected = b;
        curSelected.SetColor();
    }

}
