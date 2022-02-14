using SharpDX.DirectInput;
using WindowsInput;


namespace joystick
{
    //Klasa odpowiadajaca za poruszanie kursorem za pomoca wybranego urzadzenia (gamepad,joystick)
    class EmulacjaMyszy
    {
        private const int MovementDivider = 2000; //zmienna odpowiedzialna za szybkosc poruszania sie kursora
        System.Threading.Timer timer;
        private Joystick joystick;
        private IMouseSimulator mouseSimulator;   //interfejs WindowsInput do obslugi zdarzen myszy
       
        //Metoda obslugujaca timer oraz pobierajaca joystick
        public EmulacjaMyszy(Joystick joystick)
        {
            this.joystick = joystick;
            joystick.Acquire();
            mouseSimulator = new InputSimulator().Mouse;  //symulator myszy
            timer = new System.Threading.Timer(obj => Update());

            timer.Change(0, 1000 / 60);

        }

        //metoda pobierajaca stan kontrolera i emulujaca zachowanie kursora
        private void Update()
        {
                var x = (joystick.GetCurrentState().X - 32767) / MovementDivider; //obliczenie polozenia wspolrzednej x 
                var y = (joystick.GetCurrentState().Y - 32767) / MovementDivider; //obliczenie polozenia wspolrzednej y
                mouseSimulator.MoveMouseBy(x, y); //przekazanie x,y do symulatora myszy
                if (joystick.GetCurrentState().Buttons[0]) mouseSimulator.LeftButtonDown(); //przycisk Fire (1) jako lewy przycisk myszy 
                else mouseSimulator.LeftButtonUp();
                if (joystick.GetCurrentState().Buttons[1]) mouseSimulator.RightButtonDown(); //przycisk 2 jako prawy przycisk myszy 
                else mouseSimulator.RightButtonUp();
            
        }

        public void TurnOff()
        {
            timer.Dispose();
        }
    }
}
