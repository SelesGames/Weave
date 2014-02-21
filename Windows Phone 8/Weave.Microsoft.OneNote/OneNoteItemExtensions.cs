using Common.Microsoft;
using Common.Microsoft.OneNote;
using Common.Microsoft.OneNote.Response;
using System.Threading.Tasks;

namespace Weave.Microsoft.OneNote
{
    public static class OneNoteItemExtensions
    {
        public static Task<BaseResponse> SendToOneNote(this HtmlLinkOneNoteItem oneNoteItem, LiveAccessToken token)
        {
            var html = new Formatter().CreateHtml(oneNoteItem);
            return CreateClient(token).CreateSimple(html);
        }

        public static Task<BaseResponse> SendToOneNote(this MobilizedOneNoteItem oneNoteItem, LiveAccessToken token)
        {
            var html = new Formatter().CreateHtml(oneNoteItem);
            return CreateClient(token).CreateSimple(html);
        }




        #region private helper methods

        static OneNoteServiceClient CreateClient(LiveAccessToken token)
        {
            return new OneNoteServiceClient(token);
        }

        #endregion
    }
}
