using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Timer
{
    class DialogMessageProvider : MessageProvider
    {
        public delegate void resetProvider();

        private Panel _dialogHost;
        private resetProvider _onReset;
        private void triggerOnReset()
        {
            resetProvider handler = _onReset;
            if (handler != null)
            {
                handler();
            }
        }

        public DialogMessageProvider(Panel host, resetProvider onReset)
        {
            _dialogHost = host;
            _onReset = onReset;
        }

        public void showError(string shortError, string longError)
        {
            if (false)
            {
                triggerOnReset();
            }
            else
            {
                SystemSounds.Hand.Play();
                DialogBox.showInfoBox(_dialogHost, longError, shortError, "Continue", null);
            }
        }

        public void showError(string shortError, string longError, Window host)
        {
            if (false)
            {
                triggerOnReset();
            }
            else
            {
                SystemSounds.Hand.Play();
                DialogBox.showInfoBox(_dialogHost, longError, shortError, "Continue", null);
            }
        }

        public void showMessage(string shortMessage, string longMessage)
        {
            if (false)
            {
                triggerOnReset();
            }
            else
            {
                SystemSounds.Exclamation.Play();
                DialogBox.showInfoBox(_dialogHost, longMessage, shortMessage, "Got It", null);
            }
        }

        public void showMessage(string shortMessage, string longMessage, Window host)
        {
            if (false)
            {
                triggerOnReset();
            }
            else
            {
                SystemSounds.Exclamation.Play();
                DialogBox.showInfoBox(_dialogHost, longMessage, shortMessage, "Got It", null);
            }
        }
    }
}
