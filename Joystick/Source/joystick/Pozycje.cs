using SharpDX.DirectInput;
using System;
using System.Windows.Forms;

namespace joystick
{
    //Klasa odpowiadajaca za okno testowania urzadzenia (X,Y,Z,FIRE)
    public partial class Pozycje : Form
    {
        System.Threading.Timer timer;
        Joystick joystick;
        private bool isEnable = false;

        public Pozycje(Joystick joystick)
        {
            InitializeComponent();

            this.joystick = joystick;

            timer = new System.Threading.Timer(obj => Update()); //timer obslugujacy zmiane danych wejsciowych urzadzenia

        }
        //Uruchomienie programu poprzez Timer
        private void start_Click(object sender, EventArgs e)
        {
            if (!isEnable)
            {
                isEnable = true;
                timer.Change(0, 1000 / 60); //odswiezanie timera
            }
            else
            {
                isEnable = false;
                timer.Dispose();
            }
        }

        //Metoda pobierajaca aktualny stan urzadzenia i wyswietlajacy go na ekranie
        private new void Update()
        {


            this.textBox1.Invoke((MethodInvoker)delegate
           {
               this.textBox1.Text = String.Format("{0}", joystick.GetCurrentState().X);
           });
            this.textBox2.Invoke((MethodInvoker)delegate
            {
                this.textBox2.Text = String.Format("{0}", joystick.GetCurrentState().Y);
            });
            this.textBox3.Invoke((MethodInvoker)delegate
            {
                this.textBox3.Text = "" + joystick.GetCurrentState().Z;
            });
            this.textBox4.Invoke((MethodInvoker)delegate
            {
                this.textBox4.Text = "" + joystick.GetCurrentState().Buttons[0];
            });

        }

        private void Pozycje_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isEnable) timer.Dispose();
        }
    }
}
