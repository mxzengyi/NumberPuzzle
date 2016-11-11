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
    NSMutableString* result=[[NSMutableString alloc]init];
    // app special
    for (SKProduct *product in products)
    {
        NSLog(@"Product title: %@" , product.localizedTitle);
        NSLog(@"Product description: %@" , product.localizedDescription);
        NSLog(@"Product price: %@" , product.price);
        NSLog(@"Product id: %@" , product.productIdentifier);
        
        [result appendFormat:@"%@/t",product.productIdentifier];
        [result appendFormat:@"%@/t",product.localizedTitle];
        [result appendFormat:@"%@/t",product.localizedDescription];
        [result appendFormat:@"%@/p",[[IAPManager Instance] localizedPrice:product]];
    }
    UnitySendMessage("IAPManager", "SetProducts", [result cStringUsingEncoding:NSUTF8StringEncoding]);
}


-(void)OnPurchasing:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction runing with productid: %@" , transaction.payment.productIdentifier);
    
    [self SendUnityPurchaseState:4 Transaction:transaction];
}

-(void)OnDeferred:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction defer with productid: %@" , transaction.payment.productIdentifier);
    
    [self SendUnityPurchaseState:5 Transaction:transaction];
}

-(void)OnPurchasFailed:(SKPaymentTransaction*) transaction
{
    NSLog(@"transaction fail with productid: %@" , transaction.payment.productIdentifier);
    NSLog(@"transaction fail with error: %ld" , transaction.error.code);
    
    [self SendUnityPurchaseState:2 Transaction:transaction];
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}


-(void)OnPurchasSuccess:(SKPaymentTransaction*) transaction
{
    // app special
    //验证凭证
    //保存凭证
    
    NSLog(@"transaction success with productid: %@" , transaction.originalTransaction.payment.productIdentifier);
    [self SendUnityPurchaseState:1 Transaction:transaction];
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}

-(void)OnPurchasRestored:(SKPaymentTransaction*) transaction
{
    
    NSLog(@"transaction restore with productid: %@" , transaction.originalTransaction.payment.productIdentifier);
    
    [self SendUnityPurchaseState:3 Transaction:transaction];
    [[SKPaymentQueue defaultQueue] finishTransaction:transaction];
}

-(void)SendUnityPurchaseState:(int) state Transaction:(SKPaymentTransaction*) transaction
{
    NSMutableString* result=[[NSMutableString alloc]init];
    [result appendFormat:@"%@/t",transaction.payment.productIdentifier];
    [result appendFormat:@"%ld/t",(long)transaction.payment.quantity];
    [result appendFormat:@"%d",state];
    UnitySendMessage("IAPManager", "SetPuchaseState", [result cStringUsingEncoding:NSUTF8StringEncoding]);
}

@end


