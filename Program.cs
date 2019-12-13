using System;
using GsmComm.PduConverter;
using GsmComm.GsmCommunication;
using System.IO.Ports;
using System.Linq;

namespace sms_sender
{
    class Program
    {
        public static GsmCommMain comm;
        public static int smsCount = 0;
        public static int smsSent = 0;
        private static string message;
        private static string[] phoneNumbers;
        private static int failcount = 0;

        static void Main(string[] args)
        {
            message = args[1];
            phoneNumbers = args[0].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            smsCount = phoneNumbers.Count();
            execute();
        }

        private static void execute()
        {
            if (!autoconnect()) Environment.Exit(0);
            sendMessage(phoneNumbers[smsSent]);
        }

        private static void sendMessage(string phoneNumber)
        {
            Console.WriteLine("Sending message to " + phoneNumber);
            SmsSubmitPdu pdu;
            try
            {
                pdu = new SmsSubmitPdu(message, phoneNumber);
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
            Console.WriteLine("Message send fialed :" + phoneNumbers[smsSent]);
            if (smsSent < (smsCount - 1))
            {
                smsSent++;
                sendMessage(phoneNumbers[smsSent]);
            }
            else
            {
                Console.WriteLine(((smsSent + 1) - failcount) + " Total Messages successfully sent");
                Environment.Exit(0);
            }
        }

        private static void Comm_MessageSendComplete(object sender, MessageEventArgs e)
        {
            Console.WriteLine("Message sent to " + phoneNumbers[smsSent]);
            if (smsSent < (smsCount - 1))
            {
                smsSent++;
                sendMessage(phoneNumbers[smsSent]);
            }
            else
            {
                Console.WriteLine(((smsSent + 1) - failcount) + " Total Messages successfully sent");
                failcount = 0;
                Environment.Exit(0);
            }
        }
    }
}
