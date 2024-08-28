using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace VoxelBusters.UseCases
{   
    public class ContactInfoUI : MonoBehaviour
    {
        [SerializeField] private RawImage m_displayPicture;
        [SerializeField] private TMP_Text m_name;
        [SerializeField] private TMP_Text m_phoneNum;
        [SerializeField] private TMP_Text m_email;
        [SerializeField] private Button m_inviteButton;

        public Action<string, string, string> OnInviteButtonClicked;

        private void Start()
        {
            m_inviteButton.onClick.AddListener(() => OnInviteButtonClicked?.Invoke(m_name.text, m_phoneNum.text, m_email.text));
        }

        public void SetData(string name, string[] phoneNumbers, string[] emails, LazyTexture displayPicture)
        {
            SetName(name);
            SetPhoneNumbers(phoneNumbers);
            SetEmails(emails);
            displayPicture.OnTextureLoaded += SetDisplayPicture;
        }

        private void SetName(string name)
        {
            m_name.text = name;
        }

        private void SetPhoneNumbers(string[] phoneNumbers)
        {
            m_phoneNum.text = phoneNumbers.Length > 0 ? phoneNumbers[0] : "---";
        }

        private void SetEmails(string[] emails)
        {
            m_email.text = emails.Length > 0 ? emails[0] : "---";
        }

        private void SetDisplayPicture(Texture displayPicture)
        {
            m_displayPicture.texture = displayPicture;
        }
    }
}
