using System;
using System.Drawing;
using System.Text;

namespace SkyRemote
{

    public class ButtonRect
    {
        public string buttonName;
        public Rectangle rect;

        public ButtonRect(string name, Rectangle r)
        {
            buttonName = name;
            rect = r;
        }

        public override string ToString()
        {
            String txt = String.Format("<button name=\"{0}\" x=\"{1}\" y=\"{2}\" width=\"{3}\" height=\"{4}\"/>",
                buttonName, rect.X, rect.Y, rect.Width, rect.Height);
            return txt;
        }
    }

    public class Command
    {
        public string name;
        public int id;

        public Command(string name, int id)
        {
            this.name = name;
            this.id = id;
        }
    }

    public class Controller
    {
        public string name;
        public string ipaddress; 

        public Controller(string name, string ipaddress)
        {
            this.name = name;
            this.ipaddress = ipaddress;
        }
    }
}
