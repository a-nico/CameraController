using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using System.Windows.Forms;
using System.IO;

namespace CameraController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        CameraControl camControl;
        bool camStarted;
        float bigModifier = 1.6f;
        float smallModifier = 1.2f;
        public string AviPath
        {
            get
            {
                var saved = Properties.Settings.Default.savepath;
                if (saved == null || saved == "")
                {
                    return "C:\\Captures";
                }
                return saved;
            }
            set
            {
                Properties.Settings.Default.savepath = value;
                Properties.Settings.Default.Save();
                NotifyPropertyChanged("AviPath");
            }
        }
        CameraControl.AviType aviFormat;
        bool isSaving;

        public MainWindow()
        {
            
            InitializeComponent();
            AviPathView.DataContext = this;
            aviFormat = CameraControl.AviType.H264; // default format
            //var selectedImg = ThumbnailBox.SelectedItem;
        }

        private async void Record_Click(object sender, RoutedEventArgs e)
        {

            if (camControl != null)
            {
                var button = (System.Windows.Controls.Button)sender;
                // if it's idle, start recording
                if (!camControl.isCapturing && !camControl.isRecording && !camControl.IsAviWriting)
                {
                    camControl.StartRecording(AviPath, aviFormat);
                    button.Foreground = Brushes.Red;
                    button.Content = "** STOP **";
                }
                // if Recording or Capturing, Stop
                else
                {
                    camControl.isRecording = false;
                    camControl.isCapturing = false;
                    button.IsEnabled = false;
                    // wait for AVI to finish writing
                    await Task.Run(() =>
                    {
                        while (camControl.IsAviWriting) ;
                    });
                    button.IsEnabled = true;
                    button.Foreground = Brushes.Black;
                    button.Content = "Record";
                }
            }
        }

        private void StartCam_Click(object sender, RoutedEventArgs e)
        {
            if (camControl == null)
            {
                camControl = new CameraControl(this.Dispatcher);
                
                if (camControl.InitializeCamera())
                {
                    MainGrid.DataContext = camControl; // every child in MainGrid gets context in this object
                    StartCam_Click(sender, e);
                    camStarted = true;
                }
                else
                {
                    camControl.Dispose();
                    camControl = null;
                }
            } 
            else
            {
                if (camControl.isLive)
                {
                    camControl.isLive = false;
                    StartStopButton.Content = "Go Live";
                }
                else
                {
                    camControl.GoLive();
                    StartStopButton.Content = "Stop Cam";

                }
                
            }

        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            var result = folderDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                AviPath = folderDialog.SelectedPath;
            }
        }
                
        private void FormatChanged(object sender, RoutedEventArgs e)
        { // radio button handler

            System.Windows.Controls.RadioButton bt = sender as System.Windows.Controls.RadioButton;
            switch (bt.Name)
            {
                case "Uncompressed":
                    aviFormat = CameraControl.AviType.Uncompressed;
                    break;
                case "Mjpg":
                    aviFormat = CameraControl.AviType.Mjpg;
                    break;
                case "H264":
                    aviFormat = CameraControl.AviType.H264;
                    break;
            }

        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {
            if (camControl != null)
            {
                var button = (System.Windows.Controls.Button)sender;
                // if it's idle, start recording
                if (!camControl.isCapturing && !camControl.isRecording && !camControl.IsAviWriting)
                {
                    camControl.Capture();
                    button.Foreground = Brushes.Red;
                    button.Content = "** STOP **";
                }
                else
                {
                    camControl.isRecording = false;
                    camControl.isCapturing = false;
                    button.Foreground = Brushes.Black;
                    button.Content = "Capture";
                }

            }

        }

        private void SaveCaptures_Click(object sender, RoutedEventArgs e)
        {
            if (camControl != null && !camControl.IsAviWriting)
            {
                Task.Run( () => 
                {
                    isSaving = true;
                    var baseFolder = MakeImageFolder(AviPath);
                    string basePath = baseFolder + DateTime.Now.ToString("ddd, dd MMM yyyy hh-mm-ss tt") + " ";
                    int k = 1;
                    while (camControl.queueForJPGs.Count > 0)
                    {
                        using (IManagedImage rawImage = camControl.queueForJPGs.Dequeue())
                        {
                            rawImage.Save(basePath + k++ + ".jpg");
                        }
                    }
                    isSaving = false;
                });
            }
        }

        // makes a folder inside "basePath" called "Images" and returns the path to it
        string MakeImageFolder(string basePath)
        {
            string baseFolder = basePath + "\\Images\\";
            System.IO.Directory.CreateDirectory(baseFolder); // creates new folder if it doesn't exist
            return baseFolder;
        }

        // Main window key down handler
        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // if 's' key is hit, save current frame
            if (e.Key == System.Windows.Input.Key.S)
            {
                if (ThumbnailBox.SelectedItem != null && camControl != null)
                {
                    var img = (BitmapSource)ThumbnailBox.SelectedItem;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    BitmapFrame outputFrame = BitmapFrame.Create(img);
                    encoder.Frames.Add(outputFrame);
                    encoder.QualityLevel = camControl.QualityBox;  // in percent I think (so 0 - 100)

                    var baseFolder = MakeImageFolder(AviPath);
                    string path = baseFolder + DateTime.Now.ToString("ddd, dd MMM yyyy hh-mm-ss tt") + ".jpg";
                    using (FileStream file = new FileStream(path, FileMode.Create)) //FileMode.Create will overwrite
                    {
                        encoder.Save(file);
                    }

                }
            }
        }

        #region Other Button Even Handlers
        //private void Stop_Click(object sender, RoutedEventArgs e)
        //{
        //    if (camControl != null)
        //    {
        //        camControl.isRecording = false;
        //        camControl.isCapturing = false;

        //    }
        //}
        private void ExposureMinus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.ExposureBox = (int)(camControl.ExposureBox / bigModifier);
            }
        }

        private void ExposureMinus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.ExposureBox = (int)(camControl.ExposureBox / smallModifier);
            }
        }

        private void ExposurePlus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.ExposureBox = (int)(camControl.ExposureBox * smallModifier);
            }
        }

        private void ExposurePlus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.ExposureBox = (int)(camControl.ExposureBox * bigModifier);
            }
        }

        private void DurationMinus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.DurationBox -= 1;
            }
        }

        private void DurationMinus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.DurationBox -= 5;
            }
        }

        private void DurationPlus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.DurationBox += 1;
            }
        }

        private void DurationPlus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.DurationBox += 5;
            }
        }

        private void FramesMinus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.FramesBox -= 25;
            }
        }

        private void FramesMinus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.FramesBox -= 5;
            }
        }

        private void FramesPlus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.FramesBox += 5;
            }
        }

        private void FramesPlus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.FramesBox += 25;
            }
        }

        private void RateMinus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.RateBox = (int)(camControl.RateBox / bigModifier);
            }
        }

        private void RateMinus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.RateBox = (int)(camControl.RateBox / smallModifier);
            }
        }

        private void RatePlus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.RateBox = (int)(camControl.RateBox * smallModifier);
            }
        }

        private void RatePlus2_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.RateBox = (int)(camControl.RateBox * bigModifier);
            }
        }

        private void TimerMinus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                if (camControl.TimerBox > 0)
                {
                    camControl.TimerBox -= 1;
                }
            }

        }

        private void TimerPlus_Click(object sender, RoutedEventArgs e)
        {
            if (camStarted)
            {
                camControl.TimerBox += 1;
            }
        }
        #endregion

        private async void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
            await Task.Run(() => { while (isSaving) { } }); // wait for saving to finish
            camControl?.Dispose();

        }

        #region PropertyChanged stuff
        // call this method invokes event to update UI elements which use Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #endregion


    }


    // --------------------------------------------------------------------------------

    internal class CameraControl : INotifyPropertyChanged, IDisposable
    {
        # region Fields
        Dispatcher UIDispatcher;
        private ManagedSystem spinnakerSystem; // spinnaker class
        private IManagedCamera cam; // the blackfly
        public bool isLive; // live mode or frame capture on UI
        public bool isRecording; // if direct to AVI is running
        private Queue<IManagedImage> queueForAvi; // waiting to get appended to AVI
        public Queue<IManagedImage> queueForJPGs; // waiting to get saved as JPGs
        public enum AviType
        {
            Uncompressed,
            Mjpg,
            H264
        }
        int frameWidth;
        int frameHeight;
        public bool IsAviWriting { get; private set; }
        public bool isCapturing;
        #endregion


        #region Properties

        public int RateBox
        {
            get
            {
                if (cam != null)
                    return (int)Math.Round(cam.AcquisitionFrameRate.Value);
                return 0;
            }
            set
            {
                SetFramerate(value); // when loses focus, sets camera to what user inputed
                NotifyPropertyChanged("RateBox"); // update UI box (calls get)
                // now update the number of rames
                _FramesBox = _FramesBox = (int)Math.Round(RateBox * DurationBox);
                NotifyPropertyChanged("FramesBox");
            }
        }

        private float _AviRateBox;
        public float AviRateBox
        {
            get
            {
                return _AviRateBox;
            }
            set
            {
                _AviRateBox = value;
                NotifyPropertyChanged("AviRateBox");
            }
        }

        private int _QualityBox;
        public int QualityBox
        {
            get
            {
                return _QualityBox < 1 ? 1 : _QualityBox;
            }
            set
            {
                _QualityBox = value;
                NotifyPropertyChanged("QualityBox");
            }
        }


        private ImageSource _MainImage;
        public ImageSource MainImage
        {
            get { return _MainImage; }
            set
            {
                _MainImage = value;
                NotifyPropertyChanged("MainImage");
            }
        }

        // gets filled when "Capture" UI button is clicked
        private ObservableCollection<BitmapSource> _imageSourceThumbnails;
        public ObservableCollection<BitmapSource> ImageSourceThumbnails
        {   //"observable collection" automatically notifies UI when changed
            get
            {
                return _imageSourceThumbnails;
            }

            set
            {
                _imageSourceThumbnails = value;
                NotifyPropertyChanged("ImageSourceThumbnails");
            }
        }

        private int _FramesBox;
        public int FramesBox
        {
            get
            {
                return _FramesBox < 5 ? 5 : _FramesBox; // can't be <2 for MultiFrame mode
            }
            set
            {
                _FramesBox = value;
                _DurationBox = (value / RateBox);
                NotifyPropertyChanged("FramesBox");
                NotifyPropertyChanged("DurationBox");
            }
        }

        private float _DurationBox;
        public float DurationBox
        {
            get
            {
                return _DurationBox;
            }
            set
            {
                _DurationBox = value;
                _FramesBox = (int)Math.Round(RateBox * value);
                NotifyPropertyChanged("DurationBox");
                NotifyPropertyChanged("FramesBox");
            }
        }

        private int _TimerBox;
        

        public int TimerBox
        {
            get
            {
                return _TimerBox;
            }
            set
            {
                _TimerBox = value;
                NotifyPropertyChanged("TimerBox");
            }
        }



        #endregion

        // constructor
        public CameraControl(Dispatcher UIDispatcher)
        {
            this.UIDispatcher = UIDispatcher;
            queueForAvi = new Queue<IManagedImage>();
            queueForJPGs = new Queue<IManagedImage>();

            // create collecton on UI thread so I won't have any problems with scope BS
            UIDispatcher?.BeginInvoke(new Action(() =>
            {
                ImageSourceThumbnails = new ObservableCollection<BitmapSource>();
            }));

        }

        public void GoLive()
        {
            isLive = true;

            Task.Run(new Action(() =>
            {
                SetAcqusitionMode(AcquisitionMode.Continuous);
                cam.BeginAcquisition();
                

                while (isLive)
                {
                    using (IManagedImage rawImage = cam.GetNextImage())
                    {
                        // set width/height only first time
                        if (frameWidth == 0 || frameHeight == 0)
                        {
                            frameHeight = (int)rawImage.Height;
                            frameWidth = (int)rawImage.Width;
                        }

                        // flash it on screen
                        // updating UI image has to be done on UI thread. Use Dispatcher
                        UIDispatcher.Invoke(new Action(() =>
                        {
                            MainImage = ConvertRawToBitmapSource(rawImage);
                        }));

                        if (isRecording)
                        {
                            // Deep copy image into queue for making AVI
                            queueForAvi.Enqueue(rawImage.Convert(PixelFormatEnums.Mono8));
                        }

                    }
                }
                cam.EndAcquisition(); // done with camera
            })); // end Task
        }

        
        public async void StartRecording(string aviFileDirectory, AviType fileType)
        {
            if (IsAviWriting) return; // don't start recording until it's done with previous
            IsAviWriting = true;
            System.IO.Directory.CreateDirectory(aviFileDirectory); // creates new folder if it doesn't exist
            
            string name = DateTime.Now.ToString("ddd, dd MMM yyyy hh-mm-ss tt");
            //string name = "test";
            string aviFilename = aviFileDirectory + "\\" + name;
            //string aviFilename = "C:\\Captures\\test";

            // start eating up the queue and appending to AVI
            await Task.Run(new Action(() =>
            {
                System.Threading.Thread.Sleep(1000 * TimerBox);
                isRecording = true;
                IManagedAVIRecorder aviRecorder = new ManagedAVIRecorder();
                try
                {
                    switch (fileType)
                    {
                        case AviType.Uncompressed:
                            AviOption uncompressedOption = new AviOption();
                            uncompressedOption.frameRate = AviRateBox;
                            aviRecorder.AVIOpen(aviFilename, uncompressedOption);
                            break;

                        case AviType.Mjpg:
                            MJPGOption mjpgOption = new MJPGOption();
                            mjpgOption.frameRate = AviRateBox;
                            mjpgOption.quality = QualityBox;
                            aviRecorder.AVIOpen(aviFilename, mjpgOption);
                            break;

                        case AviType.H264:
                            H264Option h264Option = new H264Option();
                            h264Option.frameRate = AviRateBox;
                            h264Option.bitrate = 100000 * QualityBox;
                            h264Option.height = frameHeight;
                            h264Option.width = frameWidth;
                            aviRecorder.AVIOpen(aviFilename, h264Option);
                            break;
                    }

                    while (isRecording || queueForAvi.Count > 0)
                    {
                        if (queueForAvi.Count > 0)
                        {
                            using (var rawImage = queueForAvi.Dequeue())
                                aviRecorder.AVIAppend(rawImage);
                        }
                    }
                    aviRecorder.AVIClose();
                    queueForAvi.Clear();
                }
                catch (Exception e)
                {
                System.Windows.MessageBox.Show("AVI threw exception " + e.Message);
                }
                finally
                {
                    aviRecorder.Dispose();
                }

            }));
            IsAviWriting = false;
        }

        public async void Capture()
        {
            bool wasLive = isLive;
            isLive = false;
            isRecording = false;
            queueForJPGs.Clear(); // so it won't keep adding on
            
            await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(TimerBox);
                while (IsAviWriting || cam.IsStreaming())
                {
                    // wait for cam to stop live
                    // wait for AVI to finish so it won't overload the CPU
                }
            });

            isCapturing = true;
            SetAcqusitionMode(AcquisitionMode.Multi, FramesBox);
            cam.BeginAcquisition();
            ImageSourceThumbnails.Clear();

            await Task.Run(new Action( () =>
            {
                for (int i = 0; i < FramesBox && isCapturing; i++)
                {
                    using (var rawImage = cam.GetNextImage())
                    {
                        // deep copy image so it'll get processed and saved later
                        queueForJPGs.Enqueue(rawImage.Convert(PixelFormatEnums.Mono8));
                        
                        // now put on UI
                        UIDispatcher.Invoke(new Action(() =>
                        {
                            var bitmapImage = ConvertRawToBitmapSource(rawImage);
                            ImageSourceThumbnails.Add(bitmapImage);
                            MainImage = bitmapImage;
                        }));

                    } 
                }

            }));

            isCapturing = false;
            cam.EndAcquisition();

            if (wasLive)
            {
                GoLive();
            }            
        }

        #region Acquisition Mode
        public enum AcquisitionMode { Single, Multi, Continuous }; // made this for kicks
        public void SetAcqusitionMode(AcquisitionMode mode, int numFrames=1)
        {
            // get the handle on the properties (called "nodes")
            INodeMap nodeMap = this.cam.GetNodeMap();
            // Retrieve enumeration node from nodemap
            IEnum iAcquisitionMode = nodeMap.GetNode<IEnum>("AcquisitionMode");

            switch (mode)
            {
                case AcquisitionMode.Continuous:
                    {
                        // Retrieve entry node from enumeration node
                        IEnumEntry iAqContinuous = iAcquisitionMode.GetEntryByName("Continuous");
                        // Set symbolic from entry node as new value for enumeration node (no idea wtf this is necessary)
                        iAcquisitionMode.Value = iAqContinuous.Symbolic;
                        break;
                    }
                case AcquisitionMode.Single:
                    {
                        IEnumEntry iAqSingle = iAcquisitionMode.GetEntryByName("SingleFrame");
                        iAcquisitionMode.Value = iAqSingle.Symbolic;
                        break;
                    }
                case AcquisitionMode.Multi:
                    {
                        IEnumEntry iAqMultiFrame = iAcquisitionMode.GetEntryByName("MultiFrame");
                        iAcquisitionMode.Value = iAqMultiFrame.Symbolic;
                        // set burst
                        IInteger frameCount = nodeMap.GetNode<IInteger>("AcquisitionFrameCount");
                        frameCount.Value = numFrames;
                        break;
                    }

            }
        }
        #endregion

        #region Exposure Time
        public int ExposureBox
        {
            get
            {
                if (cam != null)
                    return (int)cam.ExposureTime.Value;
                return 0;
            }
            set
            {
                SetExposure(value); // when loses focus, sets camera to what user inputed
                NotifyPropertyChanged("ExposureBox"); // update UI box (calls get)
            }
        }
        public void SetExposure(int micros)
        {
            try
            {
                cam.ExposureAuto.Value = ExposureAutoEnums.Off.ToString();
                // set exposure, make sure it's within limits
                if (micros > cam.ExposureTime.Max) // ~30 sec for blackfly
                {
                    cam.ExposureTime.Value = cam.ExposureTime.Max;
                }
                else if (micros < cam.ExposureTime.Min) // ~6 micros for blackfly
                {
                    cam.ExposureTime.Value = cam.ExposureTime.Min;
                }
                else
                {
                    cam.ExposureTime.Value = micros;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Exception thrown in SetExposure(uint micros) method. Exception message: " + e.Message);
            }
        }
        #endregion

        #region Frame Rate

        public void SetFramerate(double Hz)
        {
            try
            {
                // frame rate enable property (otherwise won't let you change it)
                IBool iAqFrameRateEnable = cam.GetNodeMap().GetNode<IBool>("AcquisitionFrameRateEnable");
                iAqFrameRateEnable.Value = true;

                // set frame rate, make sure it's within limits 
                if (Hz > cam.AcquisitionFrameRate.Max) // ~170 fps for blackfly
                {
                    cam.AcquisitionFrameRate.Value = cam.AcquisitionFrameRate.Max;
                }
                else if (Hz < cam.AcquisitionFrameRate.Min)
                {
                    cam.AcquisitionFrameRate.Value = cam.AcquisitionFrameRate.Min;
                }
                else
                {
                    cam.AcquisitionFrameRate.Value = Hz;

                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Exception thrown in SetFramerate(double Hz) method. Exception message: " + e.Message);
            }
        }
        #endregion

        // convert raw image (IManagedImage) to BitmapSource
        private BitmapSource ConvertRawToBitmapSource(IManagedImage rawImage)
        { // convert and copy raw bytes into compatible image type
            
            byte[] bytes = rawImage.ManagedData;// convertedImage.ManagedData;
            System.Windows.Media.PixelFormat format = System.Windows.Media.PixelFormats.Gray8;

            return BitmapSource.Create(
                    (int)rawImage.Width,
                    (int)rawImage.Height,
                    96d,
                    96d,
                    format,
                    null,
                    bytes,
                    ((int)rawImage.Width * format.BitsPerPixel + 7) / 8
                    );

        }


        public bool InitializeCamera()
        { // call only once
            spinnakerSystem = new ManagedSystem();
            // get list of cams plugged in:
            {
                List<IManagedCamera> camList = spinnakerSystem.GetCameras();
                if (0 == camList.Count)
                {
                    System.Windows.MessageBox.Show("No camera connected.");
                    return false;
                }
                else cam = camList[0]; // get the first one
                camList.Clear();
            } // camlist is garbage collected
            cam.Init(); // don't know what this does

            // defaults:
            SetFramerate(30);
            QualityBox = 75; // default quality
            TimerBox = 0;
            //DurationBox = 1;
            FramesBox = 10;
            AviRateBox = 30;

            NotifyPropertyChanged("ExposureBox"); // refresh it w/ cam data
            return true;
        }

        public async void Dispose()
        {
            isLive = false;
            if (cam != null && cam.IsInitialized())
            {
                await Task.Run(() =>
                {
                    while (cam.IsStreaming() || IsAviWriting)
                    {
                        // hold on till cam stops
                    }
                });

                cam?.DeInit();
            }
            spinnakerSystem?.Dispose();
        }

        #region PropertyChanged stuff
        // call this method invokes event to update UI elements which use Binding
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #endregion

    }
}
