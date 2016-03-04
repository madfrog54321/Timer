using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timer
{
    public class DialogButton
    {
        public enum Alignment { Left, Center, Right }
        public enum Style { Flat, Normal }
        public enum ReturnEvent { DoNothing, Close, Empty }

        public delegate ReturnEvent DialogButtonClick();

        private string _text;
        private Alignment _alignment;
        private Style _Style;
        private DialogButtonClick _onClick;

        public ReturnEvent triggerOnClick()
        {
            DialogButtonClick handler = _onClick;
            if (handler != null)
            {
                return handler();
            }
            return ReturnEvent.Empty;
        }

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public Alignment Position
        {
            get { return _alignment; }
            set { _alignment = value; }
        }

        public Style Look
        {
            get { return _Style; }
            set { _Style = value; }
        }

        public DialogButtonClick OnClick
        {
            get { return _onClick; }
            set { _onClick = value; }
        }

        public DialogButton(string text, Alignment alignment, Style style, DialogButtonClick onClick)
        {
            _text = text;
            _alignment = alignment;
            _Style = style;
            _onClick = onClick;
        }
    }
}
