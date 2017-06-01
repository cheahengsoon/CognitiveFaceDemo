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
    public class DemoFaceIdentifyViewModel : BindableBase
    {
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
        /// 名前を取得または設定します
        /// </summary>
        private string _personName;
        public string PersonName
        {
            get { return _personName; }
            set { SetProperty(ref _personName, value); }
        }

        /// <summary>
        /// 確度を取得または設定します
        /// </summary>
        private string _confidence;
        public string Confidence
        {
            get { return _confidence; }
            set { SetProperty(ref _confidence, value); }
        }

        /// <summary>
        /// おまけ情報を取得または設定します
        /// </summary>
        private string _omake;
        public string Omake
        {
            get { return _omake; }
            set { SetProperty(ref _omake, value); }
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
        /// <param name="pageDialogService"></param>
        public DemoFaceIdentifyViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
        {
            _navigationService = navigationService;
            _pageDialogService = pageDialogService;
            TakePhotoCommand = new DelegateCommand(async () => await TakePhotoAsync());
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
                // 結果表示用
                PersonName = "未登録";
                Confidence = "???";
                Omake = "";
                // サービスクライアント生成
                var faceServiceClient = new FaceServiceClient(Const.SUBSCRIPTION_KEY);
                // 画像から顔検出(Detect)
                var faces = await faceServiceClient.DetectAsync(file.GetStream(), returnFaceId: true, returnFaceAttributes: new[]
                {
                    //FaceAttributeType.Gender,
                    FaceAttributeType.Age,
                    FaceAttributeType.Smile,
                });
                // 検出されなければ何もしない
                if (!faces.Any()) return;

                // おまけ(性別、年齢、笑顔度)
                Omake = string.Format("(Age[{0}] Smile[{1}])", faces.First().FaceAttributes.Age, faces.First().FaceAttributes.Smile * 100);

                // 顔の認証(Identify)
                var results = await faceServiceClient.IdentifyAsync(Const.GROUP_ID, faces.Select(face => face.FaceId).ToArray());
                foreach (var result in results)
                {
                    if (result.Candidates.Length == 0)
                    {
                        // 認証できなかったら何もしない
                    }
                    else
                    {
                        // Personから情報を取得
                        var person = await faceServiceClient.GetPersonAsync(Const.GROUP_ID, result.Candidates.First().PersonId);
                        // 名前取得
                        PersonName = person?.Name;
                        // 認証の精度
                        Confidence = (result.Candidates.First().Confidence * 100).ToString();
                    }
                }

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
                await _pageDialogService.DisplayAlertAsync("処理終了", "認識処理が終了しました。", "OK");
            }
        }
    }
}
