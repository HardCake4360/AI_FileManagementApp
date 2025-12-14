using UnityEngine;
using System.Collections.Generic;

public class TagsList : MonoBehaviour
{
    [SerializeField] GameObject TagPrefab;
    public void ClearList()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
    public void SetList(List<string> tags)
    {
        ClearList();
        foreach(var tag in tags)
        {
            var tagObj = Instantiate(TagPrefab, gameObject.transform);
            tagObj.GetComponent<TagPrefab>().SetText(tag);
        }

    }
}
