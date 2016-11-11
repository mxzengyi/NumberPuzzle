//
//  IAPInterface.h
//  Unity-iPhone
//
//  Created by 曾逸 on 11/7/16.
//
//
#pragma once

#import <StoreKit/StoreKit.h>

@interface IAPInterface : NSObject

+(IAPInterface*)Instance;


-(void) SendProductsInfoToUnity:(NSArray*) products;



//transaction state callback
-(void) OnPurchasing:(SKPaymentTransaction*) transaction;
-(void) OnDeferred:(SKPaymentTransaction*) transaction;
-(void) OnPurchasFailed:(SKPaymentTransaction*) transaction;
-(void) OnPurchasSuccess:(SKPaymentTransaction*) transaction;
-(void) OnPurchasRestored:(SKPaymentTransaction*) transaction;



@end


extern "C" bool CanUserPurchase();


extern "C" void InitIAP();

//productsid format: ID1/tID2/tID3......
//products info will be returned by method sendProductsInfoToUnity
extern "C" void QuestProductsInfo(const char* productsID);


extern "C" void PurchasRequest(const char* productid, int num);