﻿using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using SelesGames;
using System.Threading.Tasks;

namespace weave
{
    public partial class MainPageSettingsPopup : UserControl, IDisposable
    {
        CompositeDisposable disposables = new CompositeDisposable();

        BindableMainPageFontStyle bindingSource;
        PermanentState permState;

        public MainPageSettingsPopup()
        {
            InitializeComponent();

            if (this.IsInDesignMode())
                return;

            permState = AppSettings.Instance.PermanentState.Get().WaitOnResult();

            this.bindingSource = ServiceResolver.Get<BindableMainPageFontStyle>();

            InitializeFontSizePicker();
            InitializeFontThicknessPicker();

            this.title.SetBinding(TextBlock.FontSizeProperty, this.bindingSource.TitleSizeBinding);
            this.title.SetBinding(TextBlock.FontFamilyProperty, this.bindingSource.ThicknessBinding);

            this.description.SetBinding(TextBlock.FontSizeProperty, this.bindingSource.DescriptionSizeBinding);
            this.description.SetBinding(TextBlock.FontFamilyProperty, this.bindingSource.ThicknessBinding);
            this.description.SetBinding(TextBlock.LineHeightProperty, this.bindingSource.LineHeightBinding);

            this.publishedDate.SetBinding(TextBlock.FontSizeProperty, this.bindingSource.PublicationLineSizeBinding);
            this.publishedDateOverlay.SetBinding(TextBlock.FontSizeProperty, this.bindingSource.PublicationLineSizeBinding);

            this.GetLayoutUpdated().Take(1).Subscribe(() => RotationSB.Begin());
        }



        #region FontSize change handling

        void InitializeFontSizePicker()
        {
            Observable.FromEventPattern<SelectionChangedEventArgs>(this.fontSizePicker, "SelectionChanged").Take(1)
                .Subscribe(() =>
                {
                    var currentFontSize = permState.MainPageArticleListFontSize;
                    switch (currentFontSize)
                    {
                        case weave.FontSize.Small:
                            this.fontSizePicker.SelectedItem = this.fontSizePicker.Items[0];
                            this.fontSizePicker.SelectedIndex = 0;
                            break;

                        case weave.FontSize.Medium:
                            this.fontSizePicker.SelectedItem = this.fontSizePicker.Items[1];
                            this.fontSizePicker.SelectedIndex = 1;
                            break;

                        case weave.FontSize.MediumLarge:
                            this.fontSizePicker.SelectedItem = this.fontSizePicker.Items[2];
                            this.fontSizePicker.SelectedIndex = 2;
                            break;

                        case weave.FontSize.Large:
                            this.fontSizePicker.SelectedItem = this.fontSizePicker.Items[3];
                            this.fontSizePicker.SelectedIndex = 3;
                            break;

                        case weave.FontSize.ExtraLarge:
                            this.fontSizePicker.SelectedItem = this.fontSizePicker.Items[4];
                            this.fontSizePicker.SelectedIndex = 4;
                            break;

                        default:
                            throw new Exception("unexexpected FontSize value");
                    }

                    Observable.FromEventPattern<SelectionChangedEventArgs>(this.fontSizePicker, "SelectionChanged")
                        .Subscribe(OnFontSizeChanged)
                        .DisposeWith(disposables);
                });
        }

        void OnFontSizeChanged(EventPattern<SelectionChangedEventArgs> ea)
        {
            var e = ea.EventArgs;
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            var item = e.AddedItems.Cast<object>().OfType<ListPickerItem>().FirstOrDefault();
            var content = item.Content.ToString();
            DebugEx.WriteLine(content);

            weave.FontSize fontSize;

            switch (content)
            {
                case "small":
                    fontSize = weave.FontSize.Small;
                    break;
                case "medium":
                    fontSize = weave.FontSize.Medium;
                    break;
                case "medium large (default)":
                    fontSize = weave.FontSize.MediumLarge;
                    break;
                case "large":
                    fontSize = weave.FontSize.Large;
                    break;
                case "extra large":
                    fontSize = weave.FontSize.ExtraLarge;
                    break;
                default:
                    return;
            }

            permState.MainPageArticleListFontSize = fontSize;
        }

        #endregion



        #region Font thickness change handling

        void InitializeFontThicknessPicker()
        {
            Observable.FromEventPattern<SelectionChangedEventArgs>(this.fontThicknessPicker, "SelectionChanged").Take(1)
                .Subscribe(() =>
                {
                    var currentFontThickness = permState.MainPageArticleListFontThickness;
                    switch (currentFontThickness)
                    {
                        case weave.FontThickness.VerySkinny:
                            this.fontThicknessPicker.SelectedItem = this.fontThicknessPicker.Items[0];
                            this.fontThicknessPicker.SelectedIndex = 0;
                            break;

                        case weave.FontThickness.Skinny:
                            this.fontThicknessPicker.SelectedItem = this.fontThicknessPicker.Items[1];
                            this.fontThicknessPicker.SelectedIndex = 1;
                            break;

                        case weave.FontThickness.Regular:
                            this.fontThicknessPicker.SelectedItem = this.fontThicknessPicker.Items[2];
                            this.fontThicknessPicker.SelectedIndex = 2;
                            break;

                        case weave.FontThickness.Fat:
                            this.fontThicknessPicker.SelectedItem = this.fontThicknessPicker.Items[3];
                            this.fontThicknessPicker.SelectedIndex = 3;
                            break;

                        default:
                            throw new Exception("unexexpected FontThickness value");
                    }

                    Observable.FromEventPattern<SelectionChangedEventArgs>(this.fontThicknessPicker, "SelectionChanged")
                        .Subscribe(OnFontThicknessChanged)
                        .DisposeWith(disposables);
                });
        }

        void OnFontThicknessChanged(EventPattern<SelectionChangedEventArgs> ea)
        {
            var e = ea.EventArgs;
            if (e.AddedItems == null || e.AddedItems.Count == 0)
                return;

            var item = e.AddedItems.Cast<object>().OfType<ListPickerItem>().FirstOrDefault();
            var content = item.Content.ToString();
            DebugEx.WriteLine(content);

            weave.FontThickness thickness;

            switch (content)
            {
                case "skinny":
                    thickness = weave.FontThickness.Skinny;
                    break;
                case "very skinny":
                    thickness = weave.FontThickness.VerySkinny;
                    break;
                case "regular (default)":
                    thickness = weave.FontThickness.Regular;
                    break;
                case "fat":
                    thickness = weave.FontThickness.Fat;
                    break;
                default:
                    return;
            }

            permState.MainPageArticleListFontThickness = thickness;
        }

        #endregion



        public IObservable<Unit> Show()
        {
            return Observable.Return(new Unit());
        }

        public IObservable<Unit> Hide()
        {
            return Observable.Return(new Unit());
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            observable.OnNext(ToastPromptControl.PopUpResult.Ok);
        }

        Subject<ToastPromptControl.PopUpResult> observable = new Subject<ToastPromptControl.PopUpResult>();
        public IObservable<ToastPromptControl.PopUpResult> ResultCompleted { get { return observable.AsObservable(); } }

        public void Dispose()
        {
            this.disposables.Dispose();
        }
    }
}
