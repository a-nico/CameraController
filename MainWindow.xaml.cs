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

    internal class CameraControl : INotifyPropertyChanged
    {
        Dispatcher UIDispatcher;

        private ImageSource _MainImage;
        public ImageSource MainImage
        {
            get { return _MainImage; }
            set
            {
                _MainImage = value;
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


        public CameraControl(Dispatcher UIDispatcher)
        {
            this.UIDispatcher = UIDispatcher;

            // create collecton on UI thread so I won't have any problems with scope BS
            UIDispatcher?.BeginInvoke(new Action(() =>
            {
                ImageSourceThumbnails = new ObservableCollection<ImageSource>();
            }));

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
