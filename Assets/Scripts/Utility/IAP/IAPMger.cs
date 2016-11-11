using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public interface IAPUIDelegate
{
    void OnProductsInfoReceived();
    void OnPurchaseSuccess(string productid, int num);
    void OnPurchaseFail(string productid, int num);
    void OnPurchaseRestored(string productid, int num);
    void OnPurchaseing(string productid, int num);
    void OnPurchaseDefer(string productid, int num);

}


public class IAPMger : MonoBehaviour
{
    public IAPUIDelegate CurrentUIDelegate;
    
	[DllImport ("__Internal")]
    private static extern bool CanUserPurchase();

    [DllImport("__Internal")]
    private static extern void InitIAP();

    [DllImport("__Internal")]
    private static extern void QuestProductsInfo(string productsID);

    [DllImport("__Internal")]
    private static extern void PurchasRequest(string productid, int num);

    private List<ProductInfo> _products; 


    public bool IsPurchaseEnable()
    {
        return CanUserPurchase();
    }

    public void Init()
    {
        _products=new List<ProductInfo>();
        InitIAP();
    }

    //请求都是异步
    public void QuestAllProductsInfo(string productsID)
    {
        QuestProductsInfo(productsID);
    }

    public void SendPurchaseRequest(string productid, int num)
    {
        PurchasRequest(productid, num);
    }


    string productSpliter = "/p";
    string infoSpliter = "/t";
    // IOS UnitySendMessage,游戏内不要调用
    public void SetProducts(string productsInfo)
    {
        string[] Spliter1 ={ productSpliter };
        string[] Spliter2 = { infoSpliter };
        string[] infos = productsInfo.Split(Spliter1, StringSplitOptions.RemoveEmptyEntries);
        foreach (string info in infos)
        {
            string[] productinfos = info.Split(Spliter2, StringSplitOptions.RemoveEmptyEntries);
            if (productinfos.Count()!=4)
            {
                Log.E(ELogTag.IAP, "Product info error! " + productsInfo);
                return;
            }

            ProductInfo currentinfo=new ProductInfo();
            currentinfo.ID = productinfos[0];
            currentinfo.Title = productinfos[1];
            currentinfo.Desc = productinfos[2];
            currentinfo.Price = productinfos[3];

            _products.Add(currentinfo);
        }

        if (CurrentUIDelegate!=null)
        {
            CurrentUIDelegate.OnProductsInfoReceived();
        }
        
    }


    public void SetPuchaseState(string info)
    {
        string[] Spliter2 = { infoSpliter };
        string[] infos = info.Split(Spliter2, StringSplitOptions.RemoveEmptyEntries);

        if (infos.Count()!=3)
        {
            Log.E(ELogTag.IAP,"Purchase State Error! Info="+info);
        }
        OnPurchaseStatesChange(infos[0], int.Parse(infos[1]), int.Parse(infos[2]));
    }



    private void OnPurchaseStatesChange(string productid, int num,int states)
    {
        switch (states)
        {
            case 1:
                //成功
                CurrentUIDelegate.OnPurchaseSuccess(productid,num);
                break;
            case 2:
                //失败
                CurrentUIDelegate.OnPurchaseFail(productid, num);
                break;
            case 3:
                //重发
                CurrentUIDelegate.OnPurchaseRestored(productid, num);
                break;
            case 4:
                //正在购买
                CurrentUIDelegate.OnPurchaseing(productid, num);
                break;
            case 5:
                //ask to buy
                CurrentUIDelegate.OnPurchaseDefer(productid, num);
                break;
            default:
                break;
        }
    }



    public ProductInfo GetProductinfo(string id)
    {
        foreach (ProductInfo productInfo in _products)
        {
            if (productInfo.ID.Equals(id))
            {
                return productInfo;
            }
        }
        return null;
    }


    public List<ProductInfo> GetProductsInfo()
    {
        return _products;
    }
}

public class ProductInfo
{
    public string ID;

    public string Title;

    public string Desc;

    //本地化的价格
    public string Price;
}
