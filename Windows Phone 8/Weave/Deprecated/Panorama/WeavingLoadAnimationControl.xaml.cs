using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media;

namespace weave
{
    public partial class WeavingLoadAnimationControl : UserControl
    {
        public WeavingLoadAnimationControl()
        {
            InitializeComponent();
            bgImage.Source = ((ImageBrush)weave.Services.PanoramicBackgroundManagerService.Current.BackgroundBrush).ImageSource;
        }

        public IObservable<Unit> PlayLongAnimation()
        {
            statusMessage.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Collapsed;

            var observer = new AsyncSubject<Unit>();

            WeavingTextSB.Begin();
            CountdownSB.Begin();
            NumberShakeSB.Begin();
            CountdownTimer.In(TimeSpan.FromSeconds(8.9)).Do(() =>
            {
                NumberShakeSB.Stop();
                WeavingTextSB.Stop();
            });
            CountdownTimer
                .In(TimeSpan.FromSeconds(9))
                .Do(() => WeaveBlowUpSB.Begin());

            CountdownTimer
                .In(TimeSpan.FromSeconds(9.2))
                .Do(() => FadeOutSB.BeginWithNotification().Subscribe(notUsed =>
                {
                    this.Visibility = Visibility.Collapsed;
                    observer.OnNext(new Unit());
                    observer.OnCompleted();
                }));

            return observer.AsObservable();
        }

        public void PlayLoadingAnimation()
        {
            grid.Visibility = Visibility.Collapsed;
            textBlock4.Visibility = Visibility.Collapsed;
            textBlock3.Visibility = Visibility.Collapsed;
        }

        public IObservable<Unit> Close()
        {
            return Observable.Create<Unit>(observer =>
            {
                return FadeOutSB.BeginWithNotification().Subscribe(
                    _ =>
                    {
                        this.Visibility = Visibility.Collapsed;
                        observer.OnNext(Unit.Default);
                        observer.OnCompleted();
                    });
            });
        }
    }
}