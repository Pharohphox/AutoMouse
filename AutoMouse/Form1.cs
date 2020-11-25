using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Input;
using System.Runtime.InteropServices;

namespace AutoMouse
{
    public partial class Clicker : Form
    {
        InterruptibleLoop il = new InterruptibleLoop();
        private Thread checkForUpdate;
        private volatile bool loopControl;
        private delegate void delUpdateTimesClicked(string updateString, string myEndTime);

        public Clicker()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "ON";
            StatusLabel.BackColor = Color.Green;
            ClickAway();
            StartCheck();
        }

        private void Stop_Click(object sender, EventArgs e)
        {
            StatusLabel.Text = "OFF";
            StatusLabel.BackColor = Color.Red;
            TimesClicked.Text = il.count.ToString();
            il.Stop();
            EndCheck();
            lblStartTime.Text = string.Empty;
            lblEndTime.Text = string.Empty;
        }

        private void ClickAway()
        {
            int wait;

            int.TryParse(textBox1.Text, out wait);
            lblStartTime.Text = DateTime.Now.ToShortTimeString();
            il.Start(wait);
        }

        private void CheckForUpdate()
        {
            delUpdateTimesClicked DelUpdateTimesClicked = new delUpdateTimesClicked(UpdateTimesClicked);
            
            while(!loopControl)
            {
                this.TimesClicked.BeginInvoke(DelUpdateTimesClicked, il.count.ToString(), il.endTime.ToShortTimeString());
                Thread.Sleep(5000);
            }
        }

        private void StartCheck()
        {
            if (checkForUpdate != null)
            {
                return;
            }

            loopControl = false;

            checkForUpdate = new Thread(CheckForUpdate);
            checkForUpdate.Start();
        }

        private void EndCheck()
        {
            if(checkForUpdate == null)
            {
                return;
            }

            loopControl = true;
            checkForUpdate.Join();

            checkForUpdate = null;
        }

        public void UpdateTimesClicked(string updateString, string myEndTime)
        {
            TimesClicked.Text = updateString;
            lblEndTime.Text = myEndTime;
        }

        private void Clicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            EndCheck();
        }
    }

    public class InterruptibleLoop
    {
        private volatile bool stopLoop;
        private Thread loopThread;
        private int _wait;
        public int count;
        public DateTime endTime;

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public void Start(int waiter)
        {
            _wait = waiter;

            // If the thread is already running, do nothing.
            if (loopThread != null)
            {
                return;
            }

            // Reset the "stop loop" signal.
            stopLoop = false;

            // Create and start the new thread.
            loopThread = new Thread(LoopBody);
            loopThread.Start();
        }

        public void Stop()
        {
            // If the thread is not running, do nothing.
            if (loopThread == null)
            {
                return;
            }

            // Signal to the thread that it should stop looping.
            stopLoop = true;

            // Wait for the thread to terminate.
            loopThread.Join();

            loopThread = null;
        }

        private void LoopBody()
        {
            count = 0;
            DateTime myTime = DateTime.Now.AddMinutes((double)_wait);
            endTime = myTime;

            Thread.Sleep(5000);

            while (!stopLoop && DateTime.Now <= myTime )
            {
                // Do your work here
                DoMouseClick();
                count++;
                Thread.Sleep(5000);
            }
        }

        private void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, X, Y, 0, 0);
        }
    }
}
