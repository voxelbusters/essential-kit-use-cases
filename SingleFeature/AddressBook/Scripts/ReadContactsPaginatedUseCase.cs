using System;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class ReadContactsPaginatedUseCase : MonoBehaviour
    {
        public void ReadContacts(int limit, int offset, ReadContactsConstraint constraints, Action<IAddressBookContact[], int, Error> callback)
        {
            ReadContactsOptions options = new ReadContactsOptions.Builder()
                                                .WithLimit(limit)
                                                .WithOffset(offset)
                                                .WithConstraints(constraints)
                                                .Build();

            AddressBook.ReadContacts(options, (result, error) => {
                callback(result.Contacts, result.NextOffset, error);
            });
        }
    }
}
