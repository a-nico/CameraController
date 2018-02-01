using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SpinnakerNET;
using SpinnakerNET.GenApi;
using System.Windows.Forms;

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
        public string AviPath { get; set; }
        private int _AviRateBox = 30;
        public int AviRateBox
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
        CameraControl.AviType aviFormat;
        public MainWindow()
        {
            InitializeComponent();
            AviPathView.DataContext = this;
            AviPath = "C:\\Captures"; // default path
            AviRateBoxView.DataContext = this;
            AviRateBox = 30; // default fps
            aviFormat = CameraControl.AviType.H264; // default format
            
        }

        

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            camControl?.StartRecording(AviPath, AviRateBox, aviFormat);
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
                NotifyPropertyChanged("AviPath");
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

        #region Other Button Even Handlers
        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            camControl.isRecording = false;
        }

        private void Capture_Click(object sender, RoutedEventArgs e)
        {

        }

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

        private void OnClose(object sender, System.ComponentModel.CancelEventArgs e)
        {
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
        private Queue<IManagedImage> imageQueue; // waiting to get appended to AVI
        public enum AviType
        {
            Uncompressed,
            Mjpg,
            H264
        }
        int frameWidth;
        int frameHeight;
        const int DEFAULT_FRAME_RATE = 30;
        public bool isAviDone;
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

        // gets called when "Capture" UI button is clicked
        private ObservableCollection<ImageSource> _imageSourceThumbnails;
        public ObservableCollection<ImageSource> ImageSourceThumbnails
        {   //"observable collection" automatically notifies UI when changed
            get
            {
                return _imageSourceThumbnails;
            }

            set
            {
                _imageSourceThumbnails = value;
                NotifyPropertyChanged("ImageSourceFrames");
            }
        }

        private int _FramesBox = 5;
        public int FramesBox
        {
            get
            {
                return _FramesBox < 2 ? 2 : _FramesBox; // can't be <2 for MultiFrame mode
            }
            set
            {
                _FramesBox = value;
                _DurationBox = (value / RateBox);
                NotifyPropertyChanged("FramesBox");
                NotifyPropertyChanged("DurationBox");
            }
        }

        private float _DurationBox = 5;
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

        private int _TimerBox = 0;
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
            imageQueue = new Queue<IManagedImage>();

            // create collecton on UI thread so I won't have any problems with scope BS
            UIDispatcher?.BeginInvoke(new Action(() =>
            {
                ImageSourceThumbnails = new ObservableCollection<ImageSource>();
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
                            imageQueue.Enqueue(rawImage.Convert(PixelFormatEnums.Mono8));
                        }

                    }
                }
                cam.EndAcquisition(); // done with camera
            })); // end Task
        }

        
        public async void StartRecording(string aviFileDirectory, float frameRate, AviType fileType)
        {
            isAviDone = false;
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
                            uncompressedOption.frameRate = frameRate;
                            aviRecorder.AVIOpen(aviFilename, uncompressedOption);
                            break;

                        case AviType.Mjpg:
                            MJPGOption mjpgOption = new MJPGOption();
                            mjpgOption.frameRate = frameRate;
                            mjpgOption.quality = 75;
                            aviRecorder.AVIOpen(aviFilename, mjpgOption);
                            break;

                        case AviType.H264:
                            H264Option h264Option = new H264Option();
                            h264Option.frameRate = frameRate;
                            h264Option.bitrate = 10000000;
                            h264Option.height = frameHeight;
                            h264Option.width = frameWidth;
                            aviRecorder.AVIOpen(aviFilename, h264Option);
                            break;
                    }

                    while (isRecording || imageQueue.Count > 0)
                    {
                        if (imageQueue.Count > 0)
                        {
                            using (var rawImage = imageQueue.Dequeue())
                                aviRecorder.AVIAppend(rawImage);
                        }
                    }
                    aviRecorder.AVIClose();
                    imageQueue.Clear();
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
            isAviDone = true;
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
        {
            // convert and copy raw bytes into compatible image type
            using (IManagedImage convertedImage = rawImage.Convert(PixelFormatEnums.Mono8))
            {
                byte[] bytes = convertedImage.ManagedData;

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
            
            SetFramerate(DEFAULT_FRAME_RATE);
            NotifyPropertyChanged("ExposureBox");
            return true;
        }

        public async void Dispose()
        {
            isLive = false;
            if (cam != null && cam.IsInitialized())
            {
                await Task.Run(() =>
                {
                    while (cam.IsStreaming() || !isAviDone)
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
