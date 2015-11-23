using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;
using System.Collections;

namespace Client
{
    public partial class ClientFormThread : Form
    {
        Socket socClient;
         const int FRAMENUMBER = 15;
        byte[] byteImg;
        byte[] byData;
        Boolean onConnect;
        Boolean onReceive;
        Image[] images;
        //ImageFrame[] imageFrames;
        FrameData[] framesData;
        ImageSetting imageSetting;
        char[] states;
        int currentFrameNumber;

        WaitCallback callBackReceive;
        WaitCallback callBackProcess;
        WaitCallback callBackDisplay;

        
        public ClientFormThread()
        {
            InitializeComponent();
            
        }
        private void ClientForm_Load(object sender, EventArgs e)
        {
        
            txtIPAddress.Text = getLocalIp();
            onReceive = false;


            onConnect = false;
            images = new Image[FRAMENUMBER];
            //imageFrames = new ImageFrame[FRAMENUMBER];
            
            framesData = new FrameData[FRAMENUMBER];
            imageSetting = new ImageSetting();
            states = new char[FRAMENUMBER];


            currentFrameNumber = 0;
            callBackReceive = new WaitCallback(receiveImage);
            callBackProcess = new WaitCallback(reProcessImage);
            callBackDisplay = new WaitCallback(displayImage);


        }

        public string getLocalIp()
        {
            String hostIp = "";
            String hostname = Dns.GetHostName();
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
            return hostIp;

        }

