using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Face;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CognitiveFaceDemo.ViewModels
{
    /// <summary>
    /// AddPersonFace用ViewModel
    /// </summary>
    public class AddPersonFacePageViewModel : BindableBase, INavigationAware
    {
        /// <summary>
        /// タイトルを取得または設定します
        /// </summary>
        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private Person _person;
        /// <summary>
        /// 画像ソースを取得または設定します
        /// </summary>
        private ImageSource _imageSource;
        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
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
        /// TakePhotoCommand
        /// </summary>
        public DelegateCommand TakePhotoCommand { get; }
        /// <summary>
        /// TrainPersonGroupCommand
        /// </summary>
        public DelegateCommand TrainPersonGroupCommand { get; }

        /// <summary>
        /// 実行中かどうかを取得また設定します
        /// </summary>
        private bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
            set { SetProperty(ref isRunning, value); }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="navigationService"></param>
        /// <param name=""></param>
        /// <param name="pageDialogService"></param>
        public AddPersonFacePageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
        {
            _navigationService = navigationService;
            _pageDialogService = pageDialogService;
            TakePhotoCommand = new DelegateCommand(async () => await TakePhotoAsync());
            TrainPersonGroupCommand = new DelegateCommand(async () => await TrainPersonGroupAsync());
        }

        /// <summary>
        /// 写真撮影
        /// </summary>
        /// <returns></returns>
        private async Task TakePhotoAsync()
        {
            // Cameraを起動して写真を撮影（Plugin任せ）
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                DefaultCamera = CameraDevice.Front,
                AllowCropping = false,
                PhotoSize = PhotoSize.Medium,
            });

            // 撮影しなかった場合は何もしない
            if (file == null) return;

            // 画像を取得
            ImageSource = ImageSource.FromStream(() => file.GetStream());

            try
            {
                // ぐるぐる開始
                IsRunning = true;
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // PersonFaceを追加
                var result = await faceServiceClient.AddPersonFaceAsync(Const.GROUP_ID, _person.PersonId, file.GetStream());
                // 学習を開始
                await faceServiceClient.TrainPersonGroupAsync(Const.GROUP_ID);
            }
            catch
            {
                // エラーは取りあえず捨てる
                ;
            }
            finally
            {
                // ぐるぐる停止
                IsRunning = false;

                // 結果表示
                await _pageDialogService.DisplayAlertAsync("処理終了", "登録＆学習が完了しました。", "OK");
            }
        }

        /// <summary>
        /// 学習
        /// </summary>
        /// <returns></returns>
        private async Task TrainPersonGroupAsync()
        {
            try
            {
                // ぐるぐる開始
                IsRunning = true;
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // 学習を開始
                await faceServiceClient.TrainPersonGroupAsync(Const.GROUP_ID);
            }
            catch
            {
                // エラーは取りあえず捨てる
                ;
            }
            finally
            {
                // ぐるぐる停止
                IsRunning = false;

                // 結果表示
                await _pageDialogService.DisplayAlertAsync("処理終了", "学習が完了しました。", "OK");
            }
        }

        public void OnNavigatedFrom(NavigationParameters parameters)
        {
        }

        public void OnNavigatedTo(NavigationParameters parameters)
        {
            if (parameters.ContainsKey(Const.PARAM_PERSON))
            {
                var person = parameters[Const.PARAM_PERSON] as Person;
                if (person == null)
                {

                }
                else
                {
                    _person = person;
                    Title = person.Name;
                }
            }

        }

        public void OnNavigatingTo(NavigationParameters parameters)
        {
        }
    }
}
