//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.DepthBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using OpenCvSharp;
    //using TimingScanner;
    using static TimingScanner.ScannerUtils;
    using static TimingScanner.BendClassifier;
    //using static TimingScanner.TestClass;
    //using static TimingScanner.NewtonInterpolation;

    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : System.Windows.Window, INotifyPropertyChanged  // !!! izmjenjeno sa Window na System.Windows.Window da ne dolazi do preklapanja sa window iz OpenCvSharp biblioteke
    {
        /// <summary>
        /// Map depth range to byte range
        /// </summary>
        private const int MapDepthToByte = 8000 / 256;
        
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;
            
        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap depthBitmap = null;

        /// <summary>
        /// Intermediate storage for frame data converted to color
        /// </summary>
        private byte[] depthPixels = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        ushort[] depthFrameData = null;  //!!! dodato
        Mat testImage = new Mat();  //!!! dodato

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the depth frames
            this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();

            // wire handler for frame arrival
            //this.depthFrameReader.FrameArrived += this.Reader_FrameArrived;  !!! commented because of reading from csv for testing purposes

            // get FrameDescription from DepthFrameSource
            this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // allocate space to put the pixels being received and converted
            this.depthPixels = new byte[this.depthFrameDescription.Width * this.depthFrameDescription.Height];

            // create the bitmap to display
            this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, this.depthFrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);
            //this.depthBitmap = new WriteableBitmap(this.depthFrameDescription.Width, 300, 96.0, 96.0, PixelFormats.Gray8, null);

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;


            //Only for reading one frame from csv file:  !!! ...

            //this.depthFrameData = ReadDepthFrameDataFromCSV(@"C:\Users\Djordje\Documents\TIMING_skener\Primjeri lukova\HE\dordje.csv");
            this.depthFrameData = ReadDepthFrameDataFromCSV(@"C:\Users\Djordje\Documents\TIMING_skener\Primjeri lukova\dordje_nivelisan_pravilan_luk\dordje.csv");
            //this.depthFrameData = ReadDepthFrameDataFromCSV(@"C:\Users\Djordje\Documents\TIMING_skener\Primjeri lukova\isecak1\dordje.csv");
            //this.depthFrameData = ReadDepthFrameDataFromCSV(@"C:\Users\Djordje\Documents\TIMING_skener\Primjeri lukova\ve\dordje.csv");

            ProcessDepthFrameDataFromCsv(this.depthFrameData, this.depthPixels, 0, ushort.MaxValue);
            RenderDepthPixels();

            // ... !!!

            // initialize the components (controls) of the window
            this.InitializeComponent();
        }

        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.depthBitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.depthFrameReader != null)
            {
                // DepthFrameReader is IDisposable
                this.depthFrameReader.Dispose();
                this.depthFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.depthBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.depthBitmap));

                string time = System.DateTime.UtcNow.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Depth-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        encoder.Save(fs);
                    }

                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.SavedScreenshotStatusTextFormat, path);
                }
                catch (IOException)
                {
                    this.StatusText = string.Format(CultureInfo.CurrentCulture, Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }
        }

        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            bool depthFrameProcessed = false;

            using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    // the fastest way to process the body index data is to directly access 
                    // the underlying buffer
                    using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                    {
                        // verify data and write the color data to the display bitmap
                        if (((this.depthFrameDescription.Width * this.depthFrameDescription.Height) == (depthBuffer.Size / this.depthFrameDescription.BytesPerPixel)) &&
                            (this.depthFrameDescription.Width == this.depthBitmap.PixelWidth) && (this.depthFrameDescription.Height == this.depthBitmap.PixelHeight))
                        {
                            // Note: In order to see the full range of depth (including the less reliable far field depth)
                            // we are setting maxDepth to the extreme potential depth threshold
                            ushort maxDepth = ushort.MaxValue;

                            // If you wish to filter by reliable depth distance, uncomment the following line:
                            //// maxDepth = depthFrame.DepthMaxReliableDistance
                            
                            this.ProcessDepthFrameData(depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);
                            depthFrameProcessed = true;
                        }
                    }
                }
            }

            if (depthFrameProcessed)
            {
                this.RenderDepthPixels();
            }
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the DepthFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the depthFrameData pointer.
        /// </summary>
        /// <param name="depthFrameData">Pointer to the DepthFrame image data</param>
        /// <param name="depthFrameDataSize">Size of the DepthFrame image data</param>
        /// <param name="minDepth">The minimum reliable depth value for the frame</param>
        /// <param name="maxDepth">The maximum reliable depth value for the frame</param>
        private unsafe void ProcessDepthFrameData(IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / this.depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                this.depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MapDepthToByte) : 0);
            }
        }

        /// <summary>
        /// Renders color pixels into the writeableBitmap.
        /// </summary>
        private void RenderDepthPixels()
        {
            this.depthBitmap.WritePixels(
                new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                this.depthPixels,
                this.depthBitmap.PixelWidth,
                0);
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }


        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessDepthFrameDataFromCsv(this.depthFrameData, this.depthPixels, 0, ushort.MaxValue);
            RenderDepthPixels();

            MaxDepthInput.IsEnabled = true;
            DepthButton.IsEnabled = true;
            ErodeDilateButton.IsEnabled = false;
            BlackWhiteButton.IsEnabled = false;
            ExtractLargestButton.IsEnabled = false;
            TestButton.IsEnabled = false;

            //ErodeDilateButton.IsEnabled = true;
            //BlackWhiteButton.IsEnabled = true;
            //ExtractLargestButton.IsEnabled = true;
            //CorrectRotationButton.IsEnabled = true;
            //TestButton.IsEnabled = true;
        }

        private void Depth_Button_Click(object sender, RoutedEventArgs e)
        {
            ProcessDepthFrameDataFromCsv(this.depthFrameData, this.depthPixels, 0, Convert.ToUInt16(MaxDepthInput.Text));
            RenderDepthPixels();

            ErodeDilateButton.IsEnabled = true;
            DepthButton.IsEnabled = false;
            MaxDepthInput.IsEnabled = false;
        }

        private void Erode_Dilate_Button_Click(object sender, RoutedEventArgs e)
        {
            this.depthPixels = MatToByte1D(ErodeDilateImage(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels)));
            RenderDepthPixels();

            BlackWhiteButton.IsEnabled = true;
            ErodeDilateButton.IsEnabled = false;
        }

        private void Black_White_Button_Click(object sender, RoutedEventArgs e)
        {
            this.depthPixels = MatToByte1D(ToBlackWhiteImage(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels)));
            RenderDepthPixels();

            //CannyEdgeButton.IsEnabled = true;
            BlackWhiteButton.IsEnabled = false;
            ExtractLargestButton.IsEnabled = true;
        }

        /*private void CannyEdgeButton_Click(object sender, RoutedEventArgs e)
        {
            this.depthPixels = MatToByte1D(CannyEdgeDetection(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels)));
            RenderDepthPixels();

            ExtractLargestButton.IsEnabled = true;
            //CannyEdgeButton.IsEnabled = false;
        }*/

        private void ExtractLargestButton_Click(object sender, RoutedEventArgs e)
        {
            if (ExtractLargestContour(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels), false) is null)
            {
                MessageBox.Show("Pomjerite profil ka sredini kadra!");
            }
            else
            {
                this.testImage = ExtractLargestContour(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels), false);
                ShowImage("Izdvojena kontura profila", this.testImage);
                //ShowImage("Izdvojena kontura profila", ResizeImage(ExtractLargestContour(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels)), this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight));
                this.depthPixels = MatToByte1D(ResizeImage(ExtractLargestContour(Byte1DToMat(this.depthBitmap.PixelHeight, this.depthBitmap.PixelWidth, this.depthPixels), true), this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight));
                RenderDepthPixels();
            }

            ExtractLargestButton.IsEnabled = false;
            CorrectRotationButton.IsEnabled = true;
        }

        private void CorrectRotationButton_Click(object sender, RoutedEventArgs e)
        {
            this.testImage = scaleImageToOnlyContour(correctRotation(this.testImage));
            ShowImage("Ispravljena", this.testImage);

            CorrectRotationButton.IsEnabled = false;
            TestButton.IsEnabled = true;
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //this.testImage = MatRotate(this.testImage, 120);
            //ShowImage("Rotirana", this.testImage);

            //this.testImage = scaleImageToOnlyContour(correctRotation(this.testImage));
            //ShowImage("Ispravljena", this.testImage);

            //this.testImage = scaleImageToOnlyContour(MatRotate(this.testImage, 315));
            //ShowImage("Rotirana za 315", this.testImage);


          
            
            if (detectBend1(this.testImage).Contains("false"))
            {
                if (detectBend2(this.testImage).Contains("false"))
                {
                    if (detectBend4(this.testImage).Contains("false"))
                    {
                        if (detectBend6(this.testImage).Contains("false"))
                        {
                            if (isSectionWithoutFlat(this.testImage).Contains("false"))
                            {
                                MessageBox.Show(detectBend3or5(this.testImage));
                            }
                            else
                            {
                                MessageBox.Show("Class 3.1 detected!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Class 6 detected!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Class 4 detected!");
                    }
                }
                else
                {
                    MessageBox.Show("Class 2 detected!");
                }
            }
            else
            {
                MessageBox.Show("Class 1 detected!");
            }
            


            ///   !!!!
            /*this.testImage = scaleImageToOnlyContour(scaledResize(LeftHalf(this.testImage), 100));
            ShowImage("Dio luka za uzimanje odbiraka", this.testImage);
            int[] arr = getArrayY(this.testImage);
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ", ";
            }
            //MessageBox.Show(str + "\n\n" + arr.Length.ToString());
            //MessageBox.Show(str);

            float[] floatArr = IntToFloatArray(arr);
            float[] arrAverage = AveragePoolingSmooth1D(floatArr, 5);
            arrAverage = AveragePoolingSmooth1D(arrAverage, 3);
            string strA = "";
            for (int i = 0; i < arrAverage.Length; i++)
            {
                strA += arrAverage[i].ToString() + ", ";
            }
            MessageBox.Show(strA);*/


            //MessageBox.Show(isSectionWithoutFlat(this.testImage));
            //MessageBox.Show(detectBend3or5(this.testImage));


            //MessageBox.Show(detectBend1(this.testImage, 0.05f, 0.05f));
            //MessageBox.Show(detectBend2(this.testImage));
            //MessageBox.Show(detectBend5(this.testImage));
            //MessageBox.Show(detectBend6(this.testImage));
            //MessageBox.Show(isSectionWithoutFlat(this.testImage));

            //this.testImage = scaledResize(this.testImage, 400);
            //ShowImage("Skalirane dimenzije - sirina 200", this.testImage);
            //this.testImage = LeftHalf(this.testImage);
            //ShowImage("Lijeva polovina", this.testImage);
            //this.testImage = AdaptHeightToWidth(this.testImage);
            //ShowImage("Prilagodjena visina sirini", this.testImage);

            /*

            this.testImage = scaleImageToOnlyContour(MatRotate(this.testImage, 45));
            ShowImage("Rotirana za 45", this.testImage);

            this.testImage = scaledResize(this.testImage, 200);
            ShowImage("Skalirane dimenzije - sirina 200", this.testImage);

            this.testImage = LeftHalf(this.testImage);
            ShowImage("Lijeva polovina", this.testImage);
            //this.testImage = CannyEdgeDetection(scaleImageToOnlyContour(this.testImage));
            //ShowImage("Test Dilate", this.testImage);

            this.testImage = setEvenSize(this.testImage);
            int[] arr = getArrayY(this.testImage);
            string str = "";
            for (int i = 0; i < arr.Length; i++)
            {
                str += arr[i].ToString() + ", ";
            }
            //MessageBox.Show(str + "\n\n" + arr.Length.ToString());

            float[] floatArr = IntToFloatArray(arr);
            //floatArr = AveragePoolingSmooth1D(floatArr, 7);
            floatArr = AveragePoolingSmooth1D(floatArr, 5);
            floatArr = AveragePoolingSmooth1D(floatArr, 3);
            floatArr = AveragePoolingSmooth1D(floatArr, 5);
            //floatArr = AveragePoolingSmooth1D(floatArr, 3);
            string strA = "";
            for (int i = 0; i < floatArr.Length; i++)
            {
                strA += floatArr[i].ToString() + ", ";
            }

            float[] arrSlope = GetSlopeArray(floatArr);
            arrSlope = AveragePoolingSmooth1D(arrSlope, 3);
            string strS = "";
            for (int i = 0; i < arrSlope.Length; i++)
            {
                strS += arrSlope[i].ToString() + ", ";
            }
            //double angle = Math.Atan(Math.Abs(floatArr[0] - floatArr[1])) * 180.0 / Math.PI;

            //MessageBox.Show(str + "\n\n" + strA + "\n\n" + arr.Length.ToString() + "\n\n" + floatArr.Length.ToString() + "\n\n" + angle.ToString() + "\n\n" + floatArr[57].ToString() + " " + floatArr[58].ToString());

            MessageBox.Show(str + "\n\n" + strA + "\n\n" + strS);
            

            */

            //this.testImage = TestFindCornerPoints(this.testImage);
            //ShowImage("Detektovane tacke", this.testImage);

            /*float[] dArr = GetDerivativeArray(arr, 10, 8);
            string strd = "";
            for (int i = 0; i < dArr.Length; i++)
            {
                strd += dArr[i].ToString() + ", ";
            }
            MessageBox.Show(str + "\n\n" + strd);*/

            TestButton.IsEnabled = false;
        }

    }
}
