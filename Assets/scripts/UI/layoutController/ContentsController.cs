using UnityEngine;
using System.Collections.Generic;

public class ContentsController : MonoBehaviour
{
    public List<RectTransform> Contents;

    public void SetTop(int idx)
    {
        int i = 0;
        foreach(var con in Contents)
        {
            if(i == idx)
            {
                con.SetAsLastSibling();
                con.gameObject.SetActive(true);
            }
            else
            {
                con.gameObject.SetActive(false);
            }
            i++;
        }
    }
}
