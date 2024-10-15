using System;
using VoxelBusters.CoreLibrary;
using VoxelBusters.EssentialKit;

namespace VoxelBusters.UseCases
{
    public class MessagePrompt
    {
        private AlertDialog m_alertDialog;

        public MessagePrompt(string title, string message, string okText = "Ok", Action okCallback = null, string cancelText = null, Action cancelCallback = null)
        {
            var builder = new AlertDialogBuilder()
                                .SetTitle(title)
                                .SetMessage(message)
                                .AddButton(okText, () => okCallback?.Invoke());
            
            if(!string.IsNullOrEmpty(cancelText))
            {
                builder.AddCancelButton(cancelText, () => cancelCallback?.Invoke());
            }
                                
            m_alertDialog = builder.Build();
        }

        public void Show()
        {   
            m_alertDialog.Show();
        }
    }
}
