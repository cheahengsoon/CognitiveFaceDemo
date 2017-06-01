using Prism.Unity;
using CognitiveFaceDemo.Views;
using Xamarin.Forms;

namespace CognitiveFaceDemo
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override void OnInitialized()
        {
            InitializeComponent();

            NavigationService.NavigateAsync("NavigationPage/MainPage?title=Hello%20from%20Xamarin.Forms");
        }

        protected override void RegisterTypes()
        {
            Container.RegisterTypeForNavigation<NavigationPage>();
            Container.RegisterTypeForNavigation<MainPage>();
            Container.RegisterTypeForNavigation<AddPersonPage>();
            Container.RegisterTypeForNavigation<DemoFaceIdentify>();

            // C
            Plugin.Media.CrossMedia.Current.Initialize();
            Container.RegisterTypeForNavigation<AddPersonFacePage>();
        }
    }
}
