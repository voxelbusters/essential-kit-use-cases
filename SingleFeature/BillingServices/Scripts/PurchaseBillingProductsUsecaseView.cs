using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class PurchaseBillingProductsUsecaseView : BillingServicesUsecaseViewBase
    {
        #region Events
        public event Action                         OnRequestBillingProducts;
        public event Action<IBillingProduct, int>   OnRequestBuyBillingProduct;
        public event Action<bool>                   OnRequestRestorePurchases;
        #endregion

        #region Fields

        [SerializeField] private Button             m_restorePurchasesButton;

        private (IBillingProduct product, BillingProductCell cell)?         m_currentPurchasingProduct;
        private List<(IBillingProduct product, BillingProductCell cell)>    m_cachedMap = new List<(IBillingProduct product, BillingProductCell cell)>();

        #endregion


        private void Start()
        {
            m_restorePurchasesButton.onClick.AddListener(OnRestorePurchasesButtonClicked);

            OnRequestBillingProducts?.Invoke();
        }

        #region Public methods

        public void PopulateBillingProductCells(IBillingProduct[] products, Error error)
        {
            if(error != null)
            {
                MessagePrompt prompt = new("Error", error.Description, "Retry", () => OnRequestBillingProducts(), "Cancel", null);
                prompt.Show();
                return;
            }

            foreach(IBillingProduct billingProduct in products)
            {
                //Instantiate the product cell
                BillingProductCell cell = GetBillingProductCell(billingProduct);
                cell.OnBuyClicked += () => OnBuyButtonClicked(cell, billingProduct);
                
                //Get the container where to place this product (In this demo we group the UI based on product types)
                cell.transform.SetParent(m_billingProductsContainer, false);

                //Adding for future use to find a cell related to a billing product
                m_cachedMap.Add((billingProduct, cell));
            }

            //Request restore purchases to update the cell buy status. Passing true should be done only if its a user action as it may prompt a login dialog, else false.
            OnRequestRestorePurchases?.Invoke(false);
        }

        public void UpdateTransaction(IBillingTransaction transaction)
        {
            //Still processing, so just do nothing.
            if(transaction.TransactionState == BillingTransactionState.Purchasing)
                return;

            //Dismiss the overlay if the purchase action is complete. While in purchasing state, it is still processing so we should keep the overlay
            HandleProcessingOverlay(transaction);

            //Check if the transaction state is successful along with verification state
            if (transaction.TransactionState == BillingTransactionState.Purchased &&
                transaction.ReceiptVerificationState == BillingReceiptVerificationState.Success)
            {
                //Allocate the reward
                ReportPurcaseSuccessful(transaction);
            }
            else
            {
                //Report user based on TransactionStatus (Failed/Deferred/Refunded)
                if(transaction.Error != null)
                {
                    ReportPurchaseFailed(transaction);
                }
                else
                {
                    Debug.Log("Handle based on the transaction state.");
                }
            }
        }

        public void UpdateRestorePurchases(IBillingTransaction[] transactions, Error error)
        {
            if(error != null)
            {
                MessagePrompt prompt = new("Error", error.Description, "Retry", () => OnRequestRestorePurchases(true), "Cancel", null);
                prompt.Show();
                return;
            }


            foreach(var transaction in transactions)
            {
                //We can restore purchases only for non-consumable and subscription type products
                if(transaction.Product.Type == BillingProductType.NonConsumable || transaction.Product.Type == BillingProductType.Subscription)
                {
                    var cell = FindBillingProductCell(transaction.Product);
                    var isPurchasedAndVerified = transaction.TransactionState           == BillingTransactionState.Purchased && 
                                                transaction.ReceiptVerificationState    == BillingReceiptVerificationState.Success;

                    //Allow to purchase only if its not yet done!                                                
                    cell.SetBuyStatus(!isPurchasedAndVerified);    
                }
            }
        }

        #endregion

        #region Callbacks

        private void OnBuyButtonClicked(BillingProductCell cell, IBillingProduct billingProduct)
        {
            //Cache the billing product info (along with cell) and show a full screen overlay with some processing info...
            m_currentPurchasingProduct = (billingProduct, cell);
            SetProcessingOverlayStatus(true);

            OnRequestBuyBillingProduct?.Invoke(billingProduct, 1);
        }


        private void OnRestorePurchasesButtonClicked()
        {
            //Pass forceRefresh as true for user triggered action.
            OnRequestRestorePurchases?.Invoke(true);
        }

        #endregion

        #region Helpers

        private void HandleProcessingOverlay(IBillingTransaction transaction)
        {
            if (IsCurrentPurchasingProduct(transaction.Product.Id))
            {
                SetProcessingOverlayStatus(false);
                m_currentPurchasingProduct = null;
            }
        }

        private bool IsCurrentPurchasingProduct(string productId)
        {
            return m_currentPurchasingProduct != null && m_currentPurchasingProduct.Value.product.Id.Equals(productId);
        }

        private void ReportPurcaseSuccessful(IBillingTransaction transaction)
        {
            //Tip: Use transaction.Product.Payouts to get the rewards related to this product.
            MessagePrompt prompt = new("Purchase successful!", $"Purchased {transaction.Product.Id} product with {transaction.PurchasedQuantity} quantity.", "Ok");
            prompt.Show();

            //Update buy status of the product cell
            if(transaction.Product.Type == BillingProductType.NonConsumable || transaction.Product.Type == BillingProductType.Subscription)
            {
                var cell = FindBillingProductCell(transaction.Product);
                var alreadyPurchased = transaction.TransactionState == BillingTransactionState.Purchased && transaction.ReceiptVerificationState == BillingReceiptVerificationState.Success;
                cell.SetBuyStatus(!alreadyPurchased);
            }
        }

        private void ReportPurchaseFailed(IBillingTransaction transaction)
        {
            var error = transaction.Error;
            MessagePrompt prompt = new("Error", error.Description, "Ok");
            prompt.Show();
        }

        private BillingProductCell FindBillingProductCell(IBillingProduct product)
        {
            foreach(var cachedProduct in m_cachedMap)
            {
                if(cachedProduct.product.Id == product.Id)
                {
                    return cachedProduct.cell;
                }
            }

            return null;
        }

        #endregion
    }
}
