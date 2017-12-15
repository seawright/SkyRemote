/*
 * Copyright (c) <YEAR>, <OWNER>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, this
 *    list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE. 
 */
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml;


namespace SkyRemote
{
    public class SkyRemote : System.Windows.Forms.Form
    {
                 
        // The controller that's currently selected in the dropdown
        int m_currentController = -1;

        Bitmap skyhdremote;
        Bitmap redButton;
        Bitmap blackButton;
      
        ArrayList buttonRects = new ArrayList();
        ArrayList buttonRectsPlus = new ArrayList();
        ArrayList commands = new ArrayList();
        List<Controller> Controllers = new List<Controller>();
        Hashtable commandsHash = new Hashtable();

        private ComboBox skyBoxList;

        string data = null;
        bool IsDeviceOpen = false;
        
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SkyRemote app = new SkyRemote();
            Application.Run(app);
        }

        public SkyRemote()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            loadConfig();

            ClientSize = new Size(292, 600);
            skyhdremote = new Bitmap("data/SkyHD-remote.png");
            redButton = new Bitmap("data/redbutton.png");
            blackButton = new Bitmap("data/blackbutton.png");

            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);

            this.Paint += new PaintEventHandler(onPaint);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SkyRemote));
            this.skyBoxList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // skyBoxList
            // 
            this.skyBoxList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.skyBoxList.FormattingEnabled = true;
            this.skyBoxList.Location = new System.Drawing.Point(2, 3);
            this.skyBoxList.MaxDropDownItems = 64;
            this.skyBoxList.Name = "skyBoxList";
            this.skyBoxList.Size = new System.Drawing.Size(66, 21);
            this.skyBoxList.TabIndex = 1;
            this.skyBoxList.SelectedIndexChanged += new System.EventHandler(this.skyBoxList_SelectedIndexChanged);
            // 
            // SkyRemote
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.AutoValidate = System.Windows.Forms.AutoValidate.EnablePreventFocusChange;
            this.ClientSize = new System.Drawing.Size(264, 594);
            this.Controls.Add(this.skyBoxList);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(280, 630);
            this.MinimumSize = new System.Drawing.Size(280, 630);
            this.Name = "SkyRemote";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SkyHD Remote";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form1_MouseUp);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.ResumeLayout(false);

        }
        #endregion

        public void onPaint(System.Object sender, PaintEventArgs e)
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);

            RectangleF size = skyhdremote.GetBounds(ref unit);
            int offset = (int)(this.MaximumSize.Width - size.Width) / 2;
            e.Graphics.DrawImage(skyhdremote, offset, 0);
          
            // Draw connected button
            
            if (IsDeviceOpen)
                e.Graphics.DrawImage(redButton, 180, 18);
            else
                e.Graphics.DrawImage(blackButton, 180, 18);

        }

        private void Form1_Resize(object sender, System.EventArgs e)
        {
            Invalidate();
        }

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Text = "SkyHD Remote";
            string m_ipaddress = Controllers[m_currentController].ipaddress;                
            ButtonRect item = CheckButtonHit(e.X, e.Y);
            if (item != null)
            {
                Command c = (Command)commandsHash[item.buttonName];
                if (c != null)
                    SendCommand(m_ipaddress, (byte)c.id);
            }
        }

        private void Form1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Invalidate();
        }

        private ButtonRect CheckButtonHit(int x, int y)
        {
            ButtonRect ret = null;
            ArrayList buttons;
             
            buttons = buttonRectsPlus;

            foreach (ButtonRect item in buttons)
            {
                if (item.rect.Contains(x, y))
                {
                    Text = "SkyHD Remote [" + item.buttonName + "]";
                    ret = item;
                    break;
                }
            }

            return ret;
        }

        private void loadConfig()
        {
            XmlNodeList elemList;
            XmlTextReader reader;
            XmlDocument doc;

            reader = new XmlTextReader("data/controllers.xml");
            doc = new XmlDocument();
            try
            {
                doc.Load(reader);
            }
            catch (Exception e)
            {
                MessageBox.Show("controllers.xml could not be loaded. (" + e.ToString() + ")", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // find the controllers/connections configuration
            elemList = doc.GetElementsByTagName("controller");
            for (int i = 0; i < elemList.Count; i++)
            {
                string name = elemList[i].Attributes["name"].Value;
                string ipaddress = elemList[i].Attributes["ipaddress"].Value;
                
                skyBoxList.Items.Add(name);

                Controllers.Add(new Controller(name, ipaddress));
            }
            skyBoxList.SelectedIndex = 0;

            // set the width of the dropdown list to accomodate the widest item
            {
                int width = skyBoxList.DropDownWidth;
                Graphics g = skyBoxList.CreateGraphics();
                Font font = skyBoxList.Font;

                foreach (string s in (skyBoxList.Items))
                {
                    int newWidth = (int)g.MeasureString(s, font).Width +
                        SystemInformation.VerticalScrollBarWidth; ;
                    if (width < newWidth)
                    {
                        width = newWidth;
                    }
                }
                skyBoxList.Width = width;
            }

            reader.Close();

            reader = new XmlTextReader("data/buttons.xml");
            doc = new XmlDocument();
            try
            {
                doc.Load(reader);
            }
            catch (Exception e)
            {
                MessageBox.Show("buttons.xml could not be loaded. (" + e.ToString() + ")", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            // Setup commands
            elemList = doc.GetElementsByTagName("command");
            for (int i = 0; i < elemList.Count; i++)
            {
                string name = elemList[i].Attributes["name"].Value;
                int id = int.Parse(elemList[i].Attributes["id"].Value);
                Command item = new Command(name, id);

                commands.Add(item);
                commandsHash[item.name] = item;
            }

            // Setup buttons
            XmlNodeList buttons = doc.GetElementsByTagName("buttons");
            if (buttons[0] != null)
            {
                for (XmlNode i = buttons[0].FirstChild; i != null; i = i.NextSibling)
                {
                    String name = i.Attributes["name"].Value;
                    int x = int.Parse(i.Attributes["x"].Value);
                    int y = int.Parse(i.Attributes["y"].Value);
                    int width = int.Parse(i.Attributes["width"].Value);
                    int height = int.Parse(i.Attributes["height"].Value);
                    Rectangle rect = new Rectangle(x, y, width, height);

                    ButtonRect item = new ButtonRect(name, rect);
                    buttonRects.Add(item);
                }
            }

            XmlNodeList buttonsplus = doc.GetElementsByTagName("buttonsplus");
            if (buttonsplus[0] != null)
            {
                for (XmlNode i = buttonsplus[0].FirstChild; i != null; i = i.NextSibling)
                {
                    String name = i.Attributes["name"].Value;
                    int x = int.Parse(i.Attributes["x"].Value);
                    int y = int.Parse(i.Attributes["y"].Value);
                    int width = int.Parse(i.Attributes["width"].Value);
                    int height = int.Parse(i.Attributes["height"].Value);
                    Rectangle rect = new Rectangle(x, y, width, height);

                    ButtonRect item = new ButtonRect(name, rect);
                    buttonRectsPlus.Add(item);
                }
            }
            reader.Close();

        }

        bool SendCommand(string address, byte Command)
        {
            Console.WriteLine(Command);

            // Connect to a remote device.
            try
            {
                // Establish the remote endpoint for the socket.
                IPAddress ipAddress = IPAddress.Parse(address);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 49160);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {
                    sender.Connect(remoteEP);

                    Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                    int number = 0;

                    // Data buffer for incoming data.
                    byte[] bytes = new byte[1024];
                    byte[] msg = new byte[24];
                    int bytesRec = 0;
                    int bytesSent = 0;
                    int len = 12;

                    do
                    {
                        // Receive the response from the remote device.
                        bytesRec = sender.Receive(bytes);

                        data = Encoding.ASCII.GetString(bytes, 0, len);

                        // Encode the data string into a byte array.
                        msg = Encoding.ASCII.GetBytes(data);

                        // Send the data through the socket.
                        bytesSent = sender.Send(msg);

                        len = 1;
                        number++;
                    }
                    while (number < 3);

                    // Receive the response from the remote device.
                    bytesRec = sender.Receive(bytes);

                    byte[] msg1 = { 4, 1, 0, 0, 0, 0, (byte)(Command / 16 + 224), (byte)(Command % 16) };

                    // Send the data through the socket.
                    bytesSent = sender.Send(msg1);

                    msg1[1] = 0;

                    // Send the data through the socket.
                    bytesSent = sender.Send(msg1);

                    // Release the socket.
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();
                    return true;
                    
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    return false;
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    MessageBox.Show(Controllers[m_currentController].name +
                    " Sky set top box with IP address: (" + Controllers[m_currentController].ipaddress +
                    ") is not responding.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);            
                    return false;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    return false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
                return false;
            } 
        }

        // This is also called when we start and assign set the selected index
        // to the first controller
        private void skyBoxList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_currentController == skyBoxList.SelectedIndex)
                return;

            m_currentController = skyBoxList.SelectedIndex;

            IsDeviceOpen = SendCommand(Controllers[m_currentController].ipaddress, 127);


            Console.WriteLine("item " + m_currentController + " " + Controllers[m_currentController].name);
            
            // redraw the red/black dot appropriately
            this.Refresh();
            this.ActiveControl = null;
        }
    }
}