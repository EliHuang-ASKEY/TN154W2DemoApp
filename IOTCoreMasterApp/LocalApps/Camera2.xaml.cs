using IOTCoreMasterApp.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Devices;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// 空白頁項目範本已記錄在 http://go.microsoft.com/fwlink/?LinkId=234238

namespace IOTCoreMasterApp.LocalApps
{
    /// <summary>
    /// 可以在本身使用或巡覽至框架內的空白頁面。
    /// </summary>
    public sealed partial class Camera2 : Page
    {
        private MediaCapture mediaCapture;
        private StorageFile photoFile;
        private StorageFile recordStorageFile;
        //private StorageFile audioFile;
        private readonly string PHOTO_FILE_NAME = "photo.jpg";
        private readonly string VIDEO_FILE_NAME = "video.mp4";
        
        private bool isPreviewing;
        private bool isRecording;
        private bool isInitialized;
        private bool _isShowing;
        private bool isCapturing;

        private bool _isFocused;
        private bool _settingUpUi;
        private bool _isFocusBtn;
        private bool _isZoomBtn;
        //private bool _singleControlMode;
        private readonly SystemNavigationManager _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private DisplayOrientations _displayOrientation = DisplayOrientations.Portrait;

        // Rotation metadata to apply to the preview stream and recorded videos (MF_MT_VIDEO_ROTATION)
        // Reference: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");


        private DispatcherTimer timerProgress = new DispatcherTimer();
        private int MaxPreviewTime = 0;
        private int PreviewTime = 30;

        

        public Camera2()
        {
            this.InitializeComponent();
            InitCamera();   
            
            



            isRecording = false;
            isPreviewing = false;
            _isShowing = false;
            _isFocusBtn = false;
            _isZoomBtn = false;
            isCapturing = false;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StopGpio();
            Debug.WriteLine("Name= " + this.Frame.Content.GetType().ToString());
            InitGPIO();


        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            StopGpio();
            Cleanup();
        }

        private async void InitCamera()
        {
            try
            {
                if (mediaCapture != null)
                {
                    // Cleanup MediaCapture object
                    if (isPreviewing)
                    {
                        await mediaCapture.StopPreviewAsync();
                        //captureImage.Source = null;
                        //playbackElement.Source = null;
                        isPreviewing = false;
                    }
                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        isRecording = false;
                        //recordVideo.Content = "Start Video Record";
                        //recordAudio.Content = "Start Audio Record";
                    }
                    mediaCapture.Dispose();
                    mediaCapture = null;
                }

                status.Text = "Initializing camera to capture audio and video...";
                // Use default initialization
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                //var _hdrSupported = mediaCapture.VideoDeviceController.AdvancedPhotoControl.SupportedModes.Contains(Windows.Media.Devices.AdvancedPhotoMode.Hdr);
                //Debug.WriteLine("Camera HDR:"+_hdrSupported.ToString());

                // Set callbacks for failure and recording limit exceeded
                status.Text = "Device successfully initialized for video recording!";
                mediaCapture.Failed += new MediaCaptureFailedEventHandler(mediaCapture_Failed);
                mediaCapture.RecordLimitationExceeded += new Windows.Media.Capture.RecordLimitationExceededEventHandler(mediaCapture_RecordLimitExceeded);

                // Start Preview                
                previewElement.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                /*
                /////調整影像解析度
                IEnumerable<StreamResolution> allProperties = mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));               
                foreach (var property in allProperties)
                {
                    if(property.Height == property.Width & property.Height == 640)
                    {
                        status.Text += property.Height + "*" + property.Width + "_";
                        var encodingProperties = (property as StreamResolution).EncodingProperties;
                        await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
                    }
                }
                */
                //Preview 旋轉
                /*
                var props = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                props.Properties.Add(RotationKey, 90);
                await mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
                */

                isPreviewing = true;
                //設定計時間隔 為1秒 & 計時開始 
                this.timerProgress.Interval = new TimeSpan(0, 0, 0, 1);
                timerProgress.Tick += UpdateTime;
                this.timerProgress.Start();

                status.Text += "Camera preview succeeded";
                UpdateFocusControlCapabilities();
                UpdateZoomControlCapabilities();
                // Enable buttons for video and photo capture
                // SetVideoButtonVisibility(Action.ENABLE);

                // Enable Audio Only Init button, leave the video init button disabled
                //audio_init.IsEnabled = true;
            }
            catch (Exception ex)
            {
                status.Text = "Unable to initialize camera for audio/video mode: " + ex.Message;
                status.Visibility = Visibility.Visible;
            }
        }


