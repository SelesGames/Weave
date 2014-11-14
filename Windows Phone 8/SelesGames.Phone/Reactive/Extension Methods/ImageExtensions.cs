using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;

namespace SelesGames.Phone
{
    public static class ImageExtensions
    {
        public static IObservable<EventPattern<RoutedEventArgs>> GetImageOpened(this Image image)
        {
            return Observable.FromEventPattern<RoutedEventArgs>(image, "ImageOpened");
        }

        public static IObservable<EventPattern<ExceptionRoutedEventArgs>> GetImageFailed(this Image image)
        {
            return Observable.FromEventPattern<ExceptionRoutedEventArgs>(image, "ImageFailed");
        }
    }
}