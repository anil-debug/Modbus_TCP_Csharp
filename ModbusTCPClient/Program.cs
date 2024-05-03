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

        public void SendData_str(object data, int registerAddress)
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
        public void SendData(int[] data, int registerAddress)
        {
            try
            {
                if (client == null)
                {
                    throw new InvalidOperationException("Client is not connected.");
                }

                // Modbus Function Code for Write Multiple Registers
                byte functionCode = 0x10;

                // Calculate the byte count
                int byteCount = data.Length * sizeof(short); // Assuming each integer is 16 bits (2 bytes)

                // Construct the Modbus request packet
                byte[] request = new byte[13 + byteCount]; // Adjusted the array size to accommodate the request

                // Populate the request packet with necessary data
                request[0] = 0x00; // Transaction Identifier (high byte)
                request[1] = 0x01; // Transaction Identifier (low byte)
                request[2] = 0x00; // Protocol Identifier (high byte)
                request[3] = 0x00; // Protocol Identifier (low byte)
                request[4] = (byte)((7 + byteCount) >> 8); // Length (high byte)
                request[5] = (byte)((7 + byteCount) & 0xFF); // Length (low byte)
                request[6] = 0x01; // Unit Identifier
                request[7] = functionCode; // Function Code
                request[8] = (byte)(registerAddress >> 8); // Starting Address of Register to Write (high byte)
                request[9] = (byte)(registerAddress & 0xFF); // Starting Address of Register to Write (low byte)
                request[10] = (byte)(data.Length >> 8); // Number of Registers to Write (high byte)
                request[11] = (byte)(data.Length & 0xFF); // Number of Registers to Write (low byte)
                request[12] = (byte)(byteCount); // Byte Count

                // Copy the integer values to the request packet
                for (int i = 0; i < data.Length; i++)
                {
                    int byteIndex = 13 + i * 2;
                    Console.WriteLine($"Copying integer value {data[i]} at index {byteIndex}");

                    request[byteIndex] = (byte)(data[i] >> 8); // High byte of the integer
                    request[byteIndex + 1] = (byte)(data[i] & 0xFF); // Low byte of the integer
                }

                // Send the request packet to the Modbus server
                NetworkStream stream = client.GetStream();
                stream.Write(request, 0, request.Length);

                Console.WriteLine("Data sent successfully.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to send data: {e.Message}");
            }
        }

        public int[] ReadIntegers(int registerAddress, int numberOfRegisters)
        {
            try
            {
                if (client == null)
                {
                    throw new InvalidOperationException("Client is not connected.");
                }

                // Modbus Function Code for Read Holding Registers
                byte functionCode = 0x03;

                // Construct the Modbus request packet
                byte[] request = new byte[]
                {
                    // Transaction Identifier (random value, you can increment this for each request)
                    0x00, 0x01,
                    // Protocol Identifier (always 0x00 0x00 for Modbus TCP)
                    0x00, 0x00,
                    // Length (high byte) (excluding first 6 bytes)
                    0x00, 0x06,
                    // Unit Identifier (usually 0x01 for Modbus RTU)
                    0x01,
                    // Function Code (Read Holding Registers)
                    functionCode,
                    // Starting Address of Register to Read (high byte)
                    (byte)(registerAddress >> 8),
                    // Starting Address of Register to Read (low byte)
                    (byte)(registerAddress & 0xFF),
                    // Number of Registers to Read (high byte)
                    (byte)(numberOfRegisters >> 8),
                    // Number of Registers to Read (low byte)
                    (byte)(numberOfRegisters & 0xFF),
                };

                // Send the request packet to the Modbus server
                NetworkStream stream = client.GetStream();
                stream.Write(request, 0, request.Length);

                // Read the response from the Modbus server
                byte[] buffer = new byte[numberOfRegisters * 2]; // Each register is 2 bytes
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                // Parse the received data as integers
                int[] integers = new int[numberOfRegisters];
                for (int i = 0; i < numberOfRegisters; i++)
                {
                    // Combine two bytes into a single integer
                    int value = (buffer[i * 2] << 8) | buffer[i * 2 + 1];
                    integers[i] = value;
                }

                return integers;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read data: {e.Message}");
                throw; // rethrow the caught exception
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
            string MODBUS_IP = "your modbus IP";
            // Define the data to be sent
            var data_str = new
            {
                temperature = 25,
                humidity = 60,
                status = "ok"
            };
            int[] data = new int[] { 25, 60, 100 }; // Example list of integers
            // Create a Modbus TCP/IP client
            ModbusTCPClient tcpClient = new ModbusTCPClient(MODBUS_IP);
            try
            {
                // Connect to the Modbus TCP/IP device
                tcpClient.Connect();
                // Send data to Modbus TCP/IP device
                tcpClient.SendData(data, registerAddress: 190);
                // tcpClient.SendData_str(data_str,registerAddress: 101);

                // Read data from Modbus TCP/IP device
                int[] receivedData = tcpClient.ReadIntegers(registerAddress: 190, numberOfRegisters: 3);
                foreach (int value in receivedData)
                {
                    Console.WriteLine($"Received integer: {value}");
                }

            }
            finally
            {
                // Close the Modbus TCP/IP connection
                tcpClient.Close();
            }
        }
    }
}
