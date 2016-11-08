//
//  IAPManager.h
//  Unity-iPhone
//
//  Created by 曾逸 on 11/7/16.
//
//
#pragma once
#import <StoreKit/StoreKit.h>
#import "IAPInterface.h"

@interface IAPManager : NSObject<SKProductsRequestDelegate, SKPaymentTransactionObserver>
{
    IAPInterface* CurrentActor;
    NSArray *Products;
}


+(IAPManager*)Instance;

-(void)addActor:(IAPInterface*) actor;

- (bool)CanMakePayment;

- (void)requestProductData:(NSArray* )productIDs;


-(void) SendPurchaseRequest:(NSString*)productid;

-(void) SendMutablePurchaseRequest:(NSString*) productid Num:(int) num;

- (NSString*) localizedPrice: (SKProduct *)product;


@end
