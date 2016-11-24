using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;

namespace CaseLightingMod
{
    public partial class Form1 : Form
    {
        private SerialPort dataStream;
        private PerformanceCounter cpuCounter;
        Timer cpuTimer = new Timer();
        int numReadings = 20;
        int[] readings = new int[20];
        int readIndex = 0;
        int total = 0;
        int average = 0;
        //String modType = "mod,0";

        public Form1()
        {
            InitializeComponent();
            Load += new EventHandler(connectSerial);
            this.colorWheel1.MouseUp += new MouseEventHandler(this.colorWheel1_MouseUp);
            this.trackBar1.MouseUp += new MouseEventHandler(this.trackBar1_MouseUp);
            this.toggleSwitch1.MouseClick += new MouseEventHandler(this.toggleSwitch1_MouseClick);
            this.toggleSwitch2.MouseClick += new MouseEventHandler(this.toggleSwitch2_MouseClick);
            this.toggleSwitch3.MouseClick += new MouseEventHandler(this.toggleSwitch3_MouseClick);
            this.toggleSwitch4.MouseClick += new MouseEventHandler(this.toggleSwitch4_MouseClick);

            cpuCounter = new PerformanceCounter();
            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            
            cpuTimer.Interval = (500);
            cpuTimer.Tick += new EventHandler(cpu_reader_tick);

        }

        private void toggleSwitch1_MouseClick(object sender, MouseEventArgs e)
        {
            Console.WriteLine("clicked");
            if (toggleSwitch1.Checked == true)
            {
                writeSerial("mod,0");
            }
            else
            {
                if (toggleSwitch2.Checked) { writeSerial("mod,3"); }
                if (toggleSwitch3.Checked) { writeSerial("mod,2"); }
                if (toggleSwitch4.Checked) { writeSerial("mod,4"); }
                else { writeSerial("mod,2"); }

            }
        }

        private void toggleSwitch2_MouseClick(object sender, MouseEventArgs e)
        {
            if (toggleSwitch2.Checked == true)
            {
                writeSerial("mod,2");
            }
            else
            {
                writeSerial("mod,3");
                toggleSwitch3.Checked = false;
                toggleSwitch4.Checked = false;
                cpuTimer.Stop();
            }
        }

        private void toggleSwitch3_MouseClick(object sender, MouseEventArgs e)
        {
            if (toggleSwitch3.Checked == true)
            {
                writeSerial("mod,2");
                cpuTimer.Stop();
            }
            else
            {
                toggleSwitch2.Checked = false;
                toggleSwitch4.Checked = false;
                cpuTimer.Start();
                writeSerial("mod,2");
            }
        }

        private void toggleSwitch4_MouseClick(object sender, MouseEventArgs e)
        {
            if (toggleSwitch4.Checked == true)
            {
                writeSerial("mod,2");
                cpuTimer.Stop();
                toggleSwitch1.Checked = true;
            }
            else
            {
                toggleSwitch2.Checked = false;
                toggleSwitch3.Checked = false;
                cpuTimer.Stop();
                writeSerial("mod,4");
            }
        }

        private void cpu_reader_tick(object sender, EventArgs e)
        {
            float perc = getCurrentCpuFloat();
            //float perc = 70;

            total = total - readings[readIndex];
            readings[readIndex] = (int)Math.Round(perc);
            total = total + readings[readIndex];
            readIndex++;

            if (readIndex >= numReadings)
            {
                readIndex = 0;
            }

            average = total / numReadings;

            int blue = 240 - (int)Math.Round(average * 2.55);
            int green = 240 - (int)Math.Round(average * 2.55);
            Console.WriteLine(average);
            writeSerial("cRGB," + 255 + "," + green + "," + blue);
        }

        private void connectSerial(object sender, System.EventArgs e)
        {
            try
            {
                dataStream = new SerialPort("COM7", 9600);
                dataStream.ReadTimeout = 1000;
                dataStream.DtrEnable = true;
                dataStream.Open();
                Console.WriteLine("connected to device");
                writeSerial("gief");
                string s = readNewLine(2000);
                string[] tempS = s.Split(","[0]);
                colorWheel1.Color = Color.FromArgb(255, int.Parse(tempS[1]), int.Parse(tempS[2]), int.Parse(tempS[3]));
                trackBar1.Value = int.Parse(tempS[4])*100;
                label6.Text = "Speed control: " + int.Parse(tempS[4]);
                switch (int.Parse(tempS[5]))
                {
                    case 0:
                        toggleSwitch1.Checked = false;
                        toggleSwitch2.Checked = false;
                        toggleSwitch3.Checked = false;
                        toggleSwitch4.Checked = false;
                        break;

                    case 2:
                        toggleSwitch1.Checked = true;
                        toggleSwitch2.Checked = false;
                        toggleSwitch3.Checked = false;
                        toggleSwitch4.Checked = false;
                        break;

                    case 3:
                        toggleSwitch1.Checked = true;
                        toggleSwitch2.Checked = true;
                        toggleSwitch3.Checked = false;
                        toggleSwitch4.Checked = false;
                        break;

                    case 4:
                        toggleSwitch1.Checked = true;
                        toggleSwitch2.Checked = false;
                        toggleSwitch3.Checked = false;
                        toggleSwitch4.Checked = true;
                        break;
                }
            }
            catch
            {
                Console.WriteLine("connection failed");
            }

            
        }

        public string readNewLine(int timeOut)
        {
            dataStream.ReadTimeout = timeOut;
            try
            {
                return dataStream.ReadLine() + "";
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        private void toggleSwitch1_CheckedChanged(object sender, EventArgs e)
        {

        }

        public void writeSerial(string command)
        {
            try
            {
                dataStream.WriteLine(command);
                dataStream.BaseStream.Flush();
            }
            catch
            {
                Console.WriteLine("write failed");
            }
        }

        private void colorWheel1_ColorChanged(object sender, EventArgs e)
        {
           
            
        }

        private void trackBar1_MouseUp(object sender, EventArgs e)
        {
            float k = trackBar1.Value / 100;
            //int m = (int)Math.Round(0.1 * Math.Pow(k, 2));
            int m = (int)Math.Round(k);
            //Console.WriteLine(m);
            label6.Text = "Speed control: " + m;
            writeSerial("tD," + k);
        }

        private void colorWheel1_MouseUp(object sender, MouseEventArgs e)
        {
            writeSerial("cRGB," + colorWheel1.Color.R + "," + colorWheel1.Color.G + "," + colorWheel1.Color.B);
        }

        private void toggleSwitch2_CheckedChanged(object sender, EventArgs e)
        {

        }

        public string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue() + "%";
        }

        public float getCurrentCpuFloat()
        {
            return cpuCounter.NextValue();
        }

        private void toggleSwitch3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void toggleSwitch4_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            float k = trackBar1.Value / 100;
            int m = (int)Math.Round(k);
            label6.Text = "Speed control: " + m;
        }
    }
}
