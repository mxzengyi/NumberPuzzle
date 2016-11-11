using UnityEngine;
using UnityEngine.UI;

public class IAPView : MonoBehaviour
{

    public Text ProductTitle;

    public Text ProductDesc;

    public Text ProductPrice;

    public RectTransform purchasPage;

    public RectTransform FailedPage;

    public void SetDesc(string desc)
    {
        ProductDesc.text = desc;
    }

    public void SetPrice(string price)
    {
        ProductPrice.text = price;
    }


    public void Show(bool success)
    {
        gameObject.SetActive(true);
        purchasPage.gameObject.SetActive(success);
        FailedPage.gameObject.SetActive(!success);
    }

}
