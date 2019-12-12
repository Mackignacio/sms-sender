using System;
using GsmComm.PduConverter;
using GsmComm.GsmCommunication;
using System.IO.Ports;

namespace sms_sender
{
    class Program
    {
        public static GsmCommMain comm;
        public static int smscount = 0;
        public static int smssent = 0;
        private static string message;
        private static string phonenum;
        private static int failcount = 0;

        static void Main(string[] args)
        {
            message = args[1];
            phonenum = args[0];

            execute();
        }

        private static void execute()
        {
            if (!autoconnect()) Environment.Exit(0);
            sendMessage();
        }

        private static void sendMessage()
        {
            SmsSubmitPdu pdu;
            try
            {
                pdu = new SmsSubmitPdu(message, phonenum);
                if (!comm.IsOpen() || !comm.IsConnected())
                    comm.Open();
                comm.SendMessage(pdu, true);
            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine(e);
            }
        }

        private static bool autoconnect()
        {
            bool connected = false;
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                int index = port.LastIndexOf("COM");
                string tempPort = port.Substring(index).Replace("COM", string.Empty).Replace(")", string.Empty);
                Int16 Comm_Port = Int16.Parse(tempPort);
                connected = connect(Comm_Port);
                if (connected) break;
            }

            return connected;
        }

        private static bool connect(Int16 comPort)
        {
            bool connected = false;
            Int32 baudRate = 115200;
            Int32 timeOut = 100;

            comm = new GsmCommMain(comPort, baudRate, timeOut);
            comm.MessageSendComplete += Comm_MessageSendComplete;
            comm.MessageSendFailed += Comm_MessageSendFailed;

            try
            {
                comm.Open();
                if (comm.IsConnected())
                {
                    Console.WriteLine("Connected to COM" + comPort);
                    connected = true;
                }
                else
                {
                    comm.Close();
                    comm = null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine(e);
            }
            return connected;
        }

        private static void Comm_MessageSendFailed(object sender, MessageErrorEventArgs e)
        {
            failcount++;
            Console.WriteLine("Message send fialed [" + failcount + "]:" + phonenum);
            if (smssent < (smscount - 1))
            {
                smssent++;
                sendMessage();
            }
            else
            {
                Console.WriteLine(((smssent + 1) - failcount) + " Total Messages successfully sent");
                Environment.Exit(0);
            }
        }

        private static void Comm_MessageSendComplete(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Message sent: " + phonenum);

            if (smssent < (smscount - 1))
            {
                smssent++;
                sendMessage();
            }
            else
            {
                Console.WriteLine(((smssent + 1) - failcount) + " Total Messages successfully sent");
                failcount = 0;
                Environment.Exit(0);
            }
        }
    }
}
