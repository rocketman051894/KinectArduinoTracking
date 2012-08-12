/***********************************************************/
/*                                                         */
/*                     Code by                             */
/*                   Kevin Aiosa                           */
/*                                                         */
/***********************************************************/

/* This is the main code for getting and sending the X and Y data values to the Arduino from the Kinect */

using System;
using System.Collections.Generic;
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
using Microsoft.Kinect;
using System.IO.Ports;
using System.IO;
using System.Media;
using System.Net.Sockets;
using System.Net;

namespace kinectTest
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

        bool closing = false;
        const int skeletonCount = 6;
        Skeleton[] allSkeletons = new Skeleton[skeletonCount];
        int yInvert, lastValY, xInvert, lastValX;
        int cnt = 0;

        Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.105"), 8888);

        System.Media.SoundPlayer player = new System.Media.SoundPlayer();

        byte[] packet;
        String data;

        private SerialPort comPort = new SerialPort();

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {   // Sets the COM ports and baud rates. Also does a "Hello World" print to the console and init the Kinect Sensor. 
            comPort.PortName = "COM4";
            comPort.BaudRate = 115200;
            kinectSensorChooser1.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser1_KinectSensorChanged);
            Console.WriteLine("Hello World");
            System.Diagnostics.Debug.WriteLine("Hello World");
            player.SoundLocation = "C:\\Users\\Kevin\\Documents\\Visual Studio 2010\\Projects\\kinectTest\\kinectTest\\turret_active_5.wav";
            player.Play();

            //string text = "Hello";
          //  byte[] packet = System.Text.ASCIIEncoding.ASCII.GetBytes(text);     

          //  sock.SendTo(packet, endPoint);
          //  Console.WriteLine("sent");
        }

        

        void kinectSensorChooser1_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {   // Detects if a new Kinect sensor is plugged in
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;
            newSensor.ColorStream.Enable();
            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();
            try
            {
                newSensor.Start();
                newSensor.AllFramesReady +=new EventHandler<AllFramesReadyEventArgs>(newSensor_AllFramesReady);
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser1.AppConflictOccurred();
            }

        }

        void newSensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            if (closing)
            {
                return;
            }

            Skeleton first = getFirstSkeleton(e);

            if (first == null)
            {
                return;
            }

            getCameraPoint(first, e);
        }

        void getCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
        {   // Gets the data from the Kinect and sends it to the sendData method
            using (DepthImageFrame depth = e.OpenDepthImageFrame())
            {
                if (depth == null || kinectSensorChooser1.Kinect == null)
                {
                    return;
                }

                DepthImagePoint headDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);
                DepthImagePoint leftDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.HandLeft].Position);
                DepthImagePoint rightDepthPoint = depth.MapFromSkeletonPoint(first.Joints[JointType.Head].Position);

                ColorImagePoint headColorPoint = depth.MapToColorImagePoint(headDepthPoint.X, headDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint leftColorPoint = depth.MapToColorImagePoint(leftDepthPoint.X, leftDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);
                ColorImagePoint rightColorPoint = depth.MapToColorImagePoint(rightDepthPoint.X, rightDepthPoint.Y, ColorImageFormat.RgbResolution640x480Fps30);

                CameraPosition(leftHand, leftColorPoint);
                CameraPosition(rightHand, rightColorPoint);

                sendData(rightColorPoint.X, rightColorPoint.Y);
            }
        }

        private void CameraPosition(FrameworkElement element, ColorImagePoint point)
        {
            Canvas.SetLeft(element, point.X - element.Width / 2);
            Canvas.SetTop(element, point.Y - element.Height / 2);
        }

        Skeleton getFirstSkeleton(AllFramesReadyEventArgs e)
        {   // Gets Skeleton
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                Skeleton first = (from s in allSkeletons where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                return first;
            }
        }

        void StopKinect(KinectSensor kinect)
        {
            if (kinect != null)
            {
                kinect.Stop();
                kinect.AudioSource.Stop();
            }
        }

        private void sendData(int x, int y)
        {   // Maps and sends the data to the Arduino
            y = (y / 6) + 68;
            x = (x / 7) ;

            if (cnt == 0)
            {
                player.SoundLocation = "C:\\Users\\Kevin\\Documents\\Visual Studio 2010\\Projects\\kinectTest\\kinectTest\\turret_active_7.wav";
                player.Play();
                Console.WriteLine("There you are");
                yInvert = y;
                lastValY = y;
                xInvert = x;
                lastValX = x;
            }

            if (y > lastValY)
            {
                yInvert = yInvert - (y - lastValY);                 //Have program send all data in 1 packet then have arduino set servos accordingly using posistions in the buffer array[1-3][4-6]..etc
            }

            if (y < lastValY)
            {
                yInvert = yInvert + (lastValY - y);
            }

            if (x > lastValX)
            {
                xInvert = xInvert - (x - lastValX);
            }

            if (x < lastValX)
            {
                xInvert = xInvert + (lastValX - x);
            }

            if (xInvert < 10)
            {
                data += "x00" + xInvert.ToString();
            }

            if (xInvert < 100 && xInvert > 10)
            {
                data += "x0" + xInvert.ToString();               
            }

            if (xInvert > 100)
            {
                data += "x" + xInvert.ToString();              
            }

            if (yInvert < 10)
            {
                data += "y00" + yInvert.ToString();                
            }

            if (yInvert < 100 && yInvert > 10)
            {
                data += "y0" + yInvert.ToString();             
            }

            if (yInvert > 100)
            {
                data += "y" + yInvert.ToString();              
            }

            packet = System.Text.ASCIIEncoding.ASCII.GetBytes(data);
            Console.WriteLine(x + " - " + xInvert + "," + y + " - " + yInvert);

          //  System.Threading.Thread.Sleep(50);

            if (comPort.IsOpen == true)
            {
                comPort.Write(yInvert.ToString() + "a");
                comPort.WriteLine("");
                //  System.Threading.Thread.Sleep(5);

                comPort.WriteLine(xInvert.ToString() + "b");
                comPort.WriteLine("");
                //  System.Threading.Thread.Sleep(15);

            //    comPort.WriteLine(y.ToString() + "c");
            //    comPort.WriteLine("");

            //    comPort.WriteLine(xInvert.ToString() + "d");
            //    comPort.WriteLine("");

                Console.WriteLine(x + " - " + xInvert + "," + y + " - " + yInvert);
            }
            else
            {
                comPort.Open();//opening port when the program starts
            }

            data = "";
            lastValY = y;
            lastValX = x;
            cnt = 1;
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {   // Closes the window
            player.SoundLocation = "C:\\Users\\Kevin\\Documents\\Visual Studio 2010\\Projects\\kinectTest\\kinectTest\\turret_disabled_6.wav";
            player.Play();
            System.Threading.Thread.Sleep(1000);
            StopKinect(kinectSensorChooser1.Kinect);
        }

        private void kinectColorViewer1_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
