using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{

    public class PaywallUsecaseView : MonoBehaviour
    {
        #region Events
        public event Action<List<string>>               OnRequestPaywallBillingProductDetails;
        public event Action<IBillingProduct, string>    OnRequestBuyBillingProduct;
        #endregion

        #region Fields

        [SerializeField] 
        private List<string>            m_paywallProductIds;

        [SerializeField]
        private Transform               m_productsContainer;

        [SerializeField]
        private PaywallProductCell      m_paywallProductCellPrefab;

        [SerializeField] 
        private Transform               m_processingOverlay;

        private BillingProductOffer     m_currentSelectedOffer;



        private List<(IBillingProduct product, PaywallProductCell cell)>    m_cachedMap = new();
        #endregion


        private void Start()
        {
            OnRequestPaywallBillingProductDetails?.Invoke(m_paywallProductIds);
        }

        #region Public methods

        public void UpdatePaywallBillingProductDetails(List<(IBillingProduct product, bool purchaseStatus)> products, Error error)
        {
            if(error != null)
            {
                MessagePrompt prompt = new("Error", error.Description, "Retry", () => OnRequestPaywallBillingProductDetails(m_paywallProductIds), "Cancel", null);
                prompt.Show();
                return;
            }

            PresentPaywallProducts(products);


            //paywallBillingProducts
            //Instantiate a cell to store the info.
            //It can show the following
                //Time period of the subscription
                //Price
                //Buy button
                //Items should have toggle marks which are grouped.
                //On selecting an item, we need to update the footer button with the details. 
                //On clicking the footer button we need to purchase the product.
        }

        private void PresentPaywallProducts(List<(IBillingProduct product, bool isPurchased)> products)
        {
            foreach((IBillingProduct product, bool isPurchased) in products)
            {
                PaywallProductCell cell = Instantiate(m_paywallProductCellPrefab, m_productsContainer);
                cell.SetDetails(product.LocalizedTitle, product.Type == BillingProductType.Subscription ? "Subscription" : "One-time purchase", product.LocalizedDescription);

                foreach(var offer in product.Offers)
                {
                    var title           = offer.Category == BillingProductOfferCategory.Introductory ? "Introductory offer" : "Promotional Offer";
                    var description     = GetDescriptionFromPricingPhases(offer.PricingPhases, product.Price, product.SubscriptionInfo?.Period);

                    cell.AddOffer(title, description, () => {
                        m_currentSelectedOffer = offer;
                        cell.SetPriceSummary(GetPricingSummary(product.Price, product.SubscriptionInfo?.Period, offer));
                    });
                }

                cell.SetPriceSummary(GetPricingSummary(product.Price, product.SubscriptionInfo?.Period, null));
                cell.OnBuyClicked += () => OnBuyButtonClicked(product, m_currentSelectedOffer?.Id);

                cell.SetBuyStatus(!isPurchased);


                //Cache
                m_cachedMap.Add((product, cell));
            }
        }

        public void UpdateTransaction(IBillingTransaction transaction)
        {
            //Still processing, so just do nothing.
            if(transaction.TransactionState == BillingTransactionState.Purchasing)
                return;

            //Dismiss the overlay if the purchase action is complete. While in purchasing state, it is still processing so we should keep the overlay
            SetProcessingOverlayStatus(false);

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

        #endregion

        #region Callbacks

        private void OnBuyButtonClicked(IBillingProduct billingProduct, string offerId)
        {
            
            SetProcessingOverlayStatus(true);

            //From the toggle group, get the active item and its tag. Tag is our IBillingProduct
            OnRequestBuyBillingProduct?.Invoke(billingProduct, offerId);
        }

        #endregion

        #region Helpers

        private void ReportPurcaseSuccessful(IBillingTransaction transaction)
        {
            //Tip: Use transaction.Product.Payouts to get the rewards related to this product.
            MessagePrompt prompt = new("Purchase successful!", $"Purchased {transaction.Product.Id} product with {transaction.PurchasedQuantity} quantity.", "Ok");
            prompt.Show();

            //Update buy status of the product cell
            if(transaction.Product.Type == BillingProductType.NonConsumable || transaction.Product.Type == BillingProductType.Subscription)
            {
                var cell = FindPaywallProductCell(transaction.Product);
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

        private PaywallProductCell FindPaywallProductCell(IBillingProduct product)
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

        private void SetProcessingOverlayStatus(bool isEnabled)
        {
            m_processingOverlay.gameObject.SetActive(isEnabled);
        }

        private string GetDescriptionFromPricingPhases(IOrderedEnumerable<BillingProductOfferPricingPhase> pricingPhases, BillingPrice normalPrice, BillingPeriod period)
        {

            List<string> descriptionList = new();

            string regularPricingInfo = $"{normalPrice.LocalizedText} per {period?.Duration} {period?.Unit}(s)";
            foreach(var phase in pricingPhases)
            {
                string phaseDescription = null;
                switch(phase.PaymentMode)
                {
                    case BillingProductOfferPaymentMode.FreeTrial:
                        phaseDescription = $"Free trial for {phase.Period.Duration} {phase.Period.Unit}(s)";
                        break;
                    case BillingProductOfferPaymentMode.PayAsYouGo:
                        phaseDescription = $"{phase.Price.LocalizedText} per {phase.Period.Duration} {phase.Period.Unit}(s) for {phase.RepeatCount} billing cycle(s)";
                        break;
                    case BillingProductOfferPaymentMode.PayUpFront:
                        phaseDescription = $"{phase.Price.LocalizedText} upfront for {phase.Period.Duration} {phase.Period.Unit}(s)";
                        break;
                    default:
                        break;
                }
                descriptionList.Add(phaseDescription);
            }

            descriptionList.Add(regularPricingInfo);          

            return string.Join(", then ", descriptionList);
        }

        private string GetPricingSummary(BillingPrice standardPrice, BillingPeriod period, BillingProductOffer offer)
        {
            List<string> descriptionList = new();
            if(offer != null)
            {
                var pricingPhases = offer.PricingPhases;

                foreach(var phase in pricingPhases)
                {
                    switch(phase.PaymentMode)
                    {
                        case BillingProductOfferPaymentMode.FreeTrial:
                            descriptionList.Add($"{phase.Period.Duration} {phase.Period.Unit}(s) Free trial");
                            break;
                        case BillingProductOfferPaymentMode.PayAsYouGo:
                            descriptionList.Add($"{phase.Price.LocalizedText} for {phase.RepeatCount} billing cycle(s)");
                            break;
                        case BillingProductOfferPaymentMode.PayUpFront:
                            descriptionList.Add($"{phase.Price.LocalizedText} upfront");
                            break;
                        default:
                            break;
                    }
                }
            }
            

            if(period == null)
            {
                descriptionList.Add($"{standardPrice.LocalizedText} lifetime");    
            }
            else
            {
                descriptionList.Add($"{standardPrice.LocalizedText} / {period.Duration} {period.Unit}(s)");
            }

            return string.Join(" -> ", descriptionList);
        }

        #endregion
    }
}