        private async void Cleanup()
        {
            if (mediaCapture != null)
            {
                // Cleanup MediaCapture object
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                    captureImage.Source = null;
                    ShowImage.Source = null;
                    if (this.timerProgress.IsEnabled)
                    {
                        this.timerProgress.Stop();
                        timerProgress.Tick -= UpdateTime;
                    }
                    //playbackElement.Source = null;
                    isPreviewing = false;
                }
                if (isRecording)
                {
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;
                    //recordVideo.Content = "Start Video Record";
                    //recordAudio.Content = "Start Audio Record";
                }
                mediaCapture.Dispose();
                mediaCapture = null;
            }
           

            isInitialized=false;
    }

       


        private void cleanup_Click(object sender, RoutedEventArgs e)
        {
            
            Cleanup();
        }






        
        /*
        private async void recordVideo_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                recordVideo.IsEnabled = false;
                playbackElement.Source = null;

                if (recordVideo.Content.ToString() == "Start Video Record")
                {

                    status.Text = "Initialize video recording";
                    String fileName;
                    fileName = VIDEO_FILE_NAME;

                    recordStorageFile = await Windows.Storage.KnownFolders.VideosLibrary.CreateFileAsync(fileName, Windows.Storage.CreationCollisionOption.GenerateUniqueName);

                    status.Text = "Video storage file preparation successful";

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(Windows.Media.MediaProperties.VideoEncodingQuality.Auto);

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);
                    VideoButton.IsEnabled = true;
                    
                    isRecording = true;
                    status.Text = "Video recording in progress... press \'Stop Video Record\' to stop";
                }
                else
                {

                    status.Text = "Stopping video recording...";
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;

                    var stream = await recordStorageFile.OpenReadAsync();
                    playbackElement.AutoPlay = true;
                    playbackElement.SetSource(stream, recordStorageFile.FileType);
                    playbackElement.Play();
                    status.Text = "Playing recorded video" + recordStorageFile.Path;
                    
                }
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException)
                {
                    status.Text = "Unable to play recorded video; video recorded successfully to: " + recordStorageFile.Path;
                   
                }
                else
                {
                    status.Text = ex.Message;
                    Cleanup();
                }
            }
            finally
            {
                VideoButton.IsEnabled = true;
            }
        }
        */


       
        private async void mediaCapture_Failed(MediaCapture currentCaptureObject, MediaCaptureFailedEventArgs currentFailure)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    status.Text = "MediaCaptureFailed: " + currentFailure.Message;

                    if (isRecording)
                    {
                        await mediaCapture.StopRecordAsync();
                        status.Text += "\n Recording Stopped";
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    
                    status.Text += "\nCheck if camera is diconnected. Try re-launching the app";
                }
            });
        }

        
        public async void mediaCapture_RecordLimitExceeded(Windows.Media.Capture.MediaCapture currentCaptureObject)
        {
            try
            {
                if (isRecording)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        try
                        {
                            status.Text = "Stopping Record on exceeding max record duration";
                            await mediaCapture.StopRecordAsync();
                            isRecording = false;

                            if (mediaCapture.MediaCaptureSettings.StreamingCaptureMode == StreamingCaptureMode.Audio)
                            {
                                status.Text = "Stopped record on exceeding max record duration: audioFile.Path "; //+ audioFile.Path;
                            }
                            else
                            {
                                status.Text = "Stopped record on exceeding max record duration: " + recordStorageFile.Path;
                            }
                        }
                        catch (Exception e)
                        {
                            status.Text = e.Message;
                        }
                    });
                }
            }
            catch (Exception e)
            {
                status.Text = e.Message;
            }
        }

        private void appBarButton_Click(object sender, RoutedEventArgs e)
        {
            //this.Frame.Navigate(typeof(MainPage));
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
        }

        private void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            takePhoto();
            //this.PhotoButton.Visibility = Visibility.Collapsed;
            
            

        }
        private async void takePhoto()
        {
            this.isCapturing = true;
            Debug.WriteLine("isCapturing" + isCapturing);
            this.PhotoButton.IsEnabled = false;
            status.Text = "Take Photo Start: ";
            var stream = new InMemoryRandomAccessStream();

            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);
            //await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8), stream);

            try
            {

                VideoButton.IsEnabled = false;
                captureImage.Source = null;

                photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync(
                    PHOTO_FILE_NAME, CreationCollisionOption.GenerateUniqueName);
                ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateJpeg();
                //ImageEncodingProperties imageProperties = ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8);
                await mediaCapture.CapturePhotoToStorageFileAsync(imageProperties, photoFile);

                //照片旋轉
                //await ReencodeAndSavePhotoAsync(stream, photoFile, PhotoOrientation.Rotate270);

                status.Text = "Take Photo succeeded: " + photoFile.Path;

                IRandomAccessStream photoStream = await photoFile.OpenReadAsync();
                BitmapImage bitmap = new BitmapImage();
                bitmap.SetSource(photoStream);
                captureImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                status.Text = ex.Message+"@";
                Cleanup();
            }
            finally
            {

                VideoButton.IsEnabled = true;
            }
            this.isCapturing = false;
            Debug.WriteLine("isCapturing"+isCapturing);
            this.PhotoButton.IsEnabled = true;
        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            
            try
            {

                VideoButton.IsEnabled = false;
                //playbackElement.Source = null;

                if (isRecording == false)
                {
                    captureImage.Visibility = Visibility.Collapsed;
                    PhotoButton.Visibility = Visibility.Collapsed;
                    appBarButton.Visibility = Visibility.Collapsed;
                    status.Text = "Initialize video recording";
                    String fileName;
                    fileName = VIDEO_FILE_NAME;

                    recordStorageFile = await KnownFolders.VideosLibrary.CreateFileAsync(fileName, CreationCollisionOption.GenerateUniqueName);

                    status.Text = "Video storage file preparation successful";

                    MediaEncodingProfile recordProfile = null;
                    recordProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);

                    var rotationAngle = 180;
                    recordProfile.Video.Properties.Add(RotationKey, PropertyValue.CreateInt32(rotationAngle));

                    await mediaCapture.StartRecordToStorageFileAsync(recordProfile, recordStorageFile);
                    VideoButton.IsEnabled = true;

                    isRecording = true;
                    status.Text = "Video recording in progress... press \'Stop Video Record\' to stop";
                }
                else
                {
                    captureImage.Visibility = Visibility.Visible;
                    PhotoButton.Visibility = Visibility.Visible;
                    appBarButton.Visibility = Visibility.Visible;
                    status.Text = "Stopping video recording...";
                    await mediaCapture.StopRecordAsync();
                    isRecording = false;

                    var stream = await recordStorageFile.OpenReadAsync();
                    //playbackElement.AutoPlay = true;
                    //playbackElement.SetSource(stream, recordStorageFile.FileType);
                    //playbackElement.Play();
                    status.Text = "Playing recorded video" + recordStorageFile.Path;

                }
            }
            catch (Exception ex)
            {
                if (ex is System.UnauthorizedAccessException)
                {
                    status.Text = "Unable to play recorded video; video recorded successfully to: " + recordStorageFile.Path;

                }
                else
                {
                    status.Text = ex.Message;
                    Cleanup();
                }
            }
            finally
            {
                VideoButton.IsEnabled = true;
            }

            // After starting or stopping video recording, update the UI to reflect the MediaCapture state
            UpdateCaptureControls();


        }


        private void UpdateCaptureControls()
        {
            // The buttons should only be enabled if the preview started sucessfully
            PhotoButton.IsEnabled = isPreviewing;
            VideoButton.IsEnabled = isPreviewing;

            // Update recording button to show "Stop" icon instead of red "Record" icon
            StartRecordingIcon.Visibility = isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility = isRecording ? Visibility.Visible : Visibility.Collapsed;

            // If the camera doesn't support simultaneosly taking pictures and recording video, disable the photo button on record
            
            if (isInitialized && !mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported)
            {
                PhotoButton.IsEnabled = !isRecording;

                // Make the button invisible if it's disabled, so it's obvious it cannot be interacted with
                PhotoButton.Opacity = PhotoButton.IsEnabled ? 1 : 0;
            }
            
        }

        private  void CloseShowImage(object sender, TappedRoutedEventArgs e)
        {
            CloseImage();
            
        }
        private async void CloseImage()
        {
            if (_isShowing)
            {
                _isShowing = false;
                this.timerProgress.Start();
                await mediaCapture.StartPreviewAsync();
                
                /*
                var props = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                props.Properties.Add(RotationKey, 90);
                await mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
                */
                ShowImage.Visibility = Visibility.Collapsed;
                previewElement.Visibility = Visibility.Visible;
                appBarButton.Visibility = Visibility.Visible;
                TextTime.Visibility = Visibility.Visible;
                ScenarioControlStackPanel.Visibility = Visibility.Visible;
                
                //VideoButton.Visibility = Visibility.Visible;
                PhotoButton.Visibility = Visibility.Visible;
                captureImage.Visibility = Visibility.Visible;
                ShowImage.Source = null;
                status.Visibility = Visibility.Visible;
                status.Text = "Go Back Camera...";
                
            }

        }

        private void ShowPhoto(object sender, TappedRoutedEventArgs e)
        {
            if (captureImage.Source == null)
            {
                _isShowing = false;
                status.Text = "Please take a Photo..";
            }
            else ShowAllPhoto();


        }
        private async void ShowAllPhoto()
        {
            ShowImage.Source = null;
            if (!_isShowing)
            {
                this.timerProgress.Stop();
                await mediaCapture.StopPreviewAsync();
                ShowImage.Source = captureImage.Source;
                
                ShowImage.Visibility = Visibility.Visible;
                previewElement.Visibility = Visibility.Collapsed;
                appBarButton.Visibility = Visibility.Collapsed;
                TextTime.Visibility = Visibility.Collapsed;
                ScenarioControlStackPanel.Visibility = Visibility.Collapsed;
                
                //VideoButton.Visibility = Visibility.Collapsed;
                PhotoButton.Visibility = Visibility.Collapsed;
                captureImage.Visibility = Visibility.Collapsed;
                status.Visibility = Visibility.Collapsed;
                status.Text = "Show Photo...";
                _isShowing = true;
            }
            else
            {

            }
        }
        private static async Task ReencodeAndSavePhotoAsync(IRandomAccessStream stream, StorageFile file, Windows.Storage.FileProperties.PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);

                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);

                    var properties = new BitmapPropertySet { { "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16) } };

                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }



        private void InitGPIO()
        {
            Debug.WriteLine("InitGPIO");
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                status.Text = "There is no GPIO controller on this device.";
                return;
            }
            
            buttonPin = gpio.OpenPin(BUTTON_PIN);
            
            

            // Check if input pull-up resistors are supported
            if (buttonPin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                buttonPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                buttonPin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so our buttonPin_ValueChanged 
            // function is called when the button is pressed
            buttonPin.ValueChanged += buttonPin_ValueChanged;

            status.Text = "GPIO pins initialized correctly.";
            
        }
       

        private void buttonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs e)
        {

            if (!isCapturing)
            {
                // need to invoke UI updates on the UI thread because this event
                // handler gets invoked on a separate thread.
                var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
                if (e.Edge == GpioPinEdge.FallingEdge)
                {
                    //ledEllipse.Fill = (ledPinValue == GpioPinValue.Low) ? 
                    status.Text = "Button Pressed";
                    
                        takePhoto();
                        this.PhotoButton.IsEnabled = false;
                    

                }
                else
                {

                    status.Text = "Button Released";
                }
                });

            }
            else Debug.WriteLine("Please wait few second for Caputre");
        }

        private void StopGpio()
        {
            if (buttonPin != null)
            {
                buttonPin.Dispose();
                buttonPin = null;
            }

        }
        
        private const int BUTTON_PIN = 92;       
        private GpioPin buttonPin;

        
        #region Focus
        //############################################Focus###############################################
        private void UpdateFocusControlCapabilities()
        {
            var focusControl = mediaCapture.VideoDeviceController.FocusControl;
            
            if (focusControl.Supported)
            {
                //FocusButton.Tag = Visibility.Visible;

                // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                FocusSlider.ValueChanged -= FocusSlider_ValueChanged;

                var value = focusControl.Value;
                FocusSlider.Minimum = focusControl.Min;
                FocusSlider.Maximum = focusControl.Max;
                FocusSlider.StepFrequency = focusControl.Step;
                FocusSlider.Value = value;

                FocusSlider.ValueChanged += FocusSlider_ValueChanged;

                CafFocusRadioButton.Visibility = focusControl.SupportedFocusModes.Contains(FocusMode.Continuous) ? Visibility.Visible : Visibility.Collapsed;

                // Tap to focus requires support for RegionsOfInterest
                //TapFocusRadioButton.Visibility = (mediaCapture.VideoDeviceController.RegionsOfInterestControl.AutoFocusSupported &&
                //                                  mediaCapture.VideoDeviceController.RegionsOfInterestControl.MaxRegions > 0) ? Visibility.Visible : Visibility.Collapsed;

                // Show the focus assist light only if the device supports it. Note that it exists under the FlashControl (not the FocusControl), so check for support there first
                //FocusLightCheckBox.Visibility = (mediaCapture.VideoDeviceController.FlashControl.Supported &&
                //                                 mediaCapture.VideoDeviceController.FlashControl.AssistantLightSupported) ? Visibility.Visible : Visibility.Collapsed;

                //ManualFocusRadioButton.IsChecked = true;
                CafFocusRadioButton.IsChecked = true;

                

            }
            else
            {
                //FocusButton.Visibility = Visibility.Collapsed;
                //FocusButton.Tag = Visibility.Collapsed;
            }
        }

        private void FocusLightCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            var flashControl = mediaCapture.VideoDeviceController.FlashControl;

            flashControl.AssistantLightEnabled = (FocusLightCheckBox.IsChecked == true);
        }

        private async void FocusSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_settingUpUi) return;

            var value = (sender as Slider).Value;

            await mediaCapture.VideoDeviceController.FocusControl.SetValueAsync((uint)value);
        }

        private async void ManualFocusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // The below check is necessary because this event triggers during initialization of the page, before the MediaCapture is up
            if (!isPreviewing) return;

            // Reset tap-to-focus status
            _isFocused = false;
            FocusRectangle.Visibility = Visibility.Collapsed;

            // Lock focus in case Continuous Autofocus was active when switching to Manual
            var focusControl = mediaCapture.VideoDeviceController.FocusControl;
            await focusControl.LockAsync();
        }

        private async void CafFocusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Reset tap-to-focus status
            _isFocused = false;
            FocusRectangle.Visibility = Visibility.Collapsed;

            var focusControl = mediaCapture.VideoDeviceController.FocusControl;

            await focusControl.UnlockAsync();

            var settings = new FocusSettings { Mode = FocusMode.Continuous, AutoFocusRange = AutoFocusRange.FullRange };
            focusControl.Configure(settings);
            await focusControl.FocusAsync();
        }
        

        private async void TapFocusRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // Lock focus in case Continuous Autofocus was active when switching to Tap-to-focus
            var focusControl = mediaCapture.VideoDeviceController.FocusControl;
            await focusControl.LockAsync();

            // Wait for user tap, which will be handled in PreviewControl_Tapped
        }

        /// <summary>
        /// Focus the camera on the given rectangle of the preview, defined by the position and size parameters, in UI coordinates within the CaptureElement
        /// </summary>
        /// <param name="position">The position of the tap, to become the center of the focus rectangle</param>
        /// <param name="size">the size of the rectangle around the tap</param>
        /// <returns></returns>
        public async Task TapToFocus(Point position, Size size)
        {
            // Transition to the "focused" state
            _isFocused = true;

            var previewEncodingProperties = mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            var previewRect = GetPreviewStreamRectInControl(previewEncodingProperties, previewElement, _displayOrientation);

            // Build UI rect that will highlight the tapped area
            FocusRectangle.Width = size.Width;
            FocusRectangle.Height = size.Height;

            // Adjust for the rect to be centered around the tap position 
            var left = position.X - FocusRectangle.Width / 2;
            var top = position.Y - FocusRectangle.Height / 2;

            // Move it so it's contained within the preview stream in the UI and doesn't reach into letterboxing area or outside of window bounds

            // The left and right edges should not be outside the active preview area within the CaptureElement
            left = Math.Max(previewRect.Left, left);
            left = Math.Min(previewRect.Width - FocusRectangle.Width + previewRect.Left, left);

            // The top and bottom edges should not be outside the active preview area within the CaptureElement
            top = Math.Max(previewRect.Top, top);
            top = Math.Min(previewRect.Height - FocusRectangle.Height + previewRect.Top, top);

            // Apply the adjusted position to the FocusRectangle
            Canvas.SetLeft(FocusRectangle, left);
            Canvas.SetTop(FocusRectangle, top);

            FocusRectangle.Stroke = new SolidColorBrush(Colors.White);
            FocusRectangle.Visibility = Visibility.Visible;

            // FocusRectangle exists in UI coordinates, need to convert to preview coordinates and adjust for rotation if necessary
            var focusPreview = ConvertUiTapToPreviewRect(position, size, previewRect);

            // Note that this Region Of Interest could be configured to also calculate exposure and white balance within the region
            var regionOfInterest = new RegionOfInterest
            {
                AutoFocusEnabled = true,
                BoundsNormalized = true,
                Bounds = focusPreview,
                Type = RegionOfInterestType.Unknown,
                Weight = 100,
            };

            var result = await FocusCamera(regionOfInterest);

            // Update the UI based on the result of the focusing operation
            FocusRectangle.Stroke = (result == MediaCaptureFocusState.Focused ? new SolidColorBrush(Colors.Lime) : new SolidColorBrush(Colors.Red));
        }

        private async Task TapUnfocus()
        {
            _isFocused = false;
            FocusRectangle.Visibility = Visibility.Collapsed;
            await FocusCamera(null);
        }

        /// <summary>
        /// Sets camera to focus on the passed in region of interest
        /// </summary>
        /// <param name="region">The region to focus on, or null to focus on the default region</param>
        /// <returns></returns>
        private async Task<MediaCaptureFocusState> FocusCamera(RegionOfInterest region)
        {
            var roiControl = mediaCapture.VideoDeviceController.RegionsOfInterestControl;
            var focusControl = mediaCapture.VideoDeviceController.FocusControl;

            if (region != null)
            {
                // If the call provided a region, then set it
                await roiControl.SetRegionsAsync(new[] { region }, true);

                var focusRange = focusControl.SupportedFocusRanges.Contains(AutoFocusRange.FullRange) ? AutoFocusRange.FullRange : focusControl.SupportedFocusRanges.FirstOrDefault();
                var focusMode = focusControl.SupportedFocusModes.Contains(FocusMode.Single) ? FocusMode.Single : focusControl.SupportedFocusModes.FirstOrDefault();

                var settings = new FocusSettings { Mode = focusMode, AutoFocusRange = focusRange };

                focusControl.Configure(settings);
            }
            else
            {
                // If no region provided, clear any regions and reset focus
                await roiControl.ClearRegionsAsync();
            }

            await focusControl.FocusAsync();

            return focusControl.FocusState;
        }

        /// <summary>
        /// Applies the necessary rotation to a tap on a CaptureElement (with Stretch mode set to Uniform) to account for device orientation
        /// </summary>
        /// <param name="tap">The location, in UI coordinates, of the user tap</param>
        /// <param name="size">The size, in UI coordinates, of the desired focus rectangle</param>
        /// <param name="previewRect">The area within the CaptureElement that is actively showing the preview, and is not part of the letterboxed area</param>
        /// <returns>A Rect that can be passed to the MediaCapture Focus and RegionsOfInterest APIs, with normalized bounds in the orientation of the native stream</returns>
        private Rect ConvertUiTapToPreviewRect(Point tap, Size size, Rect previewRect)
        {
            // Adjust for the resulting focus rectangle to be centered around the position
            double left = tap.X - size.Width / 2, top = tap.Y - size.Height / 2;

            // Get the information about the active preview area within the CaptureElement (in case it's letterboxed)
            double previewWidth = previewRect.Width, previewHeight = previewRect.Height;
            double previewLeft = previewRect.Left, previewTop = previewRect.Top;

            // Transform the left and top of the tap to account for rotation
            switch (_displayOrientation)
            {
                case DisplayOrientations.Portrait:
                    var tempLeft = left;

                    left = top;
                    top = previewRect.Width - tempLeft;
                    break;
                case DisplayOrientations.LandscapeFlipped:
                    left = previewRect.Width - left;
                    top = previewRect.Height - top;
                    break;
                case DisplayOrientations.PortraitFlipped:
                    var tempTop = top;

                    top = left;
                    left = previewRect.Width - tempTop;
                    break;
            }

            // For portrait orientations, the information about the active preview area needs to be rotated
            if (_displayOrientation == DisplayOrientations.Portrait || _displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                previewWidth = previewRect.Height;
                previewHeight = previewRect.Width;
                previewLeft = previewRect.Top;
                previewTop = previewRect.Left;
            }

            // Normalize width and height of the focus rectangle
            var width = size.Width / previewWidth;
            var height = size.Height / previewHeight;

            // Shift rect left and top to be relative to just the active preview area
            left -= previewLeft;
            top -= previewTop;

            // Normalize left and top
            left /= previewWidth;
            top /= previewHeight;

            // Ensure rectangle is fully contained within the active preview area horizontally
            left = Math.Max(left, 0);
            left = Math.Min(1 - width, left);

            // Ensure rectangle is fully contained within the active preview area vertically
            top = Math.Max(top, 0);
            top = Math.Min(1 - height, top);

            // Create and return resulting rectangle
            return new Rect(left, top, width, height);
        }
        #endregion Focus
        //##################################################################################################
        public static Rect GetPreviewStreamRectInControl(VideoEncodingProperties previewResolution, CaptureElement previewControl, DisplayOrientations displayOrientation)
        {
            var result = new Rect();

            // In case this function is called before everything is initialized correctly, return an empty result
            if (previewControl == null || previewControl.ActualHeight < 1 || previewControl.ActualWidth < 1 ||
                previewResolution == null || previewResolution.Height == 0 || previewResolution.Width == 0)
            {
                return result;
            }

            var streamWidth = previewResolution.Width;
            var streamHeight = previewResolution.Height;

            // For portrait orientations, the width and height need to be swapped
            if (displayOrientation == DisplayOrientations.Portrait || displayOrientation == DisplayOrientations.PortraitFlipped)
            {
                streamWidth = previewResolution.Height;
                streamHeight = previewResolution.Width;
            }

            // Start by assuming the preview display area in the control spans the entire width and height both (this is corrected in the next if for the necessary dimension)
            result.Width = previewControl.ActualWidth;
            result.Height = previewControl.ActualHeight;

            // If UI is "wider" than preview, letterboxing will be on the sides
            if ((previewControl.ActualWidth / previewControl.ActualHeight > streamWidth / (double)streamHeight))
            {
                var scale = previewControl.ActualHeight / streamHeight;
                var scaledWidth = streamWidth * scale;

                result.X = (previewControl.ActualWidth - scaledWidth) / 2.0;
                result.Width = scaledWidth;
            }
            else // Preview stream is "wider" than UI, so letterboxing will be on the top+bottom
            {
                var scale = previewControl.ActualWidth / streamWidth;
                var scaledHeight = streamHeight * scale;

                result.Y = (previewControl.ActualHeight - scaledHeight) / 2.0;
                result.Height = scaledHeight;
            }

            return result;
        }

        private async void PreviewElementTap(object sender, TappedRoutedEventArgs e)
        {
            if (!isPreviewing || (TapFocusRadioButton.IsChecked != true)) return;

            if (!_isFocused && mediaCapture.VideoDeviceController.FocusControl.FocusState != MediaCaptureFocusState.Searching)
            {
                var smallEdge = Math.Min(Window.Current.Bounds.Width, Window.Current.Bounds.Height);

                // Choose to make the focus rectangle 1/4th the length of the shortest edge of the window
                var size = new Size(smallEdge / 4, smallEdge / 4);
                var position = e.GetPosition(sender as UIElement);

                // Note that at this point, a rect at "position" with size "size" could extend beyond the preview area. The following method will reposition the rect if that is the case
                await TapToFocus(position, size);
            }
            else
            {
                await TapUnfocus();
            }
        }
        private void ManualControlButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle single control mode
            //SetSingleControl(_singleControlMode ? null : sender);
            
            if (!_isFocusBtn)
            {
                ManualControlsGrid.Visibility = Visibility.Visible;
                _isFocusBtn = true;
                ZoomButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ManualControlsGrid.Visibility = Visibility.Collapsed;
                _isFocusBtn = false;
                ZoomButton.Visibility = Visibility.Visible;
            }
            
        }
       
        #region Zoom
        private void UpdateZoomControlCapabilities()
        {
            var zoomControl = mediaCapture.VideoDeviceController.ZoomControl;

            if (zoomControl.Supported)
            {
                ZoomButton.Tag = Visibility.Visible;

                // Unhook the event handler, so that changing properties on the slider won't trigger an API call
                ZoomSlider.ValueChanged -= ZoomSlider_ValueChanged;

                var value = zoomControl.Value;
                ZoomSlider.Minimum = zoomControl.Min;
                ZoomSlider.Maximum = zoomControl.Max;
                ZoomSlider.StepFrequency = zoomControl.Step;
                ZoomSlider.Value = value;

                ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
            }
            else
            {
                ZoomButton.Visibility = Visibility.Collapsed;
                ZoomButton.Tag = Visibility.Collapsed;
            }
        }

        private void ZoomSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_settingUpUi) return;

            SetZoomLevel((float)ZoomSlider.Value);
        }

        private void SetZoomLevel(float level)
        {
            var zoomControl = mediaCapture.VideoDeviceController.ZoomControl;

            // Make sure zoomFactor is within the valid range
            level = Math.Max(Math.Min(level, zoomControl.Max), zoomControl.Min);

            // Make sure zoomFactor is a multiple of Step, snap to the next lower step
            level -= (level % zoomControl.Step);

            var settings = new ZoomSettings { Value = level };

            if (zoomControl.SupportedModes.Contains(ZoomTransitionMode.Smooth))
            {
                // Favor smooth zoom for this sample
                settings.Mode = ZoomTransitionMode.Smooth;
            }
            else
            {
                settings.Mode = zoomControl.SupportedModes.First();
            }

            zoomControl.Configure(settings);
        }
        #endregion Zoom

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isZoomBtn)
            {
                ManualControlsGrid.Visibility = Visibility.Visible;
                _isZoomBtn = true;
                FocusButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                ManualControlsGrid.Visibility = Visibility.Collapsed;
                _isZoomBtn = false;
                FocusButton.Visibility = Visibility.Collapsed;
            }
        }


        private void UpdateTime(object sender, object e)
        {

            if (isPreviewing) PreviewTime--;
            if(PreviewTime<10) this.TextTime.Text = "00:0" + PreviewTime;
            else this.TextTime.Text = "00:" + PreviewTime ;
            if (PreviewTime > MaxPreviewTime) return;



            try
            {
               

                status.Text = "Stop....";
                this.timerProgress.Stop();
                timerProgress.Tick -= UpdateTime;
                
                //if (_isShowing) CloseImage();
                //StopGpio();
                //Cleanup();

                if (this.Frame.CanGoBack)
                {
                    this.Frame.GoBack();
                }
            }
            catch (Exception ex)
            {

                Debug.WriteLine("UpdateTime Error :" + ex.Message);
            }


        }
    }
}
