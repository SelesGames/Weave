using Common.Microsoft.OneNote.Response;
using SelesGames;
using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.Microsoft.OneNote;
using Weave.SavedState;
using Weave.ViewModels;
using Weave.ViewModels.Mobilizer;

namespace Weave.Services.OneNote
{
    public class OneNoteHelper
    {
        const string CALLBACK_URI = "http://www.selesgames.com";

        PermanentState permState;
        MobilizedArticle article;
        NewsItem newsItem;
        bool isArticleMobilizedSuccessfully;

        readonly static OneNoteHelper current = new OneNoteHelper();
        public static OneNoteHelper Current { get { return current; } }

        public async Task Save(MobilizedArticle article, NewsItem newsItem, bool isArticleMobilizedSuccessfully)
        {
            if (article == null || newsItem == null)
                return;

            this.article = article;
            this.newsItem = newsItem;
            this.isArticleMobilizedSuccessfully = isArticleMobilizedSuccessfully;

            await TrySave(continueOnSaveFailure: true);
        }


        async Task TrySave(bool continueOnSaveFailure)
        {
            try
            {
                Task saveTask;

                if (continueOnSaveFailure)
                {
                    saveTask = InnerSave()
                        .ContinueWith(async _ => await Register(), TaskContinuationOptions.OnlyOnFaulted);
                }
                else
                {
                    saveTask = InnerSave();
                }

                await saveTask;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Unable to save to OneNote");
            }
        }

        async Task InnerSave()
        {
            if (article == null || newsItem == null)
                return;

            permState = ServiceResolver.Get<PermanentState>();
            var accessToken = permState.ThirdParty.LiveAccessToken;

            if (accessToken == null)
            {
                throw new Exception("accessToken not set");
            }

            Func<Task<BaseResponse>> saveTask;

            if (isArticleMobilizedSuccessfully)
            {
                var oneNoteSave = new MobilizedOneNoteItem
                {
                    Title = article.Title,
                    Link = article.Link,
                    Source = article.CombinedPublicationAndDate,
                    HeroImage = article.HeroImageUrl,
                    BodyHtml = article.ContentHtml,
                };
                saveTask = () => oneNoteSave.SendToOneNote(accessToken);
            }

            else
            {
                var oneNoteSave = new HtmlLinkOneNoteItem
                {
                    Title = newsItem.Title,
                    Link = newsItem.Link,
                    Source = newsItem.FormattedForMainPageSourceAndDate,
                };
                saveTask = () => oneNoteSave.SendToOneNote(accessToken);
            }

            await GlobalDispatcher.Current.InvokeAsync(() => ToastService.ToastPrompt("Sending to OneNote..."));

            var response = await saveTask();
            if (response is CreateSuccessResponse)
            {
                await GlobalDispatcher.Current.InvokeAsync(() => ToastService.ToastPrompt("Saved to OneNote!"));
            }
            else
            {
                await GlobalDispatcher.Current.InvokeAsync(() => ToastService.ToastPrompt("ERROR saving to OneNote"));
            }
        }

        async Task Register()
        {
            await GlobalDispatcher.Current.InvokeAsync(() => GlobalNavigationService.ToOneNoteSignInPage());
        }

        public async Task OnCallback()
        {
            await TrySave(continueOnSaveFailure: false);
        }
    }
}
