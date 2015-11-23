using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using CaputureVideo;
using System.Threading;
using Server.ImageHelper;
using System.IO;
using System.Collections;
using System.Drawing.Imaging;


namespace Server
{
    public partial class ServerFrm : Form
    {
        //Server Info
        int sendFrames = 0;

        int portNo = 8221;
        String hostname = "";
        String hostIp = "";

        const int FRAMENUMBER = 15;
        byte[] byData;
        Boolean onCapture;
        Boolean onSending;
        ImageFrame[] imageFrames;
        Image[] images;
        ImageSetting imageSetting;
        FrameData[] framesData;
        char[] states;
        int currentFrameNumber = 0;
        double totalSentData = 0;
        double totalData = 0;
        int resHeight;
        int resWidth;

        //socket operation
        public AsyncCallback pfnWorkerCallBack;
        public Socket socketListener;
        public Socket socWorker;

        //capturing video
        WebCam webcam;

        //Thread
        WaitCallback callBackCapture;
        WaitCallback callBackProcess;
        WaitCallback callBackSend;

        
        public ServerFrm()
        {
            InitializeComponent();
        }

        private void ServerFrm_Load(object sender, EventArgs e)
        {
            webcam = new WebCam();
            webcam.initializeWebCam(ref pictureBox1);
            localMachineInfo();
            txtPortNo.Text = portNo.ToString();
            txtHostname.Text = hostname;
            txtHostIP.Text = hostIp;
            sendBtn.Enabled = false;
            stopServerBtn.Enabled = false;
            onCapture = false;
            onSending = false;
            images = new Image[FRAMENUMBER];
            states = new char[FRAMENUMBER];
            imageSetting = new ImageSetting();
            imageFrames = new ImageFrame[FRAMENUMBER];
            framesData = new FrameData[FRAMENUMBER];

            resHeight = 640;
            resWidth = 720;

            currentFrameNumber = 0;
            callBackCapture = new WaitCallback(captureImage);
            callBackProcess = new WaitCallback(processImage);
            callBackSend = new WaitCallback(sendImage);
        }

        private void ServerFrm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBox.Show("Form is Closing");
            MessageBox.Show(totalSentData + "/" + totalData + " : " + totalSentData / totalData);
        }

       
        public void localMachineInfo()
        {
            hostname = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] localIPs = ipEntry.AddressList;

