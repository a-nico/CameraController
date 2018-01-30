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



        public MainWindow()
        {
            InitializeComponent();
        }

        # region Button Even Handlers
        private void StartCam_Click(object sender, RoutedEventArgs e)
        {

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

        }

        private void ExposureMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExposurePlus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExposurePlus2_Click(object sender, RoutedEventArgs e)
        {

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
        # endregion
    }


    // --------------------------------------------------------------------------------

    internal class CameraControl : INotifyPropertyChanged
    {
        # region Fields
        Dispatcher UIDispatcher;
        private ManagedSystem spinnakerSystem; // spinnaker class
        private IManagedCamera cam; // the blackfly
        public bool liveMode; // live mode or frame capture on UI


        #endregion


        #region Properties

        public int FrameRate
        {
            get
            {
                if (cam != null)
                    //return (uint)(1000 * currentCam.AcquisitionFrameRate.Value) / 1000.0;
                    return (int)cam.AcquisitionFrameRate.Value;
                return 0;
            }
            set
            {
                SetFramerate(value); // when loses focus, sets camera to what user inputed
                NotifyPropertyChanged("FrameRate"); // update UI box (calls get)
            }
        }

        private ImageSource mainImage;
        public ImageSource MainImage
        {
            get { return mainImage; }
            set
            {
                mainImage = value;
                NotifyPropertyChanged("UIimage");
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

        private uint frameCount;
        public uint FrameCount
        {
            get
            {
                return frameCount < 2 ? 2 : frameCount; // can't be <2 for MultiFrame mode
            }
            set
            {
                frameCount = value;
                NotifyPropertyChanged("FrameCount");
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
            SetAcqusitionMode(AcquisitionMode.Continuous);
            cam.BeginAcquisition();
            liveMode = true;

            Task.Run(new Action(() =>
            {
                SetAcqusitionMode(AcquisitionMode.Multi, FrameCount);
                cam.BeginAcquisition();
                IManagedAVIRecorder aviRec = new ManagedAVIRecorder();

                while (liveMode)
                {
                    using (IManagedImage rawFrame = cam.GetNextImage())
                    {
                        // flash it on screen
                        // updating UI image has to be done on UI thread. Use Dispatcher
                        UIDispatcher.Invoke(new Action(() =>
                        {
                            MainImage = ConvertRawToBitmapSource(rawFrame);
                        }));
                        // append to AVI
                        // if (isRecording)
                        aviRec.AVIAppend(rawFrame);
                    }
                }
                cam.EndAcquisition(); // done with camera
            })); // end Task
        }


        #region Acquisition Mode
        public enum AcquisitionMode { Single, Multi, Continuous }; // made this for kicks
        public void SetAcqusitionMode(AcquisitionMode mode, uint numFrames=1)
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
        public uint ExposureTime
        {
            get
            {
                if (cam != null)
                    return (uint)cam.ExposureTime.Value;
                return 0;
            }
            set
            {
                SetExposure(value); // when loses focus, sets camera to what user inputed
                NotifyPropertyChanged("ExposureTime"); // update UI box (calls get)
            }
        }
        public void SetExposure(uint micros)
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
