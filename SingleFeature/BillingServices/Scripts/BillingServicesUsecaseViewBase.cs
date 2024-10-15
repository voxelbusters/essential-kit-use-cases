using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class BillingServicesUsecaseViewBase : MonoBehaviour
    {
        [Space]
        [Header("Common UI")]
        [SerializeField] protected  BillingProductCell      m_billingProductCellPrefab; 
        [SerializeField] protected  Transform               m_billingProductsContainer;
        [SerializeField] protected  Transform               m_processingOverlay;


        #region Common methods

        protected BillingProductCell GetBillingProductCell(IBillingProduct product)
        {
            BillingProductCell cell = Instantiate(m_billingProductCellPrefab);
            cell.SetData(product.LocalizedTitle, product.LocalizedDescription, product.Price.LocalizedText, null);
            return cell;
        }

        protected void SetProcessingOverlayStatus(bool isEnabled)
        {
            m_processingOverlay.gameObject.SetActive(isEnabled);
        }

        #endregion
    }
}
