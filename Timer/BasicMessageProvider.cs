using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Timer
{
    class BasicMessageProvider : MessageProvider
    {
        public void showError(string shortError, string longError)
        {
            MessageBox.Show(longError, shortError, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void showError(string shortError, string longError, Window host)
        {
            MessageBox.Show(longError, shortError, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void showMessage(string shortMessage, string longMessage)
        {
            MessageBox.Show(longMessage, shortMessage, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void showMessage(string shortMessage, string longMessage, Window host)
        {
            MessageBox.Show(longMessage, shortMessage, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
