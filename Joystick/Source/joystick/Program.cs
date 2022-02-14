using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.DirectInput;
using System.Windows.Forms;

namespace joystick
{
    class Program
    {
        

        static void Main(string[] args)
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Utworzenie glownego okna programu
            Application.Run(new OknoListyUrzadzen());
            
        }

    }
}