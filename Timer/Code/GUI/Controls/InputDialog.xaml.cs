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
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : UserControl
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public delegate void inputResultHandler(string result);
        public event inputResultHandler onGotInputResult;
        private void triggerGotInputResult(string result)
        {
            inputResultHandler handler = onGotInputResult;
            if (handler != null)
            {
                handler(result);
            }
        }

        public static void show(Panel parent, string hint, string acceptButton, inputResultHandler handler)
        {
            InputDialog dialog = new InputDialog();
            dialog.onGotInputResult += handler;

            dialog.tbInput.SetValue(MaterialDesignThemes.Wpf.TextFieldAssist.HintProperty, hint);

            new Dialog(parent, dialog, false, false, false, null,
                new DialogButton("Cancel", DialogButton.Alignment.Right, DialogButton.Style.Flat, delegate () {
                    return DialogButton.ReturnEvent.Close;
                }), new DialogButton(acceptButton, DialogButton.Alignment.Right, DialogButton.Style.Normal, delegate () {
                    
                    dialog.triggerGotInputResult(dialog.tbInput.Text);

                    return DialogButton.ReturnEvent.Close;
                }));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(tbInput);
        }
    }
}
