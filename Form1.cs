using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
namespace VRCEXLOGGER
{
    public partial class d : Form
    {
        public static d dd;
        public d()
        {
            LoggerUtils.Log("hello");
            InitializeComponent();
            dd = this;
        }
       
        private void d_Load(object sender, EventArgs e)
        {
            if (d.issetupalr == true)
            {
                button1.Enabled = false;
            }
            var NumAv = 0;

            if (Directory.Exists($"{Environment.CurrentDirectory}/VRCA"))
            {
                FileInfo[] sa = new DirectoryInfo($"{Environment.CurrentDirectory}/VRCA").GetFiles();
                for (int i = 0; i < sa.Length; i++)
                {
                    NumAv += 1;
                }
            }

            label1.Text += $"NUMBER OF AVATARS TOTAL LOGGED: {NumAv}";
            //    var s = new Thread(ChangeTextBoxText);
            //  s.Start();
        }
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Form1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        public void UpdateTextBox(string s)
        {
            label2.Text = s;
            
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Program.Cleanup();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                Program.Cleanup();
            }
            catch
            {
                MessageBox.Show("Cleaning proxy (make sure you didn't try to stop it twice)", "[ERROR]", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // Program.SetupProxy();
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Program.Cleanup();
            Program.SetupProxy();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Program.Cleanup();
            Program.SetupProxy();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            try
            {
                Program.Cleanup();
                //   Program.SetupProxy();

            }
            catch (Exception )
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();


            }
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        { 
            string LocalLowPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("Roaming", "LocalLow") + "\\VRChat\\VRChat\\Cache-WindowsPlayer";
            DirectoryInfo[] d = new DirectoryInfo(LocalLowPath).GetDirectories();
            foreach (var dir in d)
            {
                LoggerUtils.Log("Deleting: " + dir.FullName);
                DirectoryInfo s = new DirectoryInfo(dir.FullName);
                foreach (DirectoryInfo ss in s.GetDirectories())
                {
                    var se = ss.GetFiles();
                    foreach (var ee in se) 
                    {
                        ee.Delete();
                        //ss.Delete();
                    }
                    ss.Delete();
                  //  s.Delete();
                }
                s.Delete();

            }
        }

        private void d_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
