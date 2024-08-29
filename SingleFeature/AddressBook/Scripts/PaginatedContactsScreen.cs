using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{

    public class PaginatedContactsScreen : MonoBehaviour
    {
        [Header("Use Case")]
        [SerializeField]
        private ReadContactsPaginatedUseCase m_useCase;

        [Header("Config")]
        [SerializeField] private ReadContactsConstraint     m_constraints = ReadContactsConstraint.MustIncludeName;
        [SerializeField] private int                        m_resultsPerPage = 10;
        

        [Space]
        [Space]
        [Header("UI")]
        [SerializeField] private ContactInfoUI m_contactInfoUIPrefab;
        [SerializeField] private Transform m_contactListRoot;
        [SerializeField] private Button m_loadNextPage;
        [SerializeField] private Button m_loadPreviousPage;
        [SerializeField] private TMP_Text m_info;


        private VisualPrefabPool<ContactInfoUI>  m_contactInfoUIPool;
        private PageController              m_pageController;
        
        private void Start()
        {
            m_contactInfoUIPool = new VisualPrefabPool<ContactInfoUI>(m_contactInfoUIPrefab);
            m_pageController    = new PageController(m_resultsPerPage, 0);

            m_loadNextPage.onClick.AddListener(OnLoadNextPageButtonClicked);
            m_loadPreviousPage.onClick.AddListener(OnLoadPreviousPageButtonClicked);

            UpdateButtonStatusBasedOnPageOffset();
            ReadContacts();
        }

        private void ReadContacts()
        {
            m_useCase.ReadContacts(limit: m_resultsPerPage, offset: m_pageController.GetCurrentOffset(), constraints: m_constraints, callback: OnReadContactsFinish);
        }

        private void OnReadContactsFinish(IAddressBookContact[] contacts, int nextOffset, Error error)
        {
            if(error == null)
            {
                ClearCurrentPage();
                
                for (int iter = 0; iter < contacts.Length; iter++)
                {
                    IAddressBookContact contact = contacts[iter];
                    ContactInfoUI cell = m_contactInfoUIPool.Get();
                    cell.transform.SetParent(m_contactListRoot, false);
                    UpdateContactInfoUI(contact, cell);
                }  
            }
            else
            {
                UpdateInfo(error.Description);
            }

            m_pageController.UpdateNextOffset(nextOffset);
            UpdateButtonStatusBasedOnPageOffset();
        }

        private void UpdateButtonStatusBasedOnPageOffset()
        {
            m_loadNextPage.interactable      = m_pageController.HasNextPage();
            m_loadPreviousPage.interactable  = m_pageController.HasPreviousPage();
        }

        private void OnLoadNextPageButtonClicked()
        {
            m_pageController.MoveToNextPage();
            ReadContacts();
        }

        private void OnLoadPreviousPageButtonClicked()
        {
            m_pageController.MoveToPreviousPage();
            ReadContacts();
        }

        #region Helpers

        private void UpdateContactInfoUI(IAddressBookContact contact, ContactInfoUI cell)
        {      
            LazyTexture lazyDisplayPicture = new LazyTexture();

            cell.SetData(contact.FirstName, contact.PhoneNumbers, contact.EmailAddresses, lazyDisplayPicture);
            cell.OnInviteButtonClicked = OnInviteButtonClicked;

            contact.LoadImage((textureData, error) =>
            {
                lazyDisplayPicture.SetTexture(textureData.GetTexture());
            });
        }

        private void ClearCurrentPage()
        { 
            UpdateInfo("");
            foreach (Transform child in m_contactListRoot)
            {
                m_contactInfoUIPool.Release(child.GetComponent<ContactInfoUI>());
            }
        }

        private void OnInviteButtonClicked(string name, string phoneNumber, string email)
        {
            Debug.Log($"Invite {name} {phoneNumber} {email}");
        }

        private void UpdateInfo(string info)
        {
            m_info.text = info;
        }

        #endregion
    }
}
