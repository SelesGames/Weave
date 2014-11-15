using Common.Microsoft.Services.OneNote.Response;
using System;
using System.Threading.Tasks;
using System.Windows;
using Weave.Microsoft.OneNote;
using Weave.Customizability.SavedState;
using Weave.ViewModels;
using Weave.ViewModels.Mobilizer;

namespace Weave.Services.OneNote
{
    class OneNoteSavingWorkflow
    {
        readonly PermanentState permState;
        readonly MobilizedArticle article;
        readonly NewsItem newsItem;
        readonly bool isArticleMobilizedSuccessfully;

        public OneNoteSavingWorkflow(PermanentState permState, MobilizedArticle article, NewsItem newsItem, bool isArticleMobilizedSuccessfully)
        {
            if (permState == null) throw new ArgumentNullException("permState");

            this.permState = permState;
            this.article = article;
            this.newsItem = newsItem;
            this.isArticleMobilizedSuccessfully = isArticleMobilizedSuccessfully;
        }

        internal async Task Save()
        {
            await TrySave(continueOnSaveFailure: true);
        }

        async Task TrySave(bool continueOnSaveFailure)
        {
            try
            {
                if (continueOnSaveFailure)
                {
                    await TrySaveRegisterOnFail();
                }
                else
                {
                    await InnerSave();
                }
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
                MessageBox.Show("Unable to save to OneNote");
            }
        }

        async Task TrySaveRegisterOnFail()
        {
            try
            {
                await InnerSave();
                return;
            }
            catch (Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            await Register();
        }

        async Task InnerSave()
        {
            var accessToken = permState.ThirdParty.LiveAccessToken;

            if (accessToken == null)
            {
                throw new Exception("accessToken not set");
            }

            Func<Task<BaseResponse>> saveTask;

            if (isArticleMobilizedSuccessfully && article != null)
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

            else if (!isArticleMobilizedSuccessfully && newsItem != null)
            {
                var oneNoteSave = new HtmlLinkOneNoteItem
                {
                    Title = newsItem.Title,
                    Link = newsItem.Link,
                    Source = newsItem.FormattedForMainPageSourceAndDate,
                };
                saveTask = () => oneNoteSave.SendToOneNote(accessToken);
            }

            else 
                return;

            ToastService.ToastPrompt("Sending to OneNote...");

            try
            { 
                var response = await saveTask();
                if (response is CreateSuccessResponse)
                {
                    ToastService.ToastPrompt("Saved to OneNote!");
                    return;
                }
            }
            catch(Exception ex)
            {
                DebugEx.WriteLine(ex);
            }

            ToastService.ToastPrompt("ERROR saving to OneNote");
        }

        Task Register()
        {
            var tcs = new TaskCompletionSource<int>();
            GlobalDispatcher.Current.BeginInvoke(() =>
            {
                GlobalNavigationService.ToOneNoteSignInPage(async () => await OnCallback());
                tcs.TrySetResult(0);
            });
            return tcs.Task;
        }

        async Task OnCallback()
        {
            await TrySave(continueOnSaveFailure: false);
        }
    }
}
