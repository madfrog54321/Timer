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
        private BasicMessageProvider basic;

        public DialogMessageProvider(Panel host, resetProvider onReset)
        {
            _dialogHost = host;
            _onReset = onReset;
            basic = new BasicMessageProvider();
        }

        public void showError(string shortError, string longError)
        {
            if (false)
            {
                triggerOnReset();
            }
            else
            {
                try
                {
                    SystemSounds.Hand.Play();
                    DialogBox.showInfoBox(_dialogHost, longError, shortError, "Continue", null);
                }
                catch (Exception ex)
                {
                    basic.showError(shortError, longError);
                }
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
                try
                {
                    SystemSounds.Hand.Play();
                    DialogBox.showInfoBox(_dialogHost, longError, shortError, "Continue", null);
                }
                catch (Exception ex)
                {
                    basic.showError(shortError, longError);
                }
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
                try
                {
                    SystemSounds.Exclamation.Play();
                    DialogBox.showInfoBox(_dialogHost, longMessage, shortMessage, "Got It", null);
                }
                catch (Exception ex)
                {
                    basic.showMessage(shortMessage, longMessage);
                }
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
                try
                {
                    SystemSounds.Exclamation.Play();
                    DialogBox.showInfoBox(_dialogHost, longMessage, shortMessage, "Got It", null);
                }
                catch (Exception ex)
                {
                    basic.showMessage(shortMessage, longMessage);
                }
            }
        }
    }
}
