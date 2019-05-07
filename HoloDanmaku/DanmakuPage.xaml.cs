using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DanmakuService.Bilibili;
using DanmakuService.Bilibili.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace HoloDanmaku
{
    internal class DanmakuDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate DanmakuTemplate { get; set; }
        public DataTemplate GiftTemplate { get; set; }
        public DataTemplate GiftTopTemplate { get; set; }
        public DataTemplate GuardBuyTemplate { get; set; }
        public DataTemplate WelcomeGuardTemplate { get; set; }
        public DataTemplate LiveEndTemplate { get; set; }
        public DataTemplate LiveStartTemplate { get; set; }
        public DataTemplate WelcomeTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (!(item is DanmakuModel model)) throw new ArgumentException();

            switch (model.MsgType)
            {
                case MsgTypeEnum.Comment:
                    return DanmakuTemplate;
                case MsgTypeEnum.GiftSend:
                    return GiftTemplate;
                case MsgTypeEnum.GiftTop:
                    return GiftTopTemplate;
                case MsgTypeEnum.Welcome:
                    return WelcomeTemplate;
                case MsgTypeEnum.LiveStart:
                    return LiveStartTemplate;
                case MsgTypeEnum.LiveEnd:
                    return LiveEndTemplate;
                case MsgTypeEnum.Unknown:
                    return DanmakuTemplate;
                case MsgTypeEnum.WelcomeGuard:
                    return WelcomeGuardTemplate;
                case MsgTypeEnum.GuardBuy:
                    return GuardBuyTemplate;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DanmakuPage : Page
    {
        private BilibiliApi _api;

        public DanmakuPage()
        {
            InitializeComponent();
        }

        public ObservableCollection<DanmakuModel> ItemsSource { get; set; } = new ObservableCollection<DanmakuModel>();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var id = e.Parameter is int ? (int) e.Parameter : 0;
            _api = new BilibiliApi(id);
            _api.DanmakuReceived += ApiOnDanmakuReceived;
            _api.ViewerCountChanged += ApiOnViewerCountChanged;
            _api.Start();
        }

        private void ApiOnViewerCountChanged(object sender, uint e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => CountTextBlock.Text = e.ToString());
        }

        private void ApiOnDanmakuReceived(object sender, DanmakuModel e)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                ItemsSource.Add(e));
        }
    }
}