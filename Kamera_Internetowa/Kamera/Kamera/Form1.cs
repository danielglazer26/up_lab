using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging.Filters;
using AForge.Vision.Motion;
using Accord.Video.FFMPEG;
using AForge.Controls;

namespace Kamera
{
    public partial class Form1 : Form
    {
        MotionDetector detector;
        float detectionLevel;
        bool isRecording = false;
        FilterInfoCollection videoDevices;
        VideoCaptureDevice camera;
        VideoFileWriter writer;
        int zoomValue = 0;

        public Form1()
        {
            InitializeComponent();
            zoomBar.Maximum = 5;
            zoomBar.Minimum = 0;
            buttonPhoto.Enabled = false;
            buttonSettings.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            zoomBar.Enabled = false;
        }

        private void MyForm_Load(object sender, EventArgs e)
        {
            detector = new MotionDetector(new TwoFramesDifferenceDetector(), new MotionBorderHighlighting());
            // get the collection of video input devices
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            detectionLevel = 0;
            // list these devices in the combobox
            foreach (FilterInfo device in videoDevices)
            {
                comboBoxDevices.Items.Add(device.Name);

            }

            camera = new VideoCaptureDevice();
            if(comboBoxDevices.Items.Count!=0)
                comboBoxDevices.SelectedIndex = 0; // default selected item will be the first device
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            startPreview();
            buttonPhoto.Enabled = true;
            buttonSettings.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = false;
            zoomBar.Enabled = true;

        }

        private void startPreview()
        {
            if (camera.IsRunning)
            {
                camera.Stop();
                pictureBoxOutput.Image = null;
                pictureBoxOutput.Invalidate();
            }
            else
            {
                camera = new VideoCaptureDevice(videoDevices[comboBoxDevices.SelectedIndex].MonikerString);
                videoSourcePlayer1.Visible = false;
                //videoSourcePlayer1.VideoSource = camera;
                //videoSourcePlayer1.Start();
                camera.NewFrame += videoSource_NewFrame;
                camera.Start();
            }
        }

        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            detectionLevel = detector.ProcessFrame(image);

        }
        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            var bitmap = (Bitmap)eventArgs.Frame.Clone();

            //var actualFrame = bitmap.Clone();

            const double maxScale = 5.0;


           // double scale = (double)Math.Pow(maxScale, zoomValue / zoomBar.Maximum);
          //  Size newSize = new Size((int)(pictureBoxOutput.Size.Width * scale), (int)(pictureBoxOutput.Size.Height * scale));
           // Bitmap resizedBitmap = new Bitmap(bitmap, newSize);

          //  this.Invoke(new Action(() => {
          //      pictureBoxOutput.Image = resizedBitmap;
         //   }));

            Bitmap tmpImage1 = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics g = Graphics.FromImage(tmpImage1);

            Rectangle dstRect = new Rectangle(0, 0, tmpImage1.Width, tmpImage1.Height);


            Rectangle srcRect;
            if (zoomValue > 0)
            {

                int width = bitmap.Width / zoomValue;
                int height = bitmap.Height / zoomValue;
                int left = bitmap.Width / 2 - (width / 2);
                int top = bitmap.Height / 2 - (height / 2);

                srcRect = new Rectangle(left, top, width, height);
                pictureBoxOutput.CreateGraphics().DrawImage(bitmap, dstRect, srcRect, GraphicsUnit.Pixel);
            }
            else
                pictureBoxOutput.Image = bitmap;



            // detectionLevel = detector.ProcessFrame(bitmap);


            Bitmap currentFrame = (Bitmap)pictureBoxOutput.Image.Clone();
            if (isRecording)
            {
                try
                {
                   // Bitmap currentFrame = (Bitmap)pictureBoxOutput.Image.Clone();
                    writer.WriteVideoFrame(currentFrame);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }

        }




        private void comboBoxDevices_SelectedIndexChanged(object sender, EventArgs e)
        {

        }


        private void MyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (camera.IsRunning)
            {
                camera.Stop();
            }
        }

        private void CapturePhoto(object sender, EventArgs e)
        {
            if (pictureBoxOutput.Image != null)
            {
                captureImage();
            }
            else
            { MessageBox.Show("null exception"); }
        }

        private void captureImage()
        {
            Bitmap varBmp = new Bitmap(pictureBoxOutput.Image);
            Bitmap current = (Bitmap)varBmp.Clone();
            string filepath = Environment.CurrentDirectory;
            string fileName = System.IO.Path.Combine(filepath, @"name.jpg");
            current.Save(fileName);
            current.Dispose();
        }



        private void button4_Click(object sender, EventArgs e)
        {
            camera.DisplayPropertyPage(Handle);
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
               zoomValue = zoomBar.Value;
             
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (camera.IsRunning)
            {
                try
                {
                    
                    writer = new VideoFileWriter();
                    writer.Open("video.avi", pictureBoxOutput.Image.Width, pictureBoxOutput.Image.Height, 60, VideoCodec.Default);
                    isRecording = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            buttonPhoto.Enabled = false;
            buttonSettings.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = false;
            zoomBar.Enabled = false;
            comboBoxDevices.Enabled = false;
            buttonStart.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            buttonPhoto.Enabled = true;
            buttonSettings.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = true;
            zoomBar.Enabled = true;
            comboBoxDevices.Enabled = true;
            buttonStart.Enabled = true;
            if (camera.IsRunning)
            {
                isRecording = false;
                writer.Close();

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            buttonPhoto.Enabled = false;
            buttonSettings.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = true;
            zoomBar.Enabled = false;
            comboBoxDevices.Enabled = false;
            buttonStart.Enabled = false;
            videoSourcePlayer1.Visible = true;
            videoSourcePlayer1.VideoSource = camera;
            videoSourcePlayer1.Start();
            pictureBoxOutput.Visible = false;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            buttonPhoto.Enabled = true;
            buttonSettings.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = true;
            button4.Enabled = false;
            zoomBar.Enabled = true;
            comboBoxDevices.Enabled = true;
            buttonStart.Enabled = true;
            detectionLevel = 0;
            videoSourcePlayer1.SignalToStop();
            camera.Stop();
            pictureBoxOutput.Visible = true;
            startPreview();
        }

        private void pictureBoxOutput_Click(object sender, EventArgs e)
        {

        }

    
    }
}   

