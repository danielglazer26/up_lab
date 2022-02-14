using System;
using System.Drawing;
using System.Windows.Forms;
using SharpDX.DirectInput;
using WindowsInput;

namespace joystick
{
    //Klasa odpowiadajaca za edytor graficzny
    public partial class Draw : Form
    {
        private System.Threading.Thread myThread;
        private Joystick joystick; //urzadzenie
        private IMouseSimulator mouseSimulator; //emulator myszy
        int x = 0, y = 0; //pozycje kursora w edytorze

        //Inicjalizacja okna programu i przypisanie myszki do emulatora
        public Draw()
        {
            InitializeComponent();
            this.mouseSimulator = new InputSimulator().Mouse;
        }

        //dodanie urzadzenia
        public void addJoystick(Joystick joystick)
        {
            this.joystick = joystick;
        }

        //utworzenie pola do rysowania
        public static Draw CreateComponent()
        {
            Draw draw = null;
            System.Threading.Thread thread = new System.Threading.Thread(() =>
            {
                draw = new Draw();
                draw.ShowDialog();
            });
            thread.Start();
            while (draw == null)
            {
                System.Threading.Thread.Sleep(10);
            }
            draw.myThread = thread;

            return draw;
        }

        //tykanie timera sprawdza czy edytor nalezy odswiezyc
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (updated)
            {
                updated = false;
                Refresh();
            }
        }

        //zwracanie szerokosci i wysokosci okna edytora
        public int szerokosc { get { return pictureBox1.Width; } }
        public int wysokosc { get { return pictureBox1.Height; } }

        //rysowannie na ekranie kursora w edytorze graficznym
        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            lock (bmp)
            {
                e.Graphics.DrawImage(bmp, 0, 0);
            }
            using (SolidBrush brush = new SolidBrush(Color.Black))
            {
                e.Graphics.FillEllipse(brush, x - 4, y - 4, 2 * 4, 2 * 4);
            }
        }

        //czyszczenie ekranu edytora
        public void ClearScreen()
        {
            bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            lock (bmp)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, 0, 0, szerokosc, wysokosc);
                }
                updated = true;
            }
        }
        //rysowanie w edytorze
        public void Drawing(int x, int y, int r)
        {
            lock (bmp)
            {
                using (Graphics g = Graphics.FromImage(bmp))
                using (SolidBrush brush = new SolidBrush(Color.Red))
                {
                    g.FillEllipse(brush, this.x - r, this.y - r, 2 * r, 2 * r);
                }
                updated = true;
            }
        }

        //zmiana polozenia x,y wraz ze sprawdzeniem aby nie znalazl sie poza oknem programu
        public void ChangeXY(int x, int y)
        {

            if (this.x + x > 0 && this.x + x < szerokosc)
                this.x = this.x + x;
            if (this.y + y > 0 && this.y + y < wysokosc)
                this.y = this.y + y;

            updated = true;
        }

        //utworzenie bitmapy do reprezentowania edytora
        private void LoadDraw(object sender, EventArgs e)
        {
            bmp = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
        }
    }
}
