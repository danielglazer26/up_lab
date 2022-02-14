using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;



namespace joystick
{
    //Klasa obslugujaca glowne okno programu
    public partial class OknoListyUrzadzen : Form
    {

        Joystick joystick;
        SharpDX.DirectInput.DirectInput directInput;
        List<DeviceInstance> listaUrzadzen;
        private EmulacjaMyszy emulacja = null;
        private bool wcisniety = false;


        //Pobranie na liste dostepnych urzadzen (gamepad, joystick)
        public OknoListyUrzadzen()
        {
            InitializeComponent();

            //wykorzystanie DirectInput do komunikacji z urzadzeniami
            directInput = new SharpDX.DirectInput.DirectInput();
            listaUrzadzen = new List<DeviceInstance>();
            var listaJoystickow = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly);
            var listaGamepadow = directInput.GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly);

            //dodanie urzadzen do listy
            foreach (DeviceInstance joystick in listaJoystickow)
                listaUrzadzen.Add(joystick);


            foreach (DeviceInstance gamepad in listaGamepadow)
                listaUrzadzen.Add(gamepad);

            //dodanie urzadzen z listy do listBoxa
            foreach (DeviceInstance urzadzenie in listaUrzadzen)
                listBox1.Items.Add(urzadzenie.InstanceName);


        }

        //Testowanie urzadzenia (X,Y,Z,FIRE)
        private void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                joystick = new Joystick(directInput, listaUrzadzen.ElementAt(listBox1.SelectedIndex).InstanceGuid);
                joystick.Acquire();
                new Pozycje(joystick).ShowDialog();
            }
        }

        //Poruszanie mysza za pomoca joysticka
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                wcisniety = !wcisniety;
                if (!wcisniety) emulacja.TurnOff();
                else
                {
                    joystick = new Joystick(directInput, listaUrzadzen.ElementAt(listBox1.SelectedIndex).InstanceGuid);
                    emulacja = new EmulacjaMyszy(joystick);
                }
            }

        }

        //Uruchomienie edytora graficznego
        private void button3_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                joystick = new Joystick(directInput, listaUrzadzen.ElementAt(listBox1.SelectedIndex).InstanceGuid);
                joystick.Acquire();
                Draw canvas = Draw.CreateComponent();
                Draw_Brush paint = new Draw_Brush(canvas, joystick);
                paint.InputThread();

            }
        }

    }
}
