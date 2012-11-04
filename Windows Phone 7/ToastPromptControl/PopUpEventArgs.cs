using System;

namespace ToastPromptControl
{
    public class PopUpEventArgs<T, TPopUpResult> : EventArgs
    {
        public TPopUpResult PopUpResult { get; set; }
        public Exception Error { get; set; }
        public T Result { get; set; }
    }
}
