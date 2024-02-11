using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleListView1_Item : MonoBehaviour
{
    public interface IDeleteRequest
    {
        public ref List<SimpleListView1_Item> onDeleteRequest(int itemId);
    }

    [Header("UI")]
    private RectTransform rootTransform;
    public Image uiIcon;
    public TMP_Text uiTitle;
    public TMP_Text uiDescription;

    [Header("Interfaces")]
    private IDeleteRequest requestInterface;

    /// <summary>Fill title, description, icon and create local parameter "List<SimpleListView1_Item>" and implement "IDeleteRequest" to class </summary>
    public void init(string title, string description, Sprite icon, List<SimpleListView1_Item> linkedList, IDeleteRequest requestInterface)
    {
        rootTransform = gameObject.GetComponent<RectTransform>();
        uiTitle.text = title;
        uiDescription.text = description;
        uiIcon.sprite = icon;

        this.requestInterface = requestInterface;

        //Add to list
        order(linkedList.Count);
        linkedList.Add(this);
    }

    ///<summary>Function used for naming and ordering item.</summary>
    public void order(int id)
    {
        rootTransform.anchoredPosition = new Vector2(10, -10 - id * 130);
        gameObject.name = "" + id;
    }

    public void delete() //Button controlled
    {
        int parsedId = int.Parse(gameObject.name);
        List<SimpleListView1_Item> eventItemList = requestInterface.onDeleteRequest(parsedId);

        //Refresh all orders
        for (int i = 0; i < eventItemList.Count; i++)
        {
            if (i > parsedId)
            {
                eventItemList[i].order(i - 1);
            }
        }

        eventItemList.RemoveAt(parsedId); //Pernamently remove from list

        Destroy(gameObject); //Goodbye
    }
}
