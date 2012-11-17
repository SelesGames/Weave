using System.Threading.Tasks;
using Microsoft.Phone.Controls;
using SelesGames;

namespace weave
{
    public partial class DummyPage : PhoneApplicationPage
    {
        public DummyPage()
        {
            InitializeComponent();
        }

        public async Task LayoutPopups()
        {
            SelesGames.PopupService.ForceLayout();

            var tempAccentShare = ServiceResolver.Get<SocialShareContextMenuControl>("accent");
            var tempFontSize = ServiceResolver.Get<FontSizePopup>();
            LayoutRoot.Children.Insert(0, tempAccentShare);
            LayoutRoot.Children.Insert(1, tempFontSize);
            await Task.Yield();
            LayoutRoot.Children.Remove(tempFontSize);
            LayoutRoot.Children.Remove(tempAccentShare);
        }
    }
}