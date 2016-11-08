//
//  IAPManager.m
//  Unity-iPhone
//
//  Created by 曾逸 on 11/7/16.
//
//

#import "IAPManager.h"

@implementation IAPManager

static IAPManager* _instance;

+(IAPManager*)Instance
{
    if(_instance==nil)
    {
        _instance=[[IAPManager alloc] init];
    }
    return _instance;
}

-(void)addActor:(IAPInterface *)actor
{
    CurrentActor=actor;
}


- (void)initionize
{
    [[SKPaymentQueue defaultQueue] addTransactionObserver:self];
}

- (bool)CanMakePayment
{
    return [SKPaymentQueue canMakePayments];
}


- (void)requestProductData:(NSArray* )productIDs
{
    NSSet *productIdentifiers = [NSSet setWithArray:productIDs];
    SKProductsRequest *CurrentRequest = [[SKProductsRequest alloc] initWithProductIdentifiers:productIdentifiers];
    CurrentRequest.delegate = self;
    [CurrentRequest start];
}

- (void)productsRequest:(SKProductsRequest *)request didReceiveResponse:(SKProductsResponse *)response
{
    Products = response.products;
    
    
    NSLog(@"products info received!");
    
    for (NSString *invalidProductId in response.invalidProductIdentifiers)
    {
        NSLog(@"Invalid product id: %@" , invalidProductId);
    }
    
    [CurrentActor SendProductsInfoToUnity:Products];
}

#pragma -
#pragma transaction

-(SKProduct*) getProduct:(NSString*) productid
{
    for (SKProduct *product in Products)
    {
        if ([product.productIdentifier isEqualToString:productid]) {
            return product;
        }
    }
    
    NSLog(@"Can't find id in products: %@" , productid);
    return nil;
}


-(void) SendPurchaseRequest:(NSString*)productid
{
    SKProduct* product=[self getProduct:productid];
    if(product==nil)
    {
        return;
    }
    SKPayment* payment=[SKPayment paymentWithProduct:product];
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}

-(void) SendMutablePurchaseRequest:(NSString*) productid Num:(int) num
{
    SKProduct* product=[self getProduct:productid];
    if(product==nil)
    {
        return;
    }
    
    SKMutablePayment* payment=[SKMutablePayment paymentWithProduct:product];
    payment.quantity=num;
    [[SKPaymentQueue defaultQueue] addPayment:payment];
}


-(void) paymentQueue:(SKPaymentQueue *)queue updatedTransactions:(NSArray<SKPaymentTransaction *> *)transactions
{
    for (SKPaymentTransaction* transaction in transactions) {
        switch (transaction.transactionState) {
            
            case SKPaymentTransactionStatePurchasing:
                [CurrentActor OnPurchasing:transaction];
                break;
            case SKPaymentTransactionStateDeferred:
                [CurrentActor OnDeferred:transaction];
                break;
            case SKPaymentTransactionStateFailed:
                [CurrentActor OnPurchasFailed:transaction];
                break;
            case SKPaymentTransactionStatePurchased:
                [CurrentActor OnPurchasSuccess:transaction];
                break;
            case SKPaymentTransactionStateRestored:
                [CurrentActor OnPurchasRestored:transaction];
                break;
            default:
                NSLog(@"Unexpected transaction state %@", @(transaction.transactionState));
                break;
        }
    }
}




- (NSString*) localizedPrice: (SKProduct *)product
{
    NSNumberFormatter *numberFormatter=[[NSNumberFormatter alloc] init];
    [numberFormatter setFormatterBehavior:NSNumberFormatterBehavior10_4];
    [numberFormatter setNumberStyle:NSNumberFormatterCurrencyStyle];
    [numberFormatter setLocale:product.priceLocale];
    NSString *result = [numberFormatter stringFromNumber:product.price];
    return result;
}

@end
