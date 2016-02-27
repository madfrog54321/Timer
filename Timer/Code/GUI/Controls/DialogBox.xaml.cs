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

        private Panel _parent;

        public static void showOptionBox(Panel parent, string message, string title, string secondaryOption, string mainOption, dialogResultHandler handler)
        {
            DialogBox dialog = new DialogBox();
            dialog.onGotDialogResult += handler;

            dialog.tbMessage.Text = message;
            dialog.tbTitle.Text = title;
            dialog.btnOption2.Content = secondaryOption;
            dialog.btnOption1.Content = mainOption;

            dialog._parent = parent;
            dialog._parent.Children.Add(dialog);
        }

        private void btnOption2_Click(object sender, RoutedEventArgs e)
        {
            _parent.Children.Remove(this);
            triggerGotDialogResult(DialogResult.SecondaryOption);
        }

        private void btnOption1_Click(object sender, RoutedEventArgs e)
        {
            _parent.Children.Remove(this);
            triggerGotDialogResult(DialogResult.MainOption);
        }
    }
}
