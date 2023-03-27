using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;


namespace diary_uwp
{
    public sealed partial class MainPage : Page
    {


        public MainPage()
        {
            this.InitializeComponent();
            InitializeInkCanvas();

            datePicker.Date = DateTime.Now.Date;
            datePicker.DateChanged += DatePicker_DateChanged;

            // 앱 종료 이벤트 등록
            Application.Current.Suspending += Current_Suspending;
        }

        
            private async void Current_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // 현재 선택된 날짜에 대해 일기 저장
            await SaveDiaryAsync(datePicker.Date.Date);

            deferral.Complete();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await LoadDiaryAsync(datePicker.Date.Date);
        }

        private void InitializeInkCanvas()
        {
            inkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Mouse;
            InkDrawingAttributes drawingAttributes = new InkDrawingAttributes();
            inkCanvas.InkPresenter.UpdateDefaultDrawingAttributes(drawingAttributes);
            inkToolbar.TargetInkCanvas = inkCanvas;
            inkCanvas.VerticalAlignment = VerticalAlignment.Stretch;
            scrollViewer.Content = inkCanvas;
        }




        private async void DatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            DateTime previousDate = e.OldDate.Date;
            await SaveDiaryAsync(previousDate);

            inkCanvas.InkPresenter.StrokeContainer.Clear();

            DateTime selectedDate = e.NewDate.Date;
            await LoadDiaryAsync(selectedDate);
        }

        public async Task SaveDiaryAsync(DateTime date)
        {
            string fileName = $"{date.ToString("yyyy-MM-dd")}.gif";
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                await inkCanvas.InkPresenter.StrokeContainer.SaveAsync(stream);
            }
        }

        public async Task LoadDiaryAsync(DateTime date)
        {
            string fileName = $"{date.ToString("yyyy-MM-dd")}.gif";
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            try
            {
                StorageFile file = await localFolder.GetFileAsync(fileName);

                using (var stream = await file.OpenSequentialReadAsync())
                {
                    await inkCanvas.InkPresenter.StrokeContainer.LoadAsync(stream);
                }
            }
            catch (FileNotFoundException)
            {
                // 해당 날짜의 파일이 없으면, 아무것도 하지 않습니다.
            }
        }
    }
}