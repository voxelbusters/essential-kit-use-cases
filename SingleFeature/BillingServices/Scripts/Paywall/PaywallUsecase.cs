using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class PaywallUsecase : MonoBehaviour
    {
        #region Events
        public event Action<List<(IBillingProduct product, bool isPurchased)>, Error>       OnFetchProductDetails;
        public event Action<IBillingTransaction>    OnTransactionChange;
        #endregion

        #region Fields

        private List<string> m_cachedProductIds;

        #endregion


        #region  Handle events

        private void OnEnable()
        {
            BillingServices.OnInitializeStoreComplete   += OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange    += OnTransactionStateChange;
        }

        private void OnDisable()
        {
            BillingServices.OnInitializeStoreComplete   -= OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange    -= OnTransactionStateChange;
        }


        #endregion

        #region  Public methods
        public void FetchProductDetails(List<string> productIds)
        {
            m_cachedProductIds = productIds;

            if(BillingServices.Products.IsNullOrEmpty())
            {
                BillingServices.InitializeStore();
            }
            else
            {
                ReportFetchedProductDetails(BillingServices.Products, null);
            }
        }

        public void BuyProduct(IBillingProduct billingProduct, string offerId)
        {
            //Create BuyProductOptions
            var optionsBuilder = new BuyProductOptions.Builder();
            optionsBuilder.SetQuantity(1);

            if(!string.IsNullOrEmpty(offerId))
            {
                BillingProductOfferRedeemDetails redeemDetails = GetOfferRedeemDetails(offerId);
                optionsBuilder.SetOfferRedeemDetails(redeemDetails);
            }
            
            optionsBuilder.Build();
            BuyProductOptions options = optionsBuilder.Build();

            //Start purchasing
            BillingServices.BuyProduct(billingProduct, options);
        }

        public void RestorePurchases(bool forceRefresh)
        {
            BillingServices.RestorePurchases(forceRefresh);
        }

        #endregion

        
        #region Callbacks

        private void OnInitializeStoreComplete(BillingServicesInitializeStoreResult result, Error error)
        {
             if(result.InvalidProductIds != null && result.InvalidProductIds.Length > 0)
            {
                Debug.LogWarning("You have invalid products listed as productIds: " + string.Join(",", result.InvalidProductIds));
            }

            ReportFetchedProductDetails(result.Products, error);
        }

        private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
        {
            foreach(var transaction in result.Transactions)
            {
                OnTransactionChange?.Invoke(transaction);
            }
            
        }

        #endregion

        #region Helpers

        private void ReportFetchedProductDetails(IBillingProduct[] products, Error error)
        {
            List<(IBillingProduct product, bool isPurchased)> requestedProducts = new();

            foreach(IBillingProduct product in products)
            {
                if(m_cachedProductIds.Contains(product.Id))
                {
                    requestedProducts.Add((product, isPurchased: BillingServices.IsProductPurchased(product)));
                }
            }

            OnFetchProductDetails?.Invoke(requestedProducts, error);
        }

        private BillingProductOfferRedeemDetails GetOfferRedeemDetails(string offerId)
        {            
            if(string.IsNullOrEmpty(offerId))
            {
                return null;
            }

            BillingProductOfferRedeemDetails.Builder builder = new BillingProductOfferRedeemDetails.Builder();

            if (Application.platform == RuntimePlatform.Android)
            {
                builder.SetAndroidPlatformProperties(offerId);
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                builder.SetIosPlatformProperties(offerId, keyId: null, nonce: null, signature: null, timestamp: 0);//Fill in the details here by injecting the values
            }

            return builder.Build();
        }

        #endregion

    }
}
