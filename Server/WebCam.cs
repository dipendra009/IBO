using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebCam_Capture;
using System.Windows.Forms;
using System.Drawing;

namespace CaputureVideo
{
    class WebCam
    {
        public WebCamCapture webcam;
        private PictureBox _FrameImage;
        private int frameNumber = 25;
        
        Image img  ;
        
        public void initializeWebCam(ref PictureBox imageControl)
        {
            webcam = new WebCamCapture();
            webcam.FrameNumber = ((ulong)(0ul));
            webcam.TimeToCapture_milliseconds = frameNumber;
            webcam.ImageCaptured += new WebCamCapture.WebCamEventHandler(webcam_ImageCaptured);
            _FrameImage = imageControl;
            //bmpList = new List<Bitmap>();
            //imgList = new List<Image>();
            img = null;
            
            
        }
        void webcam_ImageCaptured(object source, WebcamEventArgs e)
        {
           img = null;
            img = e.WebCamImage;
            try
            {
                lock (this)
                {
                    while (e.WebCamImage.Equals(null)) ;
                    _FrameImage.Image = e.WebCamImage;
                    img = e.WebCamImage;
                    
                  //  Bitmap bmp = new Bitmap(e.WebCamImage);
                    //bmpList.Add(bmp);
              //      imgList.Add(img);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inside webcam_ImageCaptured " + ex.ToString());
            }
           
            
            
        }
        
        public void Start()
        {
            try
            {
                webcam.TimeToCapture_milliseconds = frameNumber;
                webcam.Start(this.webcam.FrameNumber);
                 
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Video Device Found!!");
            }
        }
        public void Stop()
        {
            webcam.Stop();
            _FrameImage.Image = null;
            
        }
        public void Continue()
        {
            webcam.TimeToCapture_milliseconds = frameNumber;
            webcam.Start(this.webcam.FrameNumber);
        }
        public void ResolutionSetting()
        {
            webcam.Config();
        }
        public void AdvanceSetting()
        {
            webcam.Config2();
        }
        
        //public List<Image> getImageList()
        //{
        //    return imgList;
        //}
        public Image getCapturedFrame()
        {
       while (img == null) ;
            return img;
        }
    }
}
