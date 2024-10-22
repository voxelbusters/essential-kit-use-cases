using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelBusters.UseCases
{
    public class ProductOfferCell : MonoBehaviour
    {
        [SerializeField] private TMP_Text   m_title;
        [SerializeField] private TMP_Text   m_description;
        [SerializeField] private Toggle     m_toggle;

        public Action<bool> OnSelectionChanged;

        private void Start()
        {
            m_toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }


        public void SetDetails(string title, string description)
        {
            m_title.text = title;
            m_description.text = description;
        }

        public void AssignToggleGroup(ToggleGroup toggleGroup)
        {
            m_toggle.group = toggleGroup;
        }

        private void OnToggleValueChanged(bool isOn)
        {
            OnSelectionChanged?.Invoke(isOn);
        }

        
    }
}
