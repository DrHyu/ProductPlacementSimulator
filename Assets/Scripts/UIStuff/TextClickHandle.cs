using UnityEngine;
using UnityEngine.EventSystems;


public class TextClickHandle : MonoBehaviour, IPointerClickHandler
{
    private int ID;

    public void setID(int _ID)
    {
        ID = _ID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        GetComponentInParent<TextScrollView>().ProcessClick(ID);
    }

}