            foreach (IPAddress _IPAddress in ipEntry.AddressList)
            {
                // InterNetwork indicates that an IP version 4 address is expected
                // when a Socket connects to an endpoint
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    hostIp = _IPAddress.ToString();
                }
            }
        }

        private void WaitForData(Socket socWorker)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }
                CSocketPacket theSocPkt = new CSocketPacket();
                theSocPkt.thisSocket = socWorker;
                socWorker.BeginReceive(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length, SocketFlags.None, pfnWorkerCallBack, theSocPkt);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public class CSocketPacket
        {
            public System.Net.Sockets.Socket thisSocket;
            public byte[] dataBuffer = new byte[1];
        }
         
        public void OnDataReceived(IAsyncResult asyn)
        {
            try
            {
                CSocketPacket theSocketId = (CSocketPacket)asyn.AsyncState;
                int iRx = 0;
                iRx = theSocketId.thisSocket.EndReceive(asyn);
                char[] chars = new char[iRx + 1];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(theSocketId.dataBuffer, 0, iRx, chars, 0);
                String szData = new String(chars);
                //txtDataRx.Text = txtDataRx.Text + szData;
                
                WaitForData(socWorker);

            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\nOnDataReceived: Socket has been closed\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void strtServerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, portNo);
                socketListener.Bind(ipLocal);
                socketListener.Listen(100);
                socketListener.BeginAccept(new AsyncCallback(OnClientConnect), null);
                MessageBox.Show("Server has been Started!!");
                strtServerBtn.Enabled = false;
                stopServerBtn.Enabled = true;
                sendBtn.Enabled = true;
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
     
        }

        private void stopServerBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!strtServerBtn.Enabled)
                {
                    socketListener.Close(0);
                    if (isConnected())
                    {
                        socWorker.Close(0);
                    }
                    strtServerBtn.Enabled = true;
                    sendBtn.Enabled = false;
                    stopServerBtn.Enabled = false;
                    MessageBox.Show("Server has been closed....");
                    onCapture = false;
                    onSending = false;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Server cannot be closed at the moment....." + ex.ToString());
            }
            
        }

        public void OnClientConnect(IAsyncResult asyn)
        {
            try
            {
                socWorker = socketListener.EndAccept(asyn);
                socWorker.ReceiveBufferSize = 1024;
                socWorker.SendBufferSize = 1024;
                
            }
            catch (ObjectDisposedException)
            {
                System.Diagnostics.Debugger.Log(0, "1", "\n OnClientConnection: Socket has been closed\n");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void strtCaptureBtn_Click(object sender, EventArgs e)
        {
            try
            {
                try
                {
                    webcam.Start();
                    webcam.Continue();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Video capturing Device couldnot be started!");
                }
            
                imageSetting.ColorBits = 8;
                imageSetting.BlockSize = 4;
              //  MessageBox.Show(resWidth + "X" + resHeight);

                webcam.webcam.CaptureHeight = resHeight;
                imageSetting.Height = webcam.webcam.CaptureHeight;// 240;//webcam.getCapturedFrame().Height;
                webcam.webcam.CaptureWidth = resWidth;
                imageSetting.Width = webcam.webcam.CaptureWidth;// = resWidth; // 320;// webcam.getCapturedFrame().Width;
               // MessageBox.Show(imageSetting.Width.ToString() + "X" + imageSetting.Height.ToString() + "X" + imageSetting.BlockSize.ToString() + "X" + imageSetting.ColorBits.ToString() + "start capturee...ssssssssssssssss");
                        
                //MessageBox.Show("Before onCapture");
                 onCapture = true;

                 //MessageBox.Show("Before callcapture");
                ThreadPool.QueueUserWorkItem(callBackCapture,"");

            //    MessageBox.Show("ssssssssssssssssssss");
            }
            catch (Exception ex)
            {
                MessageBox.Show("No Video capturing Device found!");
            }
            
        }

        private void continueBtn_Click(object sender, EventArgs e)
        {
            try
            {
                webcam.Continue();
                webcam.Start();
                onCapture = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not continue capturing!");
            }
           
            
            
        }

       
        private void camSettingBtn_Click(object sender, EventArgs e)
        {
            webcam.Start();
            webcam.AdvanceSetting();
        }

        private void resolutionSettingBtn_Click(object sender, EventArgs e)
        {
            webcam.ResolutionSetting();
        }

        private void stpCaptureBtn_Click(object sender, EventArgs e)
        {

            try
            {
                webcam.Stop();
                onCapture = false;

                MessageBox.Show(totalSentData + "/" + totalData + " : " + totalSentData / totalData+" SendFrames "+sendFrames) ;
        
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in stoping capturing!");
            }
           
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
           // if (isConnected())
           // {
                try
                {
                    onSending = true;
                    try
                    {
                        Console.WriteLine("Before receiving READY in server side");
                        //recData("READY");
                        Console.WriteLine("After receiving READY in server side");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("error on receiving sync data" + ex.ToString());
                    }
                    ThreadPool.QueueUserWorkItem(callBackProcess, "");
                    ThreadPool.QueueUserWorkItem(callBackSend, "");
                    //Console.ReadLine();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error in sending");
                }

           //// }
           //// else
           // {
           //     MessageBox.Show("There is no client Connected...");
           // }
        }

        private void captureImage(Object obj)
        {
            
            int ticks = 0;
            int time = 0;

            int FrameNumber;
            int currentFrame;
            char state = 'd';
            Boolean capture = false;
            Boolean send = false;
            int synch = 0;
            Image imagCapture;
            ImageFrame imageFrame = new ImageFrame();
            ImageSetting imageSett = new ImageSetting();
            Bitmap bitmap;
            try
            {

                while (true)

                {
                    //Console.WriteLine("Hello capturing");
                    lock (this)
                    {
                        FrameNumber = FRAMENUMBER;
                        currentFrame = currentFrameNumber;
                        capture = onCapture;
                        imageSett = imageSetting;
                    }
                    ticks = Environment.TickCount;

                    //Thread.Sleep(1000);
                    while (capture)
                    {
                        if(send == false)
                        lock(this)
                        {
                        send = onSending;     
                        }
                        try
                        {
                          //  Bitmap bitmap;
                            lock (this)
                            {

                                imagCapture = pictureBox1.Image;
                            //    bitmap = new Bitmap(pictureBox1.Image);
                         
                            }
                            if (imagCapture == null)
                            {
                      //          MessageBox.Show("imagCapture is null");
                                continue;
                            }
                             lock(this)
                            {
                                currentFrame = currentFrameNumber;
                                states[currentFrame] = 'p';         
                            }
                         
                           
                            int width = imageSett.Width;
                            int height = imageSett.Height;

                            try
                            {

                                bitmap = new Bitmap(imagCapture, width, height);
                                imageFrame = extractPixels(bitmap, imageSett);


                            }
                            catch (Exception ex)
                            {
                                //MessageBox.Show("Error in bitmap" + ex.ToString());
                                continue;
                            }
                             

                            lock (this)
                            {
                                images[currentFrame] = imagCapture;
                                imageFrames[currentFrame] = imageFrame;
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error in capturing...thread"+ex.ToString());
            
                        }

                        //Increment the current frame of program
                        lock (this)
                        {
                                currentFrameNumber = (currentFrameNumber + 1) % FrameNumber;
          
                        }
                        time = 1000 / FrameNumber - (Environment.TickCount - ticks);   // k ho yo..............

                        if (time > 0)
                        {
                            Thread.Sleep(time);
                            //Console.WriteLine("Thread Sleep");
                        }
                        //else
                        //    Console.WriteLine("no sleeping..." + (-time).ToString());
                        ticks = Environment.TickCount;
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in capture Image!"+ex.ToString());
              }

        }

        private void processImage(Object obj)
        {
            int ticks = 0;
            int time = 0;

            ticks = Environment.TickCount;
            //Console.WriteLine("Time Elapsed +++++++"+ticks.ToString());
            //Image image;
            
            int FrameNumber;
            int currentFrame;
            int prevFrame;
            char state = 'd';
            Boolean capture = false;
            Boolean send = false;
            int synch = 0;
            ImageFrame imageFrame = new ImageFrame();
            ImageFrame prevImageFrame = new ImageFrame();
            FrameData frameData = new FrameData();
            try
            {
            while (true)
            {
                lock (this)
                {

                    FrameNumber = FRAMENUMBER;
                    capture = onCapture;
          //          currentFrame = (currentFrameNumber - 1 + FrameNumber) % FrameNumber;    
                    send = onSending;
                }

               // Thread.Sleep(1000);
                Thread.Sleep(1000 / FrameNumber * 2);
                try
                {
                    while (send && capture)
                {
                    lock (this)
                    {
                        currentFrame = (currentFrameNumber - 1 + FrameNumber) % FrameNumber;
                        state = states[currentFrame];
                    }
                    prevFrame = (currentFrame - 1 + FrameNumber) % FrameNumber;
                        
                    if (state != 'p')
                    {
                        //Thread.Sleep(1000 / FrameNumber);
                        //Console.WriteLine("THis is inside processing.not synchronized.." + currentFrame+state+synch);
                        //time = 1000 / FrameNumber - (Environment.TickCount - ticks);
                        //if (time > 0)
                        //Thread.Sleep(time);
                        //synch++;
                        //ticks = Environment.TickCount;
                        //currentFrame = (currentFrame + 1) % FrameNumber;   
                        //if(synch < FrameNumber)
                        continue;
                    }
                    synch = 0;

                    lock (this)
                    {
                        imageFrame = imageFrames[currentFrame];
                        prevImageFrame = imageFrames[prevFrame];
                        states[currentFrame] = 's';
                   
                    }
                    lock (this)
                    {
                        frameData = processFrames(imageFrame, prevImageFrame, imageSetting);
                    }
   
                    lock (this)
                    {
                        framesData[currentFrame] = frameData; 
                    }

                    //currentFrame = (currentFrame + 1) % FrameNumber;   
                    //Console.WriteLine("THis is inside process..." + currentFrame);
                    time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                    if (time > 0)
                        Thread.Sleep(time);
                    //else
                    //    Console.WriteLine("no sleeping..." + (-time).ToString());
                    ticks = Environment.TickCount;
                    
                }
                }
                catch(Exception ex)
                {
                     MessageBox.Show("Error in process thread!"+ex.ToString());
                }
                 }
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error in processing!"+ex.ToString());
                }
           
        }

        private void sendImage(Object obj)
        {
            //Console.WriteLine("Inside SendImage............");
            try
            {
                int time = 0;
                Image image;                
                int FrameNumber;
                int currentFrame = 0;
                char state = 'd';
                byte[] buffer = new byte[1024];
                Boolean capture = false;
                Boolean send = false;
                int sendSize = 0;
                bool first = true;
                byte[] imageBytes = null;
                byte[] frameBytes = null;
                
                FrameData frameData = new FrameData() ;
                
                byte[] sendingData;
                int synch = 0;
                int size = 0;
                int ticks = Environment.TickCount;
                byte special = 0;

                while (true)
                {
                    
                    lock (this)
                    {
                        FrameNumber = FRAMENUMBER;
                        capture = onCapture;
                        send = onSending;
                    }
                    //Thread.Sleep(1000);
                    Thread.Sleep(1000 / FrameNumber * 3);
                    try
                    {
                        Console.WriteLine("before sending settings......." + sendSize.ToString());
                        //MessageBox.Show(imageSetting.Width.ToString() + "X" + imageSetting.Height.ToString() + "X" + imageSetting.BlockSize.ToString() + "X" + imageSetting.ColorBits.ToString() + "ssssssssssssssss");
                        sendingData = imageSetting.toByte();
                        //sendData(sendingData);
                        
                        }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error on sending setting" + ex.ToString());
                    }
                            



                    try
                    {

                        //Console.WriteLine("State of onSending and onCapturing=" + send + capture);
                        while (send && capture)
                        {
                            lock (this)
                            {
                                currentFrame = (currentFrameNumber - 2 + FrameNumber) % FrameNumber;
                                state = states[currentFrame];
                            }
                            //Console.WriteLine("state = " + state.ToString());

                            if (state != 's')
                            {
                                //Console.WriteLine("THis is inside sending..not synchronized.." + currentFrame + state + synch);
                                time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                                if (time > 0)
                                    Thread.Sleep(time);
                                synch++;
                                ticks = Environment.TickCount;
                                //currentFrame = (currentFrame + 1) % FrameNumber;   
                                if (synch < FrameNumber)
                                    continue;
                            }
                            synch = 0;
                            lock (this)
                            {
                                image = images[currentFrame];
                                frameData = framesData[currentFrame];
                               
                            }

                            //Testing frameData begins

                            int row = frameData.rowChangeBits.Length;
                            int colm = frameData.columnChangeBits.Length;
                            int col = frameData.RPixels.Count;
                            if (row > 0)
                            Console.WriteLine("Before convertions of frameData: row: "+row+" "+frameData.rowChangeBits.Get(0)+ " "+frameData.rowChangeBits.Get(row - 1));
                            if (colm > 0)
                            Console.WriteLine(    " col "+colm+frameData.columnChangeBits.Get(0)+ frameData.columnChangeBits.Get(colm - 1)
                                + " rp: "+col + " "+frameData.RPixels[0]+ " "+frameData.RPixels[col-1]);
                            frameData = new FrameData(frameData.toBytes(), imageSetting);

                             row = frameData.rowChangeBits.Length;
                            colm = frameData.columnChangeBits.Length;
                             col = frameData.RPixels.Count;
                            if(row> 0)
                            Console.WriteLine("After convertions of frameData: row: "+row+" "+frameData.rowChangeBits.Get(0)+ " "+frameData.rowChangeBits.Get(row - 1));
                            if(colm >0)
                            Console.WriteLine(  " col "+colm+frameData.columnChangeBits.Get(0)+ frameData.columnChangeBits.Get(colm - 1)
                                + " rp: "+col + " "+frameData.RPixels[0]+ " "+frameData.RPixels[col-1]);
                            
                            //Testing frameData complete

                            if (image == null)
                            {
                                //  MessageBox.Show("image null in sending SSSSSSSSS");
                                continue;
                            }
                            else
                            {
                                imageBytes = imageToByteArray(image);

                            }
                            if (frameData == null)
                            {
                                //MessageBox.Show("ssssssssssssssssssSpecial" + BitConverter.GetBytes(special).Length);

                                //continue;
                                continue;

                            }
                            else
                            {
                                frameBytes = frameData.toBytes();

                            }
                            
                            
                            totalData += imageBytes.Length;

                            ImageSetting imageSett;
                            lock (this)
                            {
                                imageSett = imageSetting;
                            }
                            try
                            {

                                if ((frameData.columnChangeBits.Length == 0)&& !first)
                                {
                                    special= 0;
                                    //sendData(BitConverter.GetBytes(special));
                                    //MessageBox.Show("ssssssssssssssssssSpecial" + BitConverter.GetBytes(special).Length);
                                     
                               
                                    totalSentData++;
                                }

                                else
                                {
                                    if ((frameBytes.Length < imageBytes.Length)&& !first)
                                    {
                                        special = 1;
                                        //sendData(BitConverter.GetBytes(special));
                                        sendingData = frameBytes;
                                        //MessageBox.Show("ssssssssssssssssssSpecial" + BitConverter.GetBytes(special).Length);
                                   
                                         //Testing

                                        frameData = new FrameData(sendingData.ToArray(), imageSett);

                    //                    MessageBox.Show("sended size: " + sendingData.Length + "frameData size " + frameData.toBytes().Length +

                    //                    "imageSett: " + imageSett.Height + imageSett.Width + imageSett.BlockSize + imageSett.ColorBits +
                    //                      "LLLLLLLLLLLLLLLLLLLlll" + "frame Data : rowchange" + frameData.rowChangeBits.Length + "frame Data : colchange" + frameData.columnChangeBits.Length +
                    //"frame Data : RP" + frameData.RPixels.Count + "frame Data : GP" + frameData.GPixels.Count +
                    //    "frame Data :BP" + frameData.BPixels.Count);
                
                                        Image prevImage;
                                        int prevN = (currentFrame - 1 + FrameNumber) % FrameNumber;

                                        lock (this)
                                        {
                                            prevImage = images[prevN];

                                        }

                                        image = reProcessFrame(prevImage, frameData, imageSett);

                                        pictureBox2.Image = image;

                                    }

                                    else
                                    {
                                        special  = 2;
                                        sendSize = imageBytes.Length;
                                        sendingData = imageBytes;
                                        //MessageBox.Show("Special sizessssssssssssssssssSpecial" + BitConverter.GetBytes(special).Length);
                               
                                        //sendData(BitConverter.GetBytes(special));
                                    
                                        //Testing
                                        image = byteArrayToImage(sendingData.ToArray());
                                        pictureBox2.Image = image;

                                    }

                                    totalSentData += sendSize;

                                    try
                                    {
                                        Console.WriteLine("before sending size.......size = " + sendSize.ToString());
                                        byData = System.Text.Encoding.ASCII.GetBytes(sendSize.ToString());
                                        //MessageBox.Show("sssssssssssReceive video size + " + sendSize.ToString() + " " + byData.Length + " " + byData[0] + " " + byData[1] + " " + byData[2] + " " + byData[3] + " " + byData[4] + " " + byData[5] );

                                        //sendData(byData);
                                        Console.WriteLine("after sending size.......");
                                        sendFrames++;

                                    }


                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Error on sending size" + ex.ToString());
                                    }

                                    try
                                    {
                                        Console.WriteLine("before sending actual data :");
                                     //   MessageBox.Show("Before sending actual data" + sendSize.ToString()+" "+sendingData[0]+" "+sendingData[sendSize-1]);
                                        //sendData(sendingData);
                                      //  MessageBox.Show(" after sending actual data :");
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Error on sending video in sending actual data" + ex.ToString());
                                    }
                                }
                            }
                            catch (Exception ex)
                               {
                                    Console.WriteLine("sending special error in sssssssssssssss"+ex.ToString());
                                }
                                
                                // Console.WriteLine("ccccccccccccccccccccccccccccccccccccccccccc  after cxalling recsize...." + recSize);

                            //byte[] spec = new byte[1];

                            //spec[0] = special; 

                                Console.WriteLine("  Before calling sending bact to server end....");
                                try
                                {
                            //        //sendData(BitConverter.GetBytes(special));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("sending end error in sssssssssssssss"+ex.ToString());
                                }
                               // try
                               // {
                               //     byte sp;
                               // //do
                               // //{
                               // //    //sp = recSpecial();
                               // //}while(sp != special);
                               // //}
                               // catch (Exception ex)
                               // {
                               //     Console.WriteLine("receiving end error in sssssssssssssss"+ex.ToString());
                               // }
                               //}
                        //Testing portion begins.............................................       
                        //        ImageSetting imageSett;
                        //        lock (this)
                        //        {
                        //            imageSett = imageSetting;
                        //        }
                        //if(special == 0)
                        //{
                        
                        //}
                        //else if(special == 1)
                        //{
                        // frameData = new FrameData(sendingData.ToArray(), imageSett);   
                        //Image prevImage;
                        //  int  prevN = (currentFrame -1 +FrameNumber)%FrameNumber;
                                 
                        //    lock(this)
                        //    {
                        //        prevImage = images[prevN];

                        //    }
                         
                        //        image = reProcessFrame(prevImage, frameData, imageSett);
                                
                        //     pictureBox2.Image = image;
                        //}
                        //else if(special == 2)
                        //{
                        //    image = byteArrayToImage(sendingData.ToArray());
                        //    pictureBox2.Image = image;

                        //}
                        //else
                        //{
                        //    MessageBox.Show("IIIIIIIIIIII");
                        //}

                        //Testing portion ends.........................................
                                lock(this)
                                {
                                    states[currentFrame] = 'c';
                                }
                                first = false;
                                time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                                if (time > 0)
                                Thread.Sleep(time);
                                //else
                                //    Console.WriteLine("no sleeping..." + (-time).ToString());
                                ticks = Environment.TickCount;
                                //currentFrame = (currentFrame + 1 + FrameNumber) % FrameNumber;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error in sending thread...!"+ex.ToString());
                    }
                }

            }
        
            catch (Exception ex)
            {
                Console.WriteLine("Error in sending...!");
            }
        }

        public void recData(byte condition)
        {
            byte recSyn = 99;
            do
            {
                try
                {
                    byte[] buffer = new byte[1];
                    int iRx = socWorker.Receive(buffer);
                    //char[] chars = new char[iRx];
                    //System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                    //int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                    recSyn = buffer[0];

                }
                catch (Exception ex)
                {
                    MessageBox.Show("inside receive byte in server side " + ex.ToString());
                }
            } while (recSyn == condition);

            Console.WriteLine("Received" + condition + " in server : ");

        }
        Image reProcessFrame(Image prevImage, FrameData frameData, ImageSetting imageSetting)
        {
           Image image;
                Bitmap bitmap = new Bitmap(imageSetting.Width, imageSetting.Height);
                Bitmap prevBitmap = new Bitmap(prevImage);
                bitmap = prevBitmap;
                int RPixel;
                int GPixel;
                int BPixel;

                int columnChangeNum = 0;

                int row = -1;

                int changedBlockNum = 0;

                bool rowChanges = false;
                bool blockChange = false;

                int i=0;
                int j=0;
                try
                {
               
                for (   i = 0; i < imageSetting.Height; i += imageSetting.BlockSize)
                {
                    if (frameData.rowChangeBits[i/imageSetting.BlockSize])
                    {
                        rowChanges = true;
                        row++;
                    }
                    else
                        continue;

                    for (  j = 0; j < imageSetting.Width; j += imageSetting.BlockSize)
                    {

                        columnChangeNum = row * imageSetting.Width / imageSetting.BlockSize + j/imageSetting.BlockSize;
                        blockChange = frameData.columnChangeBits[columnChangeNum];
                        if (!blockChange)
                            continue;
                        else
                        {

                            RPixel = frameData.RPixels[changedBlockNum];
                            GPixel = frameData.GPixels[changedBlockNum];
                            BPixel = frameData.BPixels[changedBlockNum]; ;

                            changedBlockNum++;

                            for (int k = i; k < imageSetting.BlockSize; k++)
                                for (int l = j; l < imageSetting.BlockSize; l++)
                                {



                                    Color newcol = Color.FromArgb(RPixel, GPixel, BPixel);
                                    bitmap.SetPixel(k, l, newcol);

                                }
                        }

                    }

                }

                image = (Image)bitmap;

                return image;

            }
            catch (Exception ex)
            {
                MessageBox.Show("repocess frameLLLLLLLLLLLLLLLLLLLlll" + " frame Data : rowchange"+frameData.rowChangeBits.Length+ " frame Data : colchange"+frameData.columnChangeBits.Length+ 
                    "frame Data : RP"+frameData.RPixels.Count+ " frame Data : GP"+frameData.GPixels.Count+" rowchange "+rowChanges 
                    +" i "+i+" j "+j+
                    " frame Data :BP"+frameData.BPixels.Count+" blcok change "+blockChange+" changedBlockNum"+changedBlockNum
                
               +"change col "+columnChangeNum+"ex "+ex.ToString());
            }
            return null;
        }

       

        public void recData(String condition)
        {
            String recSyn = null;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int iRx = socWorker.Receive(buffer);
                    char[] chars = new char[iRx];
                    System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                    int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                    recSyn = new String(chars);
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show("inside receive data in server side "+ex.ToString());
                }
            }while (!recSyn.Equals(condition));
            
            Console.WriteLine("Received" + condition+ " in server : ");
                    
        }
       //This function may be useless
        public void recCondition(String condition)
        {
            String recSyn = null;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int iRx = socWorker.Receive(buffer);
                    char[] chars = new char[iRx];
                    System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                    int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                    recSyn = new String(chars);
                    //Console.WriteLine("Received Condition recSync "+recSyn);
                }
                catch (SocketException ex)
                {
                    MessageBox.Show("Server has been terminated....");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            } while (!recSyn.Equals(condition));


            //Console.WriteLine("End of receive Condition : /...");
        }

        public byte recSpecial()
        {
            String recSyn = null;

            byte[] buffer = new byte[2];
            byte special = 0;

            //Console.WriteLine("inside recString()= ");
            try
            {

                int iRx = socWorker.Receive(buffer);
                //char[] chars = new char[iRx];
                //System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                //int charLen = d.GetChars(buffer, 0, iRx, chars, 0);

                //Console.WriteLine("Receive Special inside recSpecial()= " + recSyn);
                // MessageBox.Show("inside special" + BitConverter.GetBytes('0'));
                //byte[] buff = new byte[2];

                special = buffer[0];
                //MessageBox.Show(special.ToString());
                //Console.WriteLine("Receive size inside recSize()= " +special );

            }
            catch (Exception ex)
            {
                MessageBox.Show(" error inside recSpecial()...CCCCCCCCCC" + ex.ToString());
            }
            return special;
        }

       
        public void sendData(byte[] byData)
        {
            try
            {
                socWorker.Send(byData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }         

            //int loop = 0;
            //while (loop < byData.Length / 1024)
            //{

            //    byte[] buffer = new byte[1024];
            //    for (int i = 0; i < 1024; i++)
            //    {
            //        buffer[i] = byData[loop * 1024 + i];
            //    }
            //    socWorker.Send(buffer);
            //    loop++;
            //}
            ////remaining data
            //int rem = byData.Length % 1024;
            //byte[] rbuffer = new byte[rem];
            //for (int j = 0; j < rem ; j++)
            //{
            //    rbuffer[j] = byData[loop * 1024 + j];
            //}
            //socWorker.Send(rbuffer);
        
        }

        ImageFrame extractPixels(Bitmap source, ImageSetting imageSetting)
        {
            ImageFrame imageFrame = new ImageFrame();
            int width = imageSetting.Width;
            int height = imageSetting.Height;
            //MessageBox.Show("I lll" + image.Height+ width+ height); 
           // Bitmap source = null;//= new Bitmap(width, height, PixelFormat.Format24bppRgb);

            
            
             int RPixel;
            int GPixel;
            int BPixel;
            
            

            for(int i =0; i<imageSetting.Width; i+=imageSetting.BlockSize)

                for (int j = 0; j < imageSetting.Height; j += imageSetting.BlockSize)
                {
                    RPixel = 0;
                    GPixel = 0;
                    BPixel = 0;
                    for (int k = i; k < imageSetting.BlockSize; k++)
                        for (int l = j; l < imageSetting.BlockSize; l++)
                        {
                            RPixel += source.GetPixel(k, l).R;
                            GPixel += source.GetPixel(k, l).G;
                            BPixel += source.GetPixel(k, l).B;
                        }
                    RPixel /= imageSetting.BlockSize;
                    GPixel /= imageSetting.BlockSize;
                    BPixel /= imageSetting.BlockSize;

                    imageFrame.RPixels.Add(toByte(RPixel));
                    imageFrame.GPixels.Add(toByte(GPixel));
                    imageFrame.BPixels.Add(toByte(BPixel));
                    

                }
            return imageFrame;
        }

        FrameData processFrames(ImageFrame imageFrame, ImageFrame prevImageFrame, ImageSetting imageSetting)
        {
            int blockChange = 0;

            FrameData frameData = new FrameData();
            List<bool> rowChangeBits = new List<bool>();
                List<bool> columnChangeBits = new List<bool>();

            for(int i =0;i<imageSetting.Height/imageSetting.BlockSize;i++)
            {
                bool rowChange = false;
                List<bool> columnChange = new List<bool>();
                
                int rowFirstBlock = imageSetting.Height/imageSetting.BlockSize*i;
                
                for (int j = 0; j < imageSetting.Width / imageSetting.BlockSize; j++)
                {
                    bool changeBlock = false;
                    int position = rowFirstBlock + j;

                    if (!imageFrame.RPixels.ElementAt(position).Equals(prevImageFrame.RPixels.ElementAt(position)))
                        changeBlock = true;

                    if (!imageFrame.GPixels.ElementAt(position).Equals(prevImageFrame.GPixels.ElementAt(position)))
                        changeBlock = true;

                    if (!imageFrame.BPixels.ElementAt(position).Equals(prevImageFrame.BPixels.ElementAt(position)))
                        changeBlock = true;
                    if (changeBlock)
                    {
                        blockChange++;
                        rowChange = true;
                        columnChange.Add(true);
                        
                        frameData.RPixels.Add(imageFrame.RPixels.ElementAt(position));
                        frameData.GPixels.Add(imageFrame.GPixels.ElementAt(position));
                        frameData.BPixels.Add(imageFrame.BPixels.ElementAt(position));
                        

                    }
                    else
                    {
                        columnChange.Add(false);
                    }
        
                }
                if (rowChange)
                {
                    rowChangeBits.Add(true);
                    columnChangeBits.AddRange(columnChange);
                }
                else
                {
                    rowChangeBits.Add(false);
                }
                
            }
            frameData.rowChangeBits = new BitArray(rowChangeBits.ToArray());
            frameData.columnChangeBits = new BitArray(columnChangeBits.ToArray());
            //MessageBox.Show("int processImageLLLLLLLLLLLLLLLLLLLlll" + "frame Data : rowchange" + frameData.rowChangeBits.Length + "frame Data : colchange" + frameData.columnChangeBits.Length +
            //       "frame Data : RP" + frameData.RPixels.Count + "frame Data : GP" + frameData.GPixels.Count +
            //           "frame Data :BP" + frameData.BPixels.Count);
                
            return frameData;
        }

       
        public byte toByte(int num)
        {

            return BitConverter.GetBytes(num)[0];
         }

        public byte[] imageToByteArray(Image imageIn)
        {

            MemoryStream ms = new MemoryStream();
                
            try
            {
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Gif);
            }
            catch (Exception ex)
            {
                MessageBox.Show("in image to byete"+ex.ToString());
            }
            return ms.ToArray();
        }

        //This function may have no use
        public Image byteArrayToImage(byte[] byteArrayIn)
        {
            MemoryStream ms = new MemoryStream(byteArrayIn);
            Image returnImage = null;
            try
            {
                returnImage = Image.FromStream(ms);
            }
            catch (Exception ex)
            {
                MessageBox.Show("In byt4e ot image"+ex.ToString());
            }
            return returnImage;
        }

        public bool isConnected()
        {
            bool connected = false;
            try
            {
                connected = socWorker.Connected;
            }
            catch (Exception ex)
            {
                connected = false;
            }
            return connected;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            //bool[] data = new bool[9];
            //data[0] = true;
            //data[1] = true;
            //data[2] = false;
            //data[3] = true;
            //data[4] = true;
            //data[5] = false;
            //data[6] = false;
            //data[7] = true;
            //data[8] = true;

            //FrameData frm = new FrameData();
            //BitArray btarray = new BitArray(data);

            //for (int i = 0; i < btarray.Length; i++)
            //{
            //    Console.WriteLine(btarray[i]);
            //}
            //Console.WriteLine("fsdfs");
            //byte[] databyte = frm.ToByteArray(btarray);

            //BitArray bt = new BitArray(databyte);
            //for (int i = 0; i < bt.Length; i++)
            //{
            //    Console.WriteLine(bt[i]);
            //}

            ImageSetting imgSetting = new ImageSetting(320, 240, 8, 8);
            byte[] imgByte = imgSetting.toByte();
            ImageSetting imgSetting1 = new ImageSetting(imgByte);
            Console.WriteLine("Height : " + imgSetting1.Height + "\nWidth : " + imgSetting1.Width + "\nblock : " + imgSetting1.BlockSize + "\nColorBIts : " + imgSetting1.ColorBits);

            int rowDataNo = imgSetting.Height / imgSetting.BlockSize;
            Console.WriteLine("Row No :" + rowDataNo);

            bool[] rowChangeBits = new bool[rowDataNo];

            for (int i = 0; i < rowDataNo; i++)
            {
                rowChangeBits[i] = true;
            }


            int colNo = imgSetting.Width / imgSetting.BlockSize;
            bool[] colChangeBits = new bool[colNo * rowDataNo];
            int count = 0;
            Console.WriteLine("total column change bits no : " + rowDataNo * colNo);
            for (int j = 0; j < rowDataNo; j++)
            {
                for (int k = 0; k < colNo; k++)
                {
                    colChangeBits[count] = true;
                    count++;
                }
            }

            int totalchange = rowDataNo * colNo;
            List<byte> RPixel = new List<byte>();
            List<byte> GPixel = new List<byte>();
            List<byte> BPixel = new List<byte>();
            char R = 'R';
            char G = 'G';
            char B = 'B';
            for (int i = 0; i < totalchange; i++)
            {
                RPixel.Add((byte)R);
                GPixel.Add((byte)G);
                BPixel.Add((byte)B);
            }

            FrameData frmData = new FrameData(rowChangeBits, colChangeBits, RPixel, GPixel, BPixel);

            byte[] frmByte = frmData.toBytes();

            FrameData frmData1 = new FrameData(frmByte, imgSetting);
            int c = 0;
            Console.WriteLine("Row Change Bits : " + rowChangeBits.Length + "------" + frmData1.rowChangeBits.Length);
            for (int i = 0; i < rowDataNo; i++)
            {
                Console.WriteLine(i + "-" + frmData1.rowChangeBits[i]);
                if (frmData1.rowChangeBits[i])
                    c++;
            }

            count = 0;
            Console.WriteLine("Column Change Bits : " + colChangeBits.Length + "----" + frmData1.columnChangeBits.Length + "true data : " + c);
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < colNo; j++)
                {
                    Console.WriteLine(frmData1.columnChangeBits[count]);
                    count++;
                }
            }

            for (int i = 0; i < colNo * c; i++)
            {
                Console.WriteLine((char)frmData1.RPixels[i]);
                Console.WriteLine((char)frmData1.GPixels[i]);
                Console.WriteLine((char)frmData1.BPixels[i]);

            }
            Console.WriteLine("No of R = " + colNo * c);


           

        }

        
       

        
    }

    class ImageSetting
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int BlockSize { get; set; }
        public int ColorBits { get; set; }

        public ImageSetting()
        { }

        public ImageSetting(int width, int height, int blockSize, int colorBits)
        {
            this.Width = width;
            this.Height = height;
            this.BlockSize = blockSize;
            this.ColorBits = colorBits;
        }
        public ImageSetting(byte[] imgSetting)
        {
            Width = BitConverter.ToInt32(imgSetting, 0);

            Height = BitConverter.ToInt32(imgSetting, 4);

            BlockSize = BitConverter.ToInt32(imgSetting, 8);

            ColorBits = BitConverter.ToInt32(imgSetting, 12);
        }

        public byte[] toByte()
        {
            List<byte> imgByte = new List<byte>();

            imgByte.AddRange(BitConverter.GetBytes(Width));

            imgByte.AddRange(BitConverter.GetBytes(Height));

            imgByte.AddRange(BitConverter.GetBytes(BlockSize));

            imgByte.AddRange(BitConverter.GetBytes(ColorBits));

            return imgByte.ToArray();
        }


    }

    class ImageFrame
    {
        public List<byte> RPixels { get; set; }
        public List<byte> GPixels { get; set; }
        public List<byte> BPixels { get; set; }

        public ImageFrame()
        {
            RPixels = new List<byte>();
            GPixels = new List<byte>();
            BPixels = new List<byte>();

        }

        

    }

    class FrameData
    {
        public BitArray rowChangeBits { get; set; }
        public BitArray columnChangeBits { get; set; }
        public List<byte> RPixels { get; set; }
        public List<byte> GPixels { get; set; }
        public List<byte> BPixels { get; set; }
        
        
        public FrameData()
        {
            rowChangeBits = null;
            columnChangeBits = null;
            RPixels = new List<byte>();
            GPixels = new List<byte>();
            BPixels = new List<byte>();

        }

        public FrameData(bool[] rowChangeBits, bool[] columnChangeBits, List<byte> RPixels, List<byte> GPixels, List<byte> BPixels)
        {
            this.rowChangeBits = new BitArray(rowChangeBits);
            this.columnChangeBits = new BitArray(columnChangeBits);
            this.RPixels = RPixels;
            this.GPixels = GPixels;
            this.BPixels = BPixels;
                 
        }
        public FrameData(byte[] frameData, ImageSetting imgSetting)
        {
            //MessageBox.Show("in cons iamgesett" + imgSetting.Height + imgSetting.Width + imgSetting.BlockSize + imgSetting.ColorBits);

            RPixels = new List<byte>();
            GPixels = new List<byte>();
            BPixels = new List<byte>();
            int rowLen = imgSetting.Height / imgSetting.BlockSize;
            int rowBytelen = rowLen/8;
            if(rowLen%8!=0)
                rowBytelen++;
            int pos = 0;
            List<byte> rowData = new List<byte>();
            for (int i = 0; i < rowBytelen; i++)
            {
                rowData.Add(frameData[i]);
                pos++;
            }
            BitArray rowBits = new BitArray(rowData.ToArray());
            rowChangeBits = new BitArray(rowLen);
            int columnChange = 0;

            for (int i = 0; i < rowLen; i++)
            {
                
                rowChangeBits[i] = rowBits[i];
                if (rowBits[i])
                    columnChange++;

            }

            //MessageBox.Show("frombyte...rowLen " + rowLen+"columnChagen"+columnChange);
            int colNo = imgSetting.Width / imgSetting.BlockSize;
            int columnByteLen = columnChange*colNo / 8;
            if (columnChange*colNo % 8 != 0)
                columnByteLen++;
            List<byte> columnData = new List<byte>();
            for (int j = 0; j < columnByteLen; j++)
            {
                columnData.Add(frameData[pos]);
                pos++;
            }
            int frameDataLen = frameData.Length;
           
            int colorSize = (frameDataLen - pos)/3;
        
            if ((frameDataLen - pos) % 3 != 0)
                Console.WriteLine("not divisible bye 3" + (frameDataLen - pos) % 3);
            
            BitArray columnBits= new BitArray(columnData.ToArray());
            columnChangeBits = new BitArray(columnChange*colNo);
            for (int i = 0; i < (columnChange*colNo); i++)
            {
                columnChangeBits[i] = columnBits[i];
            }
       //     MessageBox.Show("frombyte...columnChageBits " + columnChange + "X" + colNo + "frameDataLength: " + frameDataLen + " pos " + pos + "colorSize " +
           // colorSize);

            for (int i = 0; i < colorSize; i++)
            {
                RPixels.Add(frameData[pos]);
                pos++;
            }
            for (int i = 0; i < colorSize; i++)
            {
                GPixels.Add(frameData[pos]);
                pos++;
            }
            for (int i = 0; i < colorSize; i++)
            {
                BPixels.Add(frameData[pos]);
                pos++;
            }

        }

        public byte[] toBytes()
        {
            List<byte> data = new List<byte>();
            byte[] rowChange = ToByteArray(rowChangeBits);
            data.AddRange(rowChange);
            byte[] columnChange = ToByteArray(columnChangeBits);
            data.AddRange(columnChange);
            data.AddRange(RPixels);
            data.AddRange(GPixels);
            data.AddRange(BPixels);

            //MessageBox.Show("inside to Bytes: size of rowChnage  " + rowChangeBits.Length + " " + rowChange.Length +
            //    "colomn " + columnChangeBits.Length + " " + columnChange.Length + "RP " + RPixels.Count + "GP " + GPixels.Count + "BP " + BPixels.Count+"data"+data.Count);
           return data.ToArray();
        }

        public  byte[] ToByteArray( BitArray bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }



         
    }
}
