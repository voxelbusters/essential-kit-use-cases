using UnityEngine;

namespace VoxelBusters.UseCases
{
    public class PaywallUsecaseBridge : MonoBehaviour
    {
        [SerializeField]
        private PaywallUsecase      m_controller;

        [SerializeField]
        private PaywallUsecaseView  m_view;

        private void Awake()
        {
            //View action triggers
            m_view.OnRequestPaywallBillingProductDetails    += m_controller.FetchProductDetails;
            m_view.OnRequestBuyBillingProduct               += m_controller.BuyProduct;

            m_controller.OnFetchProductDetails              += m_view.UpdatePaywallBillingProductDetails;
            m_controller.OnTransactionChange                += m_view.UpdateTransaction;
        }
    }
}
