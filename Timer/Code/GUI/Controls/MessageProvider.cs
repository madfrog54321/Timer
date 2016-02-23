using System.Windows;

namespace Timer
{
    interface MessageProvider
    {

        void showMessage(string shortMessage, string longMessage);
        void showMessage(string shortMessage, string longMessage, Window host);

        void showError(string shortError, string longError);
        void showError(string shortError, string longError, Window host);

    }
}