        private void cmdConnect_Click(object sender, EventArgs e)
        {

            try
            {
                socClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                String ipSelected = txtIPAddress.Text;
                String portSelected = txtPort.Text;
                int alPort = Convert.ToInt16(portSelected);

                IPAddress remoteIPAddress = IPAddress.Parse(ipSelected);
                IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, alPort);
                socClient.Connect(remoteEndPoint);
                //socClient.ReceiveBufferSize = 1024;
                //socClient.SendBufferSize = 1024;
           
                MessageBox.Show("Connection has been Established");
                onConnect = true;
            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex.ToString());
                MessageBox.Show("Error Connecting to Server!\nPlease try again Later");
            } 
        }

        private void cmdClose_Click(object sender, EventArgs e)
        {
            socClient.Close();
            onReceive = false;
            MessageBox.Show("Connection to the server has been Closed!!!!!");
        }

        
        private void cmdReceiveData_Click(object sender, EventArgs e)
        {
            onReceive = true;
            
            try
            {
                byData = System.Text.Encoding.ASCII.GetBytes("READY");
                sendData(byData);
                ThreadPool.QueueUserWorkItem(callBackReceive,"");
                ThreadPool.QueueUserWorkItem(callBackProcess,"");
                ThreadPool.QueueUserWorkItem(callBackDisplay,"");
                //Console.ReadLine();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in receiving"+ex.ToString());
            }


            
        }
        int sze;

        /*private void cmdImageSize_Click(object sender, EventArgs e)
        {
            
            try
            {
                byte[] buffer = new byte[1024];
                int iRx = socClient.Receive(buffer);
                char[] chars = new char[iRx];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                String szData = new String(chars);
                sze = Convert.ToInt32(szData);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

      */

       
        private void receiveImage(Object obj)
        {
            int time = 0;
            int FrameNumber;
            int currentFrame;
            char state = 'd';

            byte special = 0;

            int recSize = 0;
            byte[] buffer = new byte[1024];

            int prevFrame = 0;

            Boolean receive = false;

            Image image;
            FrameData frameData;

            int ticks = Environment.TickCount;

            try
            {
                socClient.ReceiveBufferSize = 1024;
                socClient.SendBufferSize = 1024;
                while (true)
                {
                    recSize = 0;
                    lock (this)
                    {

                        {

                            FrameNumber = FRAMENUMBER;
                            receive = onReceive;
                            //currentFrame = (currentFrameNumber + FrameNumber) % FrameNumber;

                        }
                        ticks = Environment.TickCount;

                        try
                        {
                            imageSetting = recImageSetting();


         //                   MessageBox.Show(imageSetting.Width.ToString() + "X" + imageSetting.Height.ToString() + "X" + imageSetting.BlockSize.ToString() + "X" + imageSetting.ColorBits.ToString() + "ccccc");
                        }
                        catch(Exception ex) 
                        {
                            MessageBox.Show("Error in receiving imageSetting... in CCCCCCCCCCCCCCCCCCCCCCC");
                        }

                        while (receive)
                        {

                            lock (this)
                            {

                                currentFrame = (currentFrameNumber + FrameNumber) % FrameNumber;
                            //    state = states[currentFrame];

                            }
                             
                            prevFrame = (currentFrame - 1 + FrameNumber) % FrameNumber;
 
                               try
                               {
                                   special = recSpecial();
                             //      MessageBox.Show("ccccccccccccccSpecial" + special.ToString());
                               }
                               catch (Exception ex)
                               {
                                   MessageBox.Show("Error on receiving special" + ex.ToString());
                               }

                               if (special == 0)
                               {
                                   state = 'N';
                               }
                               else if (special == 1)
                               {
                                   state = 'P';
                               }
                               else if (special == 2)
                               {
                                   state = 'M';
                               }

                               try
                               {
                                   recSize = recieveSize();
                               //    MessageBox.Show("Receive video size + " + recSize.ToString());

                               }
                               catch (FormatException ex)
                               {
                                   Console.WriteLine("NumberFormateException");
                               }

                               

                            List<byte> totalByte = new List<byte>();
                            int recQue = recSize / 1024;
                            int recRem = recSize % 1024;
                            int total = recQue * 1024 + recRem;
                            Console.WriteLine("frame size in kb = ...." + recQue + "remainder = " + recRem + " and total = "+total);
                            int count = 0;
                            while(count!=recQue)
                            {
                                try
                                {
                                    buffer = new byte[1024];
                                    buffer = recData();
                                    foreach (byte b in buffer)
                                    {
                                        totalByte.Add(b);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.ToString());
                                }
                                count++;
                            } 
                            //remaining picture ko lagi........
                            buffer = new byte[recRem];
                            int iRx = socClient.Receive(buffer);
                            foreach (byte b in buffer)
                            {
                                totalByte.Add(b);
                                
                            }

                            MessageBox.Show("ccccccccccccafter receiving actual data" + recSize.ToString() + " " + totalByte[0] + " " + totalByte[recSize - 1]);
                            Console.WriteLine("  Before calling sending bact to  endccccccccccccccccccccccccc....");
                            try
                            {
                                       sendData(BitConverter.GetBytes(special));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("sending end error in cccccccccccccccccccccccccc" + ex.ToString());
                            }
                                

                            Console.WriteLine("ccccccccccccccccccccccccccccccccccccccccccc  before conversion to image...");

                            if (state == 'N')
                            {
                                lock (this)
                                {
                                    images[currentFrame] = images[prevFrame];
                                    //imageFrames[currentFrame] = imageFrames[prevFrame];
                                    framesData[currentFrame] = null;
                                    states[currentFrame] = 'N';
                                }
                            }
                            else if (state == 'P')
                            {
                                frameData = new FrameData(totalByte.ToArray(),  imageSetting);
                                lock (this)
                                {
                                    
                                    framesData[currentFrame] = frameData;
                                    states[currentFrame] = 'P';
                                }
                            
                            }
                            else if (state == 'M')
                            {
                                image = byteArrayToImage(totalByte.ToArray());
                                lock (this)
                                {

                                    images[currentFrame] = image;
                                    framesData[currentFrame] = null;
                                    states[currentFrame] = 'M';
                                }
                            
                            
                            }

                             
                            //Console.WriteLine("ccccccccccccccccccccccccccccccccccccccccccc  After conversion to image....");
                            
                            //Console.WriteLine("ccccccccccccccccccccccccccccccccccccccccccc after pictureBox1 called....");
                            //byData = System.Text.Encoding.ASCII.GetBytes("SHSHSHSH");
                            //Console.WriteLine("sending  shshssh to server.....");
                            //sendData(byData);
                            //Console.WriteLine("after sending shshshshs  to server........");

                            
                            lock (this)
                            {
                             //   images[currentFrame] = img;
                               // states[currentFrame] = 'p';
                                currentFrameNumber = (currentFrameNumber + 1) % FrameNumber;
                            }
                            time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                            if (time > 0)
                                Thread.Sleep(time);
                            //else
                            //    Console.WriteLine("cccccccccccccccccccccccccccccccccccccccccccno sleeping..." + (-time).ToString());
                            ticks = Environment.TickCount;
                        }


                    }
                }
            }
            catch (SocketException ex)
            {
                MessageBox.Show("receiveImage() function \n"+"server has been closed..");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in receive Image!" + ex.ToString());

            }
            
        }

        private void reProcessImage(Object obj)
        {
            int ticks = 0;
            int time = 0;

            ticks = Environment.TickCount;
            Image image;
            FrameData frameData;
            ImageFrame imageFrame;
            Image prevImage;

            int FrameNumber;
            int currentFrame;
            int prevFrame;
            char state;
            int synch = 0;
            Boolean receive = false;
            try
            {
                while (true)
                {
                    lock (this)
                    {

                        FrameNumber = FRAMENUMBER;
                        receive = onReceive;
                    }
                    Thread.Sleep(1000 / FrameNumber * 2);
                    try
                    {

                        while (receive)
                        {
                            lock (this)
                            {
                                currentFrame = (currentFrameNumber - 1 + FrameNumber) % FrameNumber;
                                state = states[currentFrame];
                            }
                            prevFrame = (currentFrame - 2 + FrameNumber) % FrameNumber;
                             
                           // if (state!= '')
                            //{
                                //Thread.Sleep(1000 / FrameNumber);

                                //Console.WriteLine("cccccccccccccccccccccccccccccccccccccccccccTHis is inside processing.not synchronized.." + currentFrame + state + synch);
                                //  time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                                //  if (time > 0)
                                //     Thread.Sleep(time);
                                //  synch++;
                                // ticks = Environment.TickCount;
                                //  currentFrame = (currentFrame + 1) % FrameNumber;   
                              //  if (synch < FrameNumber)
                                //    continue;
                            //}
                            synch = 0;

                            if (state == 'P')
                            {
                                lock (this)
                                {
                                    prevImage = images[prevFrame];
                                    frameData = framesData[currentFrame];
                                }

                                image = reProcessFrame(prevImage, frameData, imageSetting);
                                lock (this)
                                {
                                    images[currentFrame] = image;
                                    states[currentFrame] = 'M';
                                }
                            }
                             //Console.WriteLine("THis is inside process..." + currentFrame);
                            time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                            if (time > 0)
                                Thread.Sleep(time);
                            //else
                            //    Console.WriteLine("cccccccccccccccccccccccccccccccccccccccccccno sleeping..." + (-time).ToString());
                            ticks = Environment.TickCount;

                        }
                    }
                    catch (Exception ex)
                    {
                     MessageBox.Show("Error in process thread!"+ex.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in processing!"+ex.ToString());
            }

        }

        private void displayImage(Object obj)
        {
            try
            {

                int ticks = 0;
                int time = 0;
                Image image;

                //ticks = Environment.TickCount;

                int FrameNumber;
                int currentFrame;
                
                Boolean receive = false;
                 
                 

                 char state = 'd';
                 int synch = 0;
                 
             
             
            
                while (true)
                {
                    lock (this)
                    {

                        FrameNumber = FRAMENUMBER;
                        receive = onReceive;
                                   //currentFrame = currentFrameNumber-2;
             
                    }
                           ticks = Environment.TickCount;

                           Thread.Sleep(1000 / FrameNumber * 3);
                    try
                    {

                        while (onReceive)
                        {
                            lock (this)
                            {
                                currentFrame = (currentFrameNumber - 2 + FrameNumber) % FrameNumber;
                                state = states[currentFrame];

                            }

                            if (states[currentFrame] != 'M')
                            {
                                //  Thread.Sleep(1000 / FrameNumber);
                               // Console.WriteLine("cccccccccccccccccccccccccccccccccccccccccccTHis is inside displaying..not synchronized.." + currentFrame + state + synch);
                                time = 1000 / FrameNumber - (Environment.TickCount - ticks);

                                if (time > 0)
                                    Thread.Sleep(time);
                                synch++;
                                ticks = Environment.TickCount;
                                //                            currentFrame = (currentFrame + 1) % FrameNumber;   
                                if (synch < FrameNumber)
                                    continue;
                            }
                            synch = 0;

                            if(state == 'M')
                            lock (this)
                            {
                                image = images[currentFrame];

                                pictureBox1.Image = image;     
                            }
                            
                            //currentFrameNumber = (currentFrameNumber + 1) % FrameNumber;
                            time = 1000 / FrameNumber - (Environment.TickCount - ticks);
                          
                           
                            if (time > 0)
                                Thread.Sleep(time);
                            //else
                              //  Console.WriteLine("cccccccccccccccccccccccccccccccccccccccccccno sleeping..." + (-time).ToString());
                            ticks = Environment.TickCount;
                            currentFrame = (currentFrame + 1 + FrameNumber) % FrameNumber;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error in displaying thread...!"+ex.ToString());
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error in displaying...!"+ex.ToString());
            }
        }

         ImageSetting recImageSetting()
        {
            byte[] bytes = recData(16);  
            ImageSetting imageSetting = new ImageSetting(bytes);
            //imageSetting.Width = recSize();
            //imageSetting.Height = recSize();
            //imageSetting.BlockSize = recSize();
            //imageSetting.ColorBits = recSize();

            return imageSetting;
        
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
                MessageBox.Show(ex.ToString());
            }
            return ms.ToArray();
        }

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
                MessageBox.Show(ex.ToString());
            }
            return returnImage;
        }

     /*   ImageFrame extractPixels(Image image, ImageSetting imageSetting)
        {
            ImageFrame imageFrame = new ImageFrame();
            Bitmap source = new Bitmap(image);
            int RPixel;
            int GPixel;
            int BPixel;



            for (int i = 0; i < image.Width; i += imageSetting.BlockSize)

                for (int j = 0; j < image.Height; j += imageSetting.BlockSize)
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
        */
        Image reProcessFrame( Image prevImage, FrameData frameData, ImageSetting imageSetting)
        {

            Image image;
            Bitmap bitmap = new Bitmap(imageSetting.Width,imageSetting.Height);
            Bitmap prevBitmap = new Bitmap(prevImage);
            int RPixel;
            int GPixel;
            int BPixel;

            int columnChangeNum =0;

            int row =0;
            
            int changedBlockNum =0;

            bool rowChanges = false;
            bool blockChange = false;

             

            for (int i = 0; i < imageSetting.Height; i += imageSetting.BlockSize)
            {
                if (frameData.rowChangeBits[i])
                {
                    rowChanges = true;
                    row++;
                }

                for (int j = 0; j < imageSetting.Width; j += imageSetting.BlockSize)
                {
                 
                    columnChangeNum = row * imageSetting.Width/imageSetting.BlockSize + j;
                    blockChange = frameData.columnChangeBits[columnChangeNum];
                     for (int k = i; k < imageSetting.BlockSize; k++)
                        for (int l = j; l < imageSetting.BlockSize; l++)
                        {
                            if (rowChanges && blockChange)
                            {

                                RPixel = frameData.RPixels[changedBlockNum];
                                GPixel = frameData.GPixels[changedBlockNum];
                                BPixel = frameData.BPixels[changedBlockNum]; ;

                                changedBlockNum++;
                            }
                            else 
                            {
                                RPixel = prevBitmap.GetPixel(k, l).R;
                                GPixel = prevBitmap.GetPixel(k, l).G;
                                BPixel = prevBitmap.GetPixel(k, l).B;

                            }
                             Color newcol = Color.FromArgb(RPixel, GPixel, BPixel);
                             bitmap.SetPixel(k, l, newcol);

                    }
                     
                     
                }
                
            }

            image = (Image)bitmap;

            return image;
        
               
            
        }


       /* public byte toByte(int num)
        {

            return BitConverter.GetBytes(num)[0];
        }

*/
        public void recCondition(String condition)
        {
            String recSyn = null;
            do
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int iRx = socClient.Receive(buffer);
                    char[] chars = new char[iRx];
                    System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                    int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                    recSyn = new String(chars);
                    //Console.WriteLine("Received Condition recSync "+recSyn);
                }
                catch(SocketException ex)
                {
                    MessageBox.Show("Server has been terminated....");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }while (!recSyn.Equals(condition));
           
            
            //Console.WriteLine("End of receive Condition : /...");
        }
        public String recString()
        {
            String recSyn = null;
            byte[] buffer = new byte[1024];
            //Console.WriteLine("inside recString()= ");
                try
                {
                    
                    int iRx = socClient.Receive(buffer);
                    char[] chars = new char[iRx];
                    System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                    int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                    recSyn = new String(chars);
                    Console.WriteLine("Receive String inside recString()= " + recSyn);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                return recSyn;
        }
        public int recieveSize()
        {
            String recSyn;
            int recsz = 0;
            byte[] buffer = new byte[6];
            //Console.WriteLine("inside recString()= ");
            try
            {

                int iRx = socClient.Receive(buffer);
                char[] chars = new char[iRx];
                System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
                int charLen = d.GetChars(buffer, 0, iRx, chars, 0);
                //MessageBox.Show("inside recieveSize" + buffer[0] + " " + buffer[1] + " " + buffer[2] + " " + buffer[3] + " " + buffer[4] + " " + buffer[5]);
                recSyn = new String(chars);
                recsz = Int32.Parse(recSyn);
              //  MessageBox.Show("ccccccccccccccparsing cStringReceive Size inside recSize()= " + recSyn);
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("inside recSize()"+ex.ToString());
            }
            return recsz;
        }

        public byte recSpecial()
        {
            String recSyn = null;
             
            byte[] buffer = new byte[2];
            byte special = 0;

            //Console.WriteLine("inside recString()= ");
            try
            {

                int iRx = socClient.Receive(buffer);
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

        public byte[] recData()
        {
            //String recSyn = null;

           

            byte[] buffer = new byte[1024];
            try
            {
                int iRx = socClient.Receive(buffer);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Server has been closed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inside recData() function in clientside  " + ex.ToString());
            }
                //MessageBox.Show("Received data + "+recSyn);
                return buffer;
        }

        public byte[] recData(int length)
        {
            //String recSyn = null;



            byte[] buffer = new byte[length];
            try
            {
                int iRx = socClient.Receive(buffer);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("Server has been closed");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Inside recData() function in clientside  " + ex.ToString());
            }
            //MessageBox.Show("Received data + "+recSyn);
            return buffer;
        }


        public void sendData(byte[] byData)
        {
            try
            {
                socClient.Send(byData);
            }
            catch (SocketException ex)
            {
                MessageBox.Show("sendData() function \n"+"server has been closed..");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

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

            RPixels = new List<byte>();
            GPixels = new List<byte>();
            BPixels = new List<byte>();
            int rowLen = imgSetting.Height / imgSetting.BlockSize;
            int rowBytelen = rowLen / 8;
            if (rowLen % 8 != 0)
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


            int colNo = imgSetting.Width / imgSetting.BlockSize;
            int columnByteLen = columnChange * colNo / 8;
            if (columnChange * colNo % 8 != 0)
                columnByteLen++;
            List<byte> columnData = new List<byte>();
            for (int j = 0; j < columnByteLen; j++)
            {
                columnData.Add(frameData[pos]);
                pos++;
            }
            int frameDataLen = frameData.Length;
            int colorSize = (frameDataLen - pos) / 3;

            if ((frameDataLen - pos) % 3 != 0)
                Console.WriteLine("not divisible bye 3" + (frameDataLen - pos) % 3);

            BitArray columnBits = new BitArray(columnData.ToArray());
            columnChangeBits = new BitArray(columnChange * colNo);
            for (int i = 0; i < (columnChange * colNo); i++)
            {
                columnChangeBits[i] = columnBits[i];
            }

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
            return data.ToArray();
        }

        public byte[] ToByteArray(BitArray bits)
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
