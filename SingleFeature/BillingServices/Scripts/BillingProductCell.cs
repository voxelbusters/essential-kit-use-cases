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

            SetBuyStatus(true);
        }

        public void SetBuyStatus(bool enable)
        {
            m_buyButton.interactable = enable;
            m_purchaseStatus.gameObject.SetActive(!enable);
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
