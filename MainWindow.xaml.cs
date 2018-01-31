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

namespace CameraController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        CameraControl camControl;
        bool camStarted;
        float bigModifier = 1.6f;
        float smallModifier = 1.2f;

        public MainWindow()
        {
            InitializeComponent();
 
        }

        #region Button Even Handlers

        private void Record_Click(object sender, RoutedEventArgs e)
        {

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
            } 
            else
            {
                if (camControl.liveMode)
                {
                    camControl.liveMode = false;
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

        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {

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
            camControl.ExposureBox = (int)(camControl.ExposureBox / smallModifier);

        }

        private void ExposurePlus_Click(object sender, RoutedEventArgs e)
        {
            camControl.ExposureBox = (int)(camControl.ExposureBox * smallModifier);

        }

        private void ExposurePlus2_Click(object sender, RoutedEventArgs e)
        {
            camControl.ExposureBox = (int)(camControl.ExposureBox * bigModifier);

        }

        private void DurationMinus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DurationPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DurationPlus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FramesMinus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FramesMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FramesPlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FramesPlus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RateMinus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RateMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RatePlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RatePlus2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TimerMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TimerPlus_Click(object sender, RoutedEventArgs e)
        {

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
        public bool liveMode; // live mode or frame capture on UI
        public bool isRecording; // if direct to AVI is running
        private Queue<IManagedImage> imageQueue; // waiting to get appended to AVI
        public enum AviType
        {
            Uncompressed,
            Mjpg,
            H264
        }
        int frameWidth = 1280;
        int frameHeight = 1024;

        #endregion


        #region Properties

        public int RateBox
        {
            get
            {
                if (cam != null)
                    return (int)cam.AcquisitionFrameRate.Value;
                return 0;
            }
            set
            {
                SetFramerate(value); // when loses focus, sets camera to what user inputed
                NotifyPropertyChanged("FrameRate"); // update UI box (calls get)
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

        private int _FramesBox;
        public int FramesBox
        {
            get
            {
                return _FramesBox < 2 ? 2 : _FramesBox; // can't be <2 for MultiFrame mode
            }
            set
            {
                _FramesBox = value;
                NotifyPropertyChanged("FrameCount");
            }
        }

        private int _DurationBox;
        public int DurationBox
        {
            get
            {
                return _DurationBox;
            }
            set
            {
                _DurationBox = value;
                FramesBox = (int) (RateBox * value);
                NotifyPropertyChanged("DurationBox");
            }
        }

        #endregion

        // constructor
        public CameraControl(Dispatcher UIDispatcher)
        {
            this.UIDispatcher = UIDispatcher;


            // create collecton on UI thread so I won't have any problems with scope BS
            UIDispatcher?.BeginInvoke(new Action(() =>
            {
                ImageSourceThumbnails = new ObservableCollection<ImageSource>();
            }));

        }



        public void GoLive()
        {
            liveMode = true;

            Task.Run(new Action(() =>
            {
                SetAcqusitionMode(AcquisitionMode.Continuous);
                cam.BeginAcquisition();
                

                while (liveMode)
                {
                    using (IManagedImage rawImage = cam.GetNextImage())
                    {
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

        public void startRecording(string aviFilename, float frameRate=30, AviType fileType=AviType.Uncompressed)
        {
            isRecording = true;

            // start eating up the queue and appending to AVI
            Task.Run(new Action(() =>
            {
                using (IManagedAVIRecorder aviRecorder = new ManagedAVIRecorder())
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
                            h264Option.bitrate = 1000000;
                            h264Option.height = Convert.ToInt32(frameHeight);
                            h264Option.width = Convert.ToInt32(frameWidth);
                            aviRecorder.AVIOpen(aviFilename, h264Option);
                            break;
                    }

                    while (isRecording || imageQueue.Count > 0)
                    {
                        if (imageQueue.Count > 0)
                            aviRecorder.AVIAppend(imageQueue.Dequeue());
                    }
                aviRecorder.AVIClose();
                }

            }));
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
            } // camlist is garbage collected
            cam.Init(); // don't know what this does
            return true;
        }

        public void Dispose()
        {
            liveMode = false;
            cam.DeInit();
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
