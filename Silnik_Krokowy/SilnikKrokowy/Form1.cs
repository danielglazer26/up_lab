using FTD2XX_NET;
using System;
using System.Windows.Forms;


     

namespace SilnikKrokowy
{
    public partial class Form1 : Form
    {
        bool connected = false;
        FTDI device = null;
        FTD2XX_NET.FTDI.FT_STATUS ftstatus;
        FTD2XX_NET.FTDI.FT_DEVICE_INFO_NODE[] devicelist;

        string[] modes = { "Tryb falowy", "Tryb pełnokrokowy", "Tryb półkrokowy" };

        byte[] leftWave = { 0x08, 0x02, 0x04, 0x01 }; //7.5
        byte[] rightWave = { 0x01, 0x04, 0x02, 0x08 };

        byte[] leftFullStep = { 0x09, 0x0A, 0x06, 0x05 }; //7.5
        byte[] rightFullStep = { 0x05, 0x06, 0x0A, 0x09 };

        byte[] leftHalfStepping = { 0x09, 0x08, 0x0A, 0x02, 0x06, 0x04, 0x05, 0x01 }; //3.75
        byte[] rightHalfStepping = { 0x01, 0x05, 0x04, 0x06, 0x02, 0x0A, 0x08, 0x09 }; 

        byte[] leftWave2 = { 0x10, 0x80, 0x20, 0x40 }; //7.5
        byte[] rightWave2 = { 0x40, 0x20, 0x80, 0x10 };

        byte[] leftFullStep2 = { 0x50, 0x90, 0xA0, 0x60, }; //7.5
        byte[] rightFullStep2 = { 0x60, 0xA0, 0x90, 0x50 };

        byte[] leftHalfStepping2 = { 0x50, 0x10, 0x90, 0x80, 0xA0, 0x20, 0x60, 0x40 }; //3.75
        byte[] rightHalfStepping2 = { 0x40, 0x60, 0x20, 0xA0, 0x80, 0x90, 0x10, 0x50 }; 

        byte[] stop = { 0x00 };



        int step;
        int angle;
        int speed;

        public int index = 0;
        public int index2 = 0;

        public Form1()
        {
            makeList();
            InitializeComponent();
            addCombox();
            if (!connected)
            {
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button5.Enabled = false;
            }

        }

        private void addCombox()
        {
            foreach (string mode in modes)
            {
                comboBox1.Items.Add(mode);
                comboBox2.Items.Add(mode);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        private void makeList()
        {
           

            device = new FTDI();
            devicelist = new FTDI.FT_DEVICE_INFO_NODE[1];
            ftstatus = device.GetDeviceList(devicelist);

            foreach (FTDI.FT_DEVICE_INFO_NODE a in devicelist)
                if (a != null)
                    listBox1.Items.Add(a.Description);

        }

        /// przycisk podłącz
        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                ftstatus = device.OpenByDescription(devicelist[listBox1.SelectedIndex].Description);
                ftstatus = device.SetBitMode(0xff, 1);

                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;
                button5.Enabled = true;

                DialogResult res = MessageBox.Show(
                    "Podłączono urządzenie",
                    "Komunikat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exp)
            {
                DialogResult res = MessageBox.Show(
                    "Nie wybrano urządzenia. Wybierz urządzenie",
                    "Błąd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        //przycisk odłącz
        private void button7_Click(object sender, EventArgs e)
        {
            if (device != null)
            {
                device.Close();
                makeList();

                if (!connected)
                {
                    button2.Enabled = false;
                    button3.Enabled = false;
                    button4.Enabled = false;
                    button5.Enabled = false;
                }

                DialogResult res = MessageBox.Show(
                    "Odłączono urządzenie",
                    "Komunikat",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        //lewo pierwszy silnik
        private void button2_Click(object sender, EventArgs e)
        {
            if (checkMovement())
            {
                int x;
                if (step != 0)
                    x = step;
                else
                    x = (int)(angle / 7.5);

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        rotation(x, speed, leftWave, true, true);
                        break;
                    case 1:
                        rotation(x, speed, leftFullStep, true, true);
                        break;
                    case 2:
                        rotation(x * 2, speed, leftHalfStepping, true, false);
                        break;
                }

            }
        }


        //lewo drugi silnik
        private void button3_Click(object sender, EventArgs e)
        {
            if (checkMovement())
            {
                int x;
                if (step != 0)
                    x = step;
                else
                    x = (int)(angle / 7.5);

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        rotation(x, speed, leftWave2, false, true);
                        break;
                    case 1:
                        rotation(x, speed, leftFullStep2, false, true);
                        break;
                    case 2:
                        rotation(x * 2, speed, leftHalfStepping2, false, false);
                        break;
                }

            }
        }



        // prawo pierwszy silnik 
        private void button4_Click(object sender, EventArgs e)
        {
            if (checkMovement())
            {
                int x;
                if (step != 0)
                    x = step;
                else
                    x = (int)(angle / 7.5);

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        rotation(x, speed, rightWave, true, true);
                        break;
                    case 1:
                        rotation(x, speed, rightFullStep, true, true);
                        break;
                    case 2:
                        rotation(x * 2, speed, rightHalfStepping, true, false);
                        break;
                }

            }
        }

        // prawo drugi silnik
        private void button5_Click(object sender, EventArgs e)
        {
            if (checkMovement())
            {
                int x;
                if (step != 0)
                    x = step;
                else
                    x = (int)(angle / 7.5);

                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        rotation(x, speed, rightWave2, false, true);
                        break;
                    case 1:
                        rotation(x, speed, rightFullStep2, false, true);
                        break;
                    case 2:
                        rotation(x * 2, speed, rightHalfStepping2, false, false);
                        break;
                }

            }

        }

        private bool checkMovement()
        {
            try
            {
                step = Convert.ToInt32(boxSteps.Text.ToString());
                angle = Convert.ToInt32(boxAngle.Text.ToString());
                speed = Convert.ToInt32(boxSpeed.Text.ToString());
                if (speed > 100)
                {
                    DialogResult res = MessageBox.Show(
                        "Za duża prędkość obrotu. Ustaw mniejszą niż 100",
                        "Błąd",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                DialogResult res = MessageBox.Show(
                    "Nie wpisano poprawnie danych",
                    "Błąd",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return false;
            }
        }

        public void rotation(int stepNumber, int speed, byte[] bytes, bool engineType, bool rotationType)
        {
            UInt32 bytesWritten = 0;
            Int32 bytesToWrite = 1;
            try
            {
                for (int i = 0; i < stepNumber; i++)
                {
                    int j;
                    if (engineType)
                    {
                        if (rotationType)
                            j = index % 4;
                        else
                            j = index % 8;

                        byte[] x = { bytes[j] };
                        ftstatus = device.Write(x, bytesToWrite, ref bytesWritten);
                        System.Threading.Thread.Sleep(1000 / speed);
                        index++;
                    }
                    else
                    {
                        if (rotationType)
                            j = index2 % 4;
                        else
                            j = index2 % 8;

                        byte[] x = { bytes[j] };
                        ftstatus = device.Write(x, bytesToWrite, ref bytesWritten);
                        System.Threading.Thread.Sleep(1000 / speed);
                        index2++;
                    }
                }
            }
            finally
            {
                ftstatus = device.Write(stop, 1, ref bytesWritten);
            }
        }

        private void boxSteps_TextChanged(object sender, EventArgs e)
        {
            boxAngle.Text = "0";
        }

        private void boxAngle_TextChanged(object sender, EventArgs e)
        {
            boxSteps.Text = "0";
        }


    }
}
