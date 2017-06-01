using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveFaceDemo.ViewModels
{
    public class AddPersonPageViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// PersonListを取得または設定します
        /// </summary>
        private ObservableCollection<Person> _persons;
        public ObservableCollection<Person> Persons
        {
            get { return _persons; }
            set { SetProperty(ref _persons, value); }
        }

        /// <summary>
        /// 選択中のPersonを取得または設定します
        /// </summary>
        private Person _selectedPerson;
        public Person SelectedPerson
        {
            get { return _selectedPerson; }
            set { SetProperty(ref _selectedPerson, value); }
        }

        /// <summary>
        /// NavigationService
        /// </summary>
        private readonly INavigationService _navigationService;
        /// <summary>
        /// PageDialogService
        /// </summary>
        private readonly IPageDialogService _pageDialogService;

        /// <summary>
        /// 追加するPersonの名前を取得または設定します
        /// </summary>
        private string _addPersonName;
        public string AddPersonName
        {
            get { return _addPersonName; }
            set { SetProperty(ref _addPersonName, value); }
        }

        /// <summary>
        /// AddPersonCommand
        /// </summary>
        public DelegateCommand<string> AddPersonCommand { get; }
        /// <summary>
        /// DelPersonCommand
        /// </summary>
        public DelegateCommand DelPersonCommand { get; }
        /// <summary>
        /// NavigateCommand
        /// </summary>
        public DelegateCommand NavigateCommand { get; private set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="navigationService"></param>
        public AddPersonPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
        {
            _navigationService = navigationService;
            _pageDialogService = pageDialogService;
            AddPersonCommand = new DelegateCommand<string>(async (name) => await AddPersonAsync(name));
            DelPersonCommand = new DelegateCommand(async () => await DelPersonAsync());
            NavigateCommand = new DelegateCommand(NavigateNext);
        }

        /// <summary>
        /// PersonGroupを作成します
        /// </summary>
        /// <returns></returns>
        private async Task CreatePersonGroup()
        {
            try
            {
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // PersonGroupを取得
                var persons = await faceServiceClient.GetPersonGroupAsync(Const.GROUP_ID);

                // PersonGroupを作成
                await faceServiceClient.CreatePersonGroupAsync(Const.GROUP_ID, Const.GROUP_ID);
            }
            catch
            {
            }

        }

        /// <summary>
        /// PersonListを取得します
        /// </summary>
        /// <returns></returns>
        private async Task<List<Person>> GetPersonList()
        {
            try
            {
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // PersonListを取得
                var persons = await faceServiceClient.GetPersonsAsync(Const.GROUP_ID);

                if (!persons.Any()) return new List<Person>();
                return persons.ToList();
            }
            catch
            {
                return new List<Person>();
            }
        }


        /// <summary>
        /// Personを登録します
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task AddPersonAsync(string name)
        {
            // 空の場合は何もしない
            if (string.IsNullOrEmpty(name)) return;
            // 名前が重複している場合は何もしない
            if (Persons.Any(person => person.Name == name)) return;

            try
            {
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // Personを追加
                var result = await faceServiceClient.CreatePersonAsync(Const.GROUP_ID, name);
                if (result != null)
                {
                    // 要素を追加
                    Persons.Add(new Person { Name = name, PersonId = result.PersonId });
                }

                // 入力欄をクリア
                AddPersonName = "";

                // 結果表示
                await _pageDialogService.DisplayAlertAsync("処理終了", "名前を追加しました。", "OK");
            }
            catch
            {
                // エラーは無視
                ;
            }
        }

        /// <summary>
        /// Personを削除します
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task DelPersonAsync()
        {
            // 未選択の場合は何もしない
            if (SelectedPerson == null) return;

            try
            {
                if(await _pageDialogService.DisplayAlertAsync("削除確認", "顔情報を削除して良いですか？", "OK", "Cancel"))
                {
                    // サービスクライアント生成
                    var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                    // Personを削除
                    await faceServiceClient.DeletePersonAsync(Const.GROUP_ID, SelectedPerson.PersonId);
                    // 要素を削除
                    Persons.Remove(SelectedPerson);
                    // 結果表示
                    await _pageDialogService.DisplayAlertAsync("処理終了", "顔情報を削除しました。", "OK");
                }
            }
            catch
            {
                // エラーは無視
                ;
            }
        }
        /// <summary>
        /// 次のページへ遷移
        /// </summary>
        /// <param name="uri"></param>
        private void NavigateNext()
        {
            if (SelectedPerson == null) return;

            var navigationParameter = new NavigationParameters
            {
                { Const.PARAM_PERSON, SelectedPerson }
            };
            _navigationService.NavigateAsync("AddPersonFacePage", navigationParameter);
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public async void OnNavigatedTo(NavigationParameters parameters)
        {
            // PersonGroup作成
            //await CreatePersonGroup();

            // List初期設定
            List<Person> personList = await GetPersonList();
            Persons = new ObservableCollection<Person>(personList);
        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }
    }
}
