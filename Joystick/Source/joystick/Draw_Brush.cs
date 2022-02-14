using System.Threading;
using SharpDX.DirectInput;


namespace joystick
{
    //Klasa odpowiadajaca za odczytanie zmian w polozeniu joisticka potrzebnych do edytora graficznego 
    class Draw_Brush
    {

        private int x, y, z;
        private Draw draw;
        private Joystick joystick;

        private const int MovementDivider = 2000; //szybkosc kursora

        //pobranie edytora oraz urzadzenia 
        public Draw_Brush(Draw draw, Joystick device)
        {
            this.draw = draw;
            this.joystick = device;

        }
        //Watek odczytywania zmian przyciskow
        public void InputThread()
        {
            draw.ClearScreen();
            while (draw.Visible)
            {
                //pozycja x
                x = (joystick.GetCurrentState().X - 32767) / MovementDivider;
                //pozycja y
                y = (joystick.GetCurrentState().Y - 32767) / MovementDivider;
                //pozycja slidera
                z = joystick.GetCurrentState().Z;

                //wcisniecie przycisku fire rozpoczyna rysowanie
                if (joystick.GetCurrentState().Buttons[0])
                {
                    z /= 5000;

                    draw.Drawing(x, y, z);
                }

                //wcisniecie przycisku 2 (button[1]) czysci edytor graficzny
                if (joystick.GetCurrentState().Buttons[1])
                {
                    draw.ClearScreen();
                }
                //zmiana polozenia kursora w edytorze graficznym
                draw.ChangeXY(x, y);
                
                Thread.Sleep(10);
            }
        }
    }
}
