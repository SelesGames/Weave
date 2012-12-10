using SelesGames;
using System;
using System.Windows.Controls;

namespace weave.Pages.WebBrowser
{
    public partial class ArticleSpeechControls : UserControl, IDisposable, IPopup<object>
    {
        public event EventHandler StopButtonPressed;

        public ArticleSpeechControls()
        {
            InitializeComponent();
        }

        void OnStopButtonTapped(object sender, System.Windows.Input.GestureEventArgs e)
        {
            StopButtonPressed.Raise(this);
        }

        public void Dispose()
        {
            busyIndicator.IsPlaying = false;
        }

        public void BeginShow()
        {
            ShowCompleted.Raise(this);
        }

        public void BeginHide()
        {
            HideCompleted.Raise(this);
        }

        public event EventHandler ShowCompleted;
        public event EventHandler HideCompleted;
        public event EventHandler<EventArgs<PopupResult<object>>> ResultCompleted;
    }
}
