using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveFaceDemo.ViewModels
{
    /// <summary>
    /// MainPage用ViewModel
    /// </summary>
    public class MainPageViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// NavigationService
        /// </summary>
        private readonly INavigationService _navigationService;

        public DelegateCommand<string> NavigateCommand { get; private set; }
        /// <summary>
        /// 次のページへ遷移
        /// </summary>
        /// <param name="uri"></param>
        private void NavigateNext(string uri)
        {
            _navigationService.NavigateAsync(uri);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="navigationService"></param>
        public MainPageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            NavigateCommand = new DelegateCommand<string>(NavigateNext);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {

        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {

        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {

        }
    }
}
