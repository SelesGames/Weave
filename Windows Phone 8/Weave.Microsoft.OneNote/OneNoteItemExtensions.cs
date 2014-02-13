using Common.Microsoft.OneNote;
using Common.Microsoft.OneNote.Response;
using System.Threading.Tasks;

namespace Weave.Microsoft.OneNote
{
    public static class OneNoteItemExtensions
    {
        public static Task<BaseResponse> SendToOneNote(this MobilizedOneNoteItem oneNoteItem)
        {
            return CreateClient().CreateSimple(oneNoteItem.Html);
        }
    
        public static Task<BaseResponse> SendToOneNote(this HtmlLinkOneNoteItem oneNoteItem)
        {
            var html = new OneNoteHtmlFormatter().CreateHtml(oneNoteItem);
            return CreateClient().CreateSimple(html);
        }




        #region private helper methods

        static OneNoteServiceClient CreateClient()
        {
            return new OneNoteServiceClient(Settings.AccessToken);
        }

        #endregion
    }
}
