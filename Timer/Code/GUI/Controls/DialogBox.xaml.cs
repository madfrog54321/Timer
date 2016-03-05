using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Timer
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : UserControl
    {
        private DialogBox()
        {
            InitializeComponent();
        }

        public enum DialogResult
        {
            SecondaryOption, MainOption
        }

        public delegate void dialogResultHandler(DialogResult result);
        public event dialogResultHandler onGotDialogResult;
        private void triggerGotDialogResult(DialogResult result)
        {
            dialogResultHandler handler = onGotDialogResult;
            if (handler != null)
            {
                handler(result);
            }
        }

        public static void showOptionBox(Panel parent, string message, string title, string secondaryOption, string mainOption, dialogResultHandler handler)
        {
            DialogBox dialog = new DialogBox();
            dialog.onGotDialogResult += handler;

            dialog.tbMessage.Text = message;
            dialog.tbTitle.Text = title;

            new Dialog(parent, dialog, false, false, false, null,
                new DialogButton(secondaryOption, DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {

                    dialog.triggerGotDialogResult(DialogResult.SecondaryOption);

                    return DialogButton.ReturnEvent.Close;
                }), new DialogButton(mainOption, DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {

                    dialog.triggerGotDialogResult(DialogResult.MainOption);

                    return DialogButton.ReturnEvent.Close;
                }));
        }

        public static void showInfoBox(Panel parent, string message, string title, string button, dialogResultHandler handler)
        {
            DialogBox dialog = new DialogBox();
            dialog.onGotDialogResult += handler;

            dialog.tbMessage.Text = message;
            dialog.tbTitle.Text = title;

            new Dialog(parent, dialog, false, false, false, null,
                new DialogButton(button, DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {

                    dialog.triggerGotDialogResult(DialogResult.MainOption);

                    return DialogButton.ReturnEvent.Close;
                }));
        }
    }
}
