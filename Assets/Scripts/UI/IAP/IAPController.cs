using UnityEngine;
using System.Collections;

public class IAPController : MonoBehaviour, IAPUIDelegate
{

    public IAPMger IAPManager;

    public LotViewController LotViewCtl;

    public IAPView View;

    private const string productid = "Dream.NumPuzzle.Support1";
    private const int productScore = 2000;

    // Use this for initialization
    private void Start()
    {
        IAPManager.CurrentUIDelegate = this;
#if UNITY_IPHONE
        IAPManager.Init();
        IAPManager.QuestAllProductsInfo(productid);
#endif
    }

    //void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.S))
    //    {
    //        View.Show(false);
    //    }
    //}


    public void Purchase()
    {
#if UNITY_IPHONE
        if (IAPManager.IsPurchaseEnable())
        {
            IAPManager.SendPurchaseRequest(productid, 1);
        }
#endif
    }


    public void Show()
    {
        View.Show(true);

        View.SetDesc("Thanks for your Support!\n\nScore: " + productScore);

        if (IAPManager.GetProductinfo(productid)!=null)
        {
            View.SetPrice(IAPManager.GetProductinfo(productid).Price);
        }
    }


    public void Hide()
    {
        View.gameObject.SetActive(false);
    }


    public void OnProductsInfoReceived()
    {
        if (IAPManager.GetProductinfo(productid) != null)
        {
            Log.D(ELogTag.IAP, "products info received");
        }
        else
        {
            Log.E(ELogTag.IAP, "products info invalid");
        }
    }

    public void OnPurchaseSuccess(string id, int num)
    {
        if (id.Equals(productid))
        {
            LotViewCtl.SetCurScore(PlayerInfo.Instance().CurAtt.Gold + productScore);
        }
        Hide();
    }

    public void OnPurchaseFail(string productid, int num)
    {
        View.Show(false);
    }

    public void OnPurchaseRestored(string productid, int num)
    {

    }

    public void OnPurchaseing(string productid, int num)
    {

    }

    public void OnPurchaseDefer(string productid, int num)
    {

    }
}
