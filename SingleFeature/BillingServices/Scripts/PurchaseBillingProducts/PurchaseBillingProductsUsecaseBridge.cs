using UnityEngine;

namespace VoxelBusters.UseCases
{
    public class PurchaseBillingProductsUsecaseBridge : MonoBehaviour
    {
        [SerializeField]
        private PurchaseBillingProductsUsecase      m_controller;

        [SerializeField]
        private PurchaseBillingProductsUsecaseView  m_view;

        private void Awake()
        {
            //View action triggers
            m_view.OnRequestBillingProducts         += m_controller.InitializeStore;
            m_view.OnRequestBuyBillingProduct       += m_controller.BuyProduct;
            m_view.OnRequestRestorePurchases        += m_controller.RestorePurchases;

            m_controller.OnStoreInitialized         += m_view.PopulateBillingProductCells;
            m_controller.OnTransactionChange        += m_view.UpdateTransaction;
            m_controller.OnRestorePurchases         += m_view.UpdateRestorePurchases;
        }
    }
}
