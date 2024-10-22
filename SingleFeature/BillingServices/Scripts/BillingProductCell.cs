using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace VoxelBusters.UseCases
{   
    public class BillingProductCell : MonoBehaviour
    {
        [SerializeField] private TMP_Text   m_title;
        [SerializeField] private TMP_Text   m_description;
        [SerializeField] private TMP_Text   m_price;
        [SerializeField] private RawImage   m_image;
        [SerializeField] private Transform  m_purchaseStatus;
        [SerializeField] private TMP_Text   m_purchaseInfo;
        [SerializeField] private Button     m_buyButton;

        public Action OnBuyClicked;

        private void Start()
        {
            m_buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke());
        }

        public void SetData(string title, string description, string price, LazyTexture texture)
        {
            SetTitle(title);
            SetDescription(description);
            SetPrice(price);
            if(texture != null)
            {
                texture.OnTextureLoaded += SetImage;
            }

            SetPurchaseStatus(isPurchased: false, null);
        }

        public void SetPurchaseStatus(bool isPurchased, string info)
        {
            m_buyButton.interactable = !isPurchased;
            m_purchaseStatus.gameObject.SetActive(isPurchased);
            m_purchaseInfo.text = info;
        }

        private void SetTitle(string name)
        {
            m_title.text = name;
        }

        private void SetDescription(string description)
        {
            m_description.text = description;
        }

        private void SetPrice(string price)
        {
            m_price.text = price;
        }

        private void SetImage(Texture texture)
        {
            m_image.texture = texture;
        }
    }
}
