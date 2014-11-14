using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;

namespace weave
{
    public class WeavePage : PhoneApplicationPage
    {
        Subject<WeaveNavigationEventArgs> navigatedTo = new Subject<WeaveNavigationEventArgs>();
        public IObservable<WeaveNavigationEventArgs> NavigatedTo { get { return navigatedTo.AsObservable(); } }

        AsyncSubject<WeaveNavigationEventArgs> pageLoaded = new AsyncSubject<WeaveNavigationEventArgs>();
        public IObservable<WeaveNavigationEventArgs> PageLoaded { get { return pageLoaded.AsObservable(); } }

        public WeavePage()
        {
            this.OnPageLoad().ObserveOnDispatcher().Take(1).SafelySubscribe(e =>
            {
                OnPageLoad(e);
                pageLoaded.OnNext(e);
                pageLoaded.OnCompleted();
            });
        }

        protected virtual void OnPageLoad(WeaveNavigationEventArgs navigationEventArgs)
        {

        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var navArgs = new WeaveNavigationEventArgs
            {
                Content = e.Content,
                Uri = e.Uri,
                QueryString = this.NavigationContext.QueryString,
            };
            navigatedTo.OnNext(navArgs);
        }
    }

    public class WeaveNavigationEventArgs
    {
        public object Content { get; set; }
        public Uri Uri { get; set; }
        public IDictionary<string, string> QueryString { get; set; }
    }

    internal static class WeavePageExtensionsMethods
    {
        internal static IObservable<WeaveNavigationEventArgs> OnPageLoad(this WeavePage page)
        {
            var both = page.NavigatedTo.And(page.GetLoaded().Take(1)).Then((navArgs, _) => navArgs);
            var join = Observable.When(both);

            return Observable.Create<WeaveNavigationEventArgs>(observer =>
            {
                var disp = Disposable.Empty;
                try
                {
                    disp = join.SafelySubscribe(
                        navArgs =>
                        {
                            try
                            {
                                //Observable.Start(() =>
                                DispatcherScheduler.Current.Schedule(() =>
                                {
                                    try
                                    {
                                        observer.OnNext(navArgs);
                                    }
                                    catch (Exception ex)
                                    {
                                        observer.OnError(ex);
                                    }
                                });
                            }
                            catch (Exception ex)
                            {
                                observer.OnError(ex);
                            }
                        },
                        observer.OnError, observer.OnCompleted);
                }
                catch (Exception ex)
                {
                    observer.OnError(ex);
                }
                return disp;
            });
        }
    }
}
