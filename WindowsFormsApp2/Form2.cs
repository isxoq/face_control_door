using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form2 : Form
    {
        MCvFont font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade faceDetected;
        Image<Bgr, Byte> Frame;
        Capture camera;
        Image<Gray, byte> result;
        Image<Gray, byte> TrainedFace = null;
        Image<Gray, byte> grayFace = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> Users = new List<string>();
        int Count, NumLables, t;
        string name, names = null;

        bool ochiq = false;
        SerialPort port;

        double vaqt1;
        
        private void PortWrite(string message)
        {
            try
            {
                port.Write(message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
          if(port != null)
            {
                port.Close();
            }
           
            Form1 form1 = new Form1();
            form1.Show();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           

        }

        private void button2_Click(object sender, EventArgs e)
        {
          
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            comboBox.Items.Clear();
            foreach (string comport in ports)
            {
                comboBox.Items.Add(comport);
            }
        }

        private void close_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public Form2()
        {
            InitializeComponent();
            faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");
            try
            {
                string Labelsinf = File.ReadAllText(Application.StartupPath + "/Faces/Faces.txt");
                string[] Labels = Labelsinf.Split(',');
                //The first Label before, willl be the number of faces saved.
                NumLables = Convert.ToInt16(Labels[0]);
                Count = NumLables;
                string FacesLoad;
                for (int i = 1; i < NumLables + 1; i++)
                {
                    FacesLoad = "face" + i + ".bmp";
                    trainingImages.Add(new Image<Gray, byte>(Application.StartupPath + $"/Faces/{FacesLoad}"));
                    labels.Add(Labels[i]);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bazada ma'lumot  mavjud emas!");
            }
        }

        private void scan_Click(object sender, EventArgs e)
        {

            try
            {
                if (port == null)
                {
                    port = new SerialPort(comboBox.Text, 9600);//Set your board COM
                    port.Open();
                  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ulanishda xatolik! Port band emasligini tekshiring! Qurilma ulanganmi?");
                return;
            }

            scan.BackColor = Color.Red;
            scan.Text = "Skanerlanmoqda...";

            camera = new Capture();
            camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);

        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            if(ochiq)
            {
                double t = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds - vaqt1;
                
                if (t>10)
                {
                    ochiq = false;
                    PortWrite("0");
                 
                    return;
                }
                label6.Text = (10 - t).ToString() + " sekund...";
                open.Visible = true;
                close.Visible = false;
            }
            else
            {
                open.Visible = false;
                close.Visible = true;
                label6.Text = "Yopiq";
            }
            Users.Add("");
            Frame = camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            grayFace = Frame.Convert<Gray, Byte>();
            MCvAvgComp[][] facesDetectedNow = grayFace.DetectHaarCascade(faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach (MCvAvgComp f in facesDetectedNow[0])
            {
                result = Frame.Copy(f.rect).Convert<Gray, Byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                Frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCriterias = new MCvTermCriteria(Count, 0.0001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages.ToArray(), labels.ToArray(), 1500, ref termCriterias);
                    name = recognizer.Recognize(result);                  
                    if(name!="")
                    {
                        if(!ochiq)
                        {
                            ochiq = true;
                            vaqt1 = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                            PortWrite("1");                            
                          
                        }
                    }
                    Frame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }
                  Users.Add("");
            }
            cameraBox.Image = Frame;
            names = "";
            Users.Clear();
        }
    }
}
