using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelBusters.UseCases
{
    public class PaywallProductCell : MonoBehaviour
    {
        [SerializeField] private TMP_Text   m_title;
        [SerializeField] private TMP_Text   m_subTitle;
        [SerializeField] private TMP_Text   m_description;
        [SerializeField] private Transform  m_purchaseStatus;
        [SerializeField] private Button     m_buyButton;
        [SerializeField] private TMP_Text   m_priceSummary;

        [SerializeField] private ProductOfferCell   m_offerCellPrefab;
        [SerializeField] private Transform          m_offersContainer;
        [SerializeField] private ToggleGroup        m_offersToggleGroup;
        [SerializeField] private GameObject         m_noOffersAvailableInfo;
        
        public  Action  OnBuyClicked;

        private void Start()
        {
            m_buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke());
            m_offersToggleGroup.SetAllTogglesOff();
        }
        
        public void SetDetails(string title, string subTitle, string description)
        {
            m_title.text        = title;
            m_subTitle.text     = subTitle;
            m_description.text  = description;  
        }

        public void SetPriceSummary(string summary)
        {
            m_priceSummary.text = summary;
        }

        public void AddOffer(string offerTitle, string offerDescription, Action offerSelectionCallback)
        {
            var offerCell = Instantiate(m_offerCellPrefab, m_offersContainer);
            offerCell.SetDetails(offerTitle, offerDescription);  
            offerCell.OnSelectionChanged += (isOn) => {
                                                        if(isOn)
                                                        {
                                                            offerSelectionCallback?.Invoke();
                                                        }
                                            }; 

            Debug.Log("Toggle : " + offerCell.GetComponentInChildren<Toggle>());    

            offerCell.AssignToggleGroup(m_offersToggleGroup);                     

            //Deactivating the no offers text
            m_noOffersAvailableInfo.SetActive(false);     
        }

        public void SetBuyStatus(bool isEnabled)
        {
            m_buyButton.interactable = isEnabled;
            m_purchaseStatus.gameObject.SetActive(!isEnabled);
        }
    }
}
