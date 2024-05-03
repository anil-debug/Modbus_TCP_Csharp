using System;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace ModbusTCPClient
{
    class ModbusTCPClient
    {
        private string ip;
        private int port;
        private TcpClient? client; // Nullable TcpClient

        public ModbusTCPClient(string ip, int port = 502)
        {
            this.ip = ip;
            this.port = port;
        }

        public void Connect()
        {
            if (client == null)
            {
                client = new TcpClient();
                client.Connect(ip, port);
            }
        }

        public void SendData(object data, int registerAddress)
        {
            try
            {
                if (client == null)
                {
                    throw new InvalidOperationException("Client is not connected.");
                }

                // Serialize the data to JSON
                string dataJson = JsonConvert.SerializeObject(data);
                // Convert the JSON data to bytes
                byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(dataJson);
                // Write the data to a holding register
                NetworkStream stream = client.GetStream();
                stream.Write(dataBytes, 0, dataBytes.Length);
                Console.WriteLine("Data sent successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send data: {e.Message}");
            }
        }

        public void Close()
        {
            client?.Close();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Define the Modbus device's IP address
            string MODBUS_IP = "0.0.0.0";
            // Define the data to be sent
            var data = new
            {
                temperature = 25,
                humidity = 60,
                status = "ok"
            };
            // Create a Modbus TCP/IP client
            ModbusTCPClient tcpClient = new ModbusTCPClient(MODBUS_IP);
            try
            {
                // Connect to the Modbus TCP/IP device
                tcpClient.Connect();
                // Send data to Modbus TCP/IP device
                tcpClient.SendData(data, registerAddress: 100);
            }
            finally
            {
                // Close the Modbus TCP/IP connection
                tcpClient.Close();
            }
        }
    }
}
