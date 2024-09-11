using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class InfoUI : MonoBehaviour
{
    RectTransform rect;
    [SerializeField]
    Image img;
    
    Sequence inactive;
    Sequence active;

    private void Start()
    {
        //rect = GetComponent<RectTransform>();
        //inactive.Prepend(rect.domove)

        gameObject.SetActive(false);
    }

    public void ShowCardInfo(int id)
    {
        /*
        cardName.text = info.cardName;
        flavorText.text = info.flavorText;
        description.text = info.description;
        */
        img.sprite = GameManager.Card(id).front;
        gameObject.SetActive(true);
    }

    public void Disable()
    {
        
    }
}
