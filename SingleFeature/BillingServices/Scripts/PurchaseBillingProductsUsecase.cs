using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class PurchaseBillingProductsUsecase : MonoBehaviour
    {
        #region Events
        public event Action<IBillingProduct[], Error>       OnStoreInitialized;
        public event Action<IBillingTransaction>            OnTransactionChange;
        public event Action<IBillingTransaction[], Error>   OnRestorePurchases;
        #endregion


        #region  Handle events

        private void OnEnable()
        {
            BillingServices.OnInitializeStoreComplete   += OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange    += OnTransactionStateChange;
            BillingServices.OnRestorePurchasesComplete  += OnRestorePurchasesComplete;
        }

        private void OnDisable()
        {
            BillingServices.OnInitializeStoreComplete   -= OnInitializeStoreComplete;
            BillingServices.OnTransactionStateChange    -= OnTransactionStateChange;
            BillingServices.OnRestorePurchasesComplete  -= OnRestorePurchasesComplete;
        }


        #endregion

        #region  Public methods
        public void InitializeStore()
        {
            BillingServices.InitializeStore();
        }

        public void BuyProduct(IBillingProduct billingProduct, int quantity)
        {
            //Create BuyProductOptions
            var optionsBuilder = new BuyProductOptions.Builder();
            optionsBuilder.SetQuantity(quantity); //Setting quantity only works on iOS but not on Android. On Android you need to check in the transaction for PurchasedQuatity to see if user purchased the required amount or not.
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

            OnStoreInitialized?.Invoke(result.Products, error);
        }

        private void OnTransactionStateChange(BillingServicesTransactionStateChangeResult result)
        {
            foreach(var transaction in result.Transactions)
            {
                OnTransactionChange?.Invoke(transaction);
            }
            
        }

        private void OnRestorePurchasesComplete(BillingServicesRestorePurchasesResult result, Error error)
        {
            OnRestorePurchases?.Invoke(result.Transactions, error);
        }

        #endregion
    }
}
