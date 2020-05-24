using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestConnectionSerial
{
    public partial class MainWindow : Form
    {
        public string current_port;
        public SerialReaderThread thread = null;
        public bool text_reading = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private string[] ListSerialPorts()
        {
            // Get a list of serial port names.
            string[] ports = SerialPort.GetPortNames();
            // Display each port name to the console.
            //foreach (string port in ports)
            //{
            //    Console.WriteLine("- " + port);
            //}
            return ports;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // FORM 1 LOADED
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] ports = ListSerialPorts();
            serialComboBox.Items.Clear();    
            foreach(string item in ports)
            {
                serialComboBox.Items.Add(item);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Object selectedItem = serialComboBox.SelectedItem;
            string current_port = selectedItem.ToString();
            //SerialPort serial_port = new SerialPort(current_port, 9600);
            //serial_port.Open();
            richTextBox1.Text += "---------- SERIAL READ THREAD STARTING ----------" + "\n";
            if (thread == null)
            {
                thread = new SerialReaderThread(current_port);
                thread.DataReceived += ThreadDataReceived;
                thread.Start();
            }
            text_reading = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (thread != null)
            {
                richTextBox1.Text += "---------- SERIAL READ THREAD END REQUEST ----------" + "\n";
                richTextBox1.Text += "---------- WAITING ----------" + "\n";
                thread.Close();
                thread = null;
                richTextBox1.Text += "---------- SERIAL READ THREAD ENDED ----------" + "\n";
            }
        }

        // SERIAL Data Receiver (From the Emitter)
        // https://www.codeproject.com/Answers/228189/serial-port-using-threading#answer2
        void ThreadDataReceived(object s, EventArgs e)
        {
            // Note: this method is called in the thread context, thus we must
            // use Invoke to talk to UI controls. So invoke a method on our
            // thread.
            if (text_reading) BeginInvoke(new EventHandler<DataEventArgs>(ThreadDataReceivedSync), new object[] { s, e });
        }

        void ThreadDataReceivedSync(object s, DataEventArgs et)
        {
            // Handle Data Received by the Serial Thread
            if (thread != null)
            {
                richTextBox1.Text += et.Data + "\n";
            }
        }
    }

    // Serial Class and Data Emitter
    public class DataEventArgs : EventArgs
        // DATA CLASS WHICH IS USE TO SEND DATA TO GUI
        // start with a string Data 
        // Graph might be double[] ? 
    {
        public string Data { get; private set; }

        public DataEventArgs(string data) { 
            Data = data; 
        }
    }
    public class SerialReaderThread
    {
        private Thread t;
        private string port;
        private SerialPort mySerialPort;
        public SerialReaderThread(string port)
        {
            this.port = port;
            t = new Thread(RunMethod);
        }

        public void Start() { t.Start(); }

        // note: this event is fired in the background thread
        public event EventHandler<DataEventArgs> DataReceived;

        // Flag determined that the spinner is running
        private bool running = false;
        public void Close() { 
            running = true;
            t.Join();
        }
        private void RunMethod()
        {
            // I'll just believe that this is correct
            mySerialPort = new SerialPort(this.port);
            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;
            mySerialPort.Open();

            while (!running)
            {
                // CUSTOM DATA TYPE CREATE HERE BY CONVERT string line into other
                // Read the fcking serial
                string line = mySerialPort.ReadLine();
                // Emit string line by Data Event
                if (DataReceived != null) DataReceived(this, new DataEventArgs(line));
            }
            mySerialPort.Close();
            Console.WriteLine("Closed Serial Port : " + this.port);
        }
    }

   
}
