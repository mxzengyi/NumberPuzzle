//
//  IAPInterface.m
//  Unity-iPhone
//
//  Created by 曾逸 on 11/7/16.
//
//

#import "IAPInterface.h"
#import "IAPManager.h"



extern "C" bool CanUserPurchase()
{
    return [[IAPManager Instance] CanMakePayment];
}


extern "C" void InitIAP()
{
    [[SKPaymentQueue defaultQueue] addTransactionObserver:[IAPManager Instance]];
    [[IAPManager Instance] addActor:[IAPInterface Instance]];
}


extern "C" void QuestProductsInfo(const char* productsID)
{
    NSArray* productsid=[[NSString stringWithUTF8String:productsID] componentsSeparatedByString:@"/t"];
    
    [[IAPManager Instance] requestProductData:productsid];
    
}


extern "C" void PurchasRequest(const char* productid, int num)
{
    NSString* Productid=[NSString stringWithUTF8String:productid];
    if (num==1) {
        [[IAPManager Instance] SendPurchaseRequest:Productid];
    }
    else
    {
        [[IAPManager Instance] SendMutablePurchaseRequest:Productid Num:num];
    }
}




@implementation IAPInterface



static IAPInterface* _instance;

+(IAPInterface*)Instance
{
    if(_instance==nil)
    {
        _instance=[[IAPInterface alloc] init];
    }
    return _instance;
}


-(void)SendProductsInfoToUnity:(NSArray *)products
{
    // app special
    for (SKProduct *product in products)
    {
        NSLog(@"Product title: %@" , product.localizedTitle);
        NSLog(@"Product description: %@" , product.localizedDescription);
        NSLog(@"Product price: %@" , product.price);
        NSLog(@"Product id: %@" , product.productIdentifier);
    }
}


-(void)OnPurchasing:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction runing with productid: %@" , transaction.payment.productIdentifier);
}

-(void)OnDeferred:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction defer with productid: %@" , transaction.payment.productIdentifier);
}

-(void)OnPurchasFailed:(SKPaymentTransaction*) transaction
{
        NSLog(@"transaction fail with productid: %@" , transaction.payment.productIdentifier);
        NSLog(@"transaction fail with error: %ld" , transaction.error.code);
        [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}


-(void)OnPurchasSuccess:(SKPaymentTransaction*) transaction
{
    // app special
    //验证凭证
    //保存凭证
    
    
    NSLog(@"transaction success with productid: %@" , transaction.originalTransaction.payment.productIdentifier);
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}

-(void)OnPurchasRestored:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction restore with productid: %@" , transaction.originalTransaction.payment.productIdentifier);
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}



@end


