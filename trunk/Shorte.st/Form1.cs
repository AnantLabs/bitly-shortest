using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Json;

namespace Shorte.st
{
    public partial class Shortest : Form
    {
        //globals
        private ContextMenu trayMenu;
        private bool closeForm;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        private string keyPressed;
        private string url;
        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public Shortest()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(this.Form1_FormClosing);
            if (!RegisterHotKey(this.Handle, 0, (int)KeyModifier.Shift + (int)KeyModifier.Control, Keys.D.GetHashCode()))
                throw new InvalidOperationException("Couldn’t register the hot key.");
            if (!RegisterHotKey(this.Handle, 1, (int)KeyModifier.Shift + (int)KeyModifier.Control, Keys.F.GetHashCode()))
                throw new InvalidOperationException("Couldn’t register the hot key.");
        }

        private void Shortest_Load(object sender, EventArgs e)
        {
            notifyIcon1.Visible = true;
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);
            notifyIcon1.ContextMenu = trayMenu;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == 0x0312)
            {
                Keys key = (Keys)(((int)m.LParam >> 16) & 0xFFFF);                  // The key of the hotkey that was pressed.
                KeyModifier modifier = (KeyModifier)((int)m.LParam & 0xFFFF);       // The modifier of the hotkey that was pressed.
                int id = m.WParam.ToInt32();                                        // The id of the hotkey that was pressed.
                Console.WriteLine(key);
                keyPressed = key.ToString();
                if (keyPressed.Equals("D"))
                {
                    this.Show();
                    textBox1.SelectAll();
                }
                else if (keyPressed.Equals("F"))
                {
                    getUrl();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (closeForm == true)
            {
                UnregisterHotKey(this.Handle, 0);
                UnregisterHotKey(this.Handle, 1);
                e.Cancel = false;
                Close();
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        private void onReturn(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                getUrl();
            }
        }

        private void OnExit(object sender, EventArgs e)
        {
            closeForm = true;
            Close();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            textBox1.SelectAll();
        }

        private void getUrl()
        {
            if ((keyPressed != null ) && (keyPressed.Equals("F")))
            {
                url = Clipboard.GetText();
            }
            else
            {
                url = textBox1.Text;
            }
            //hide form on enter
            this.Hide();
            //CHECK FOR PROTOCOL
            if (!(url).Contains("http://") && !(url).Contains("https://"))
            {
                url = "http://" + url;
            }
            //get JSON response from bit.ly
            WebClient webClient = new WebClient();
            dynamic response = webClient.DownloadString("https://api-ssl.bitly.com/v3/shorten?access_token=39e9d629f1a1f71864ab23cd91c44a89278a25b5&longUrl=" + url);
            //check for JSON response
            if (response == null)
            {
                notifyIcon1.ShowBalloonTip(2000, "Shorte.st", "Error, try later", ToolTipIcon.Info);
            }
            else
            {
                //parse JSON and get global_hash
                var parsedJSON = JsonValue.Parse(response);
                var dataJSON = parsedJSON["data"];
                string globalHash = dataJSON["global_hash"];
                //set shortened URL
                string shortUrl = "http://bit.ly/" + globalHash;
                //copy shortened URL to clipboard
                Clipboard.SetText(shortUrl);
                notifyIcon1.ShowBalloonTip(2000, "Shorte.st", shortUrl, ToolTipIcon.Info); //"Link shorted and copied to clipboard."

            }
        }
    }
}
