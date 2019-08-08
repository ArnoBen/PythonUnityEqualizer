using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceiver : MonoBehaviour
{
    Thread readThread;
    UdpClient client;
    public int port = 9900;
    public string lastReceivedPacket = "";

    [SerializeField] BytesVariable UDPData;

    void Start()
    {
        // create thread for reading UDP messages
        readThread = new Thread(new ThreadStart(ReceiveData));
        readThread.IsBackground = true;
        readThread.Start();
    }

    // receive thread function
    private void ReceiveData()
    {
        client = new UdpClient(port);
        //client.Client.Blocking = false;
        while (true)
        {
            try
            {
                // receive bytes
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                UDPData.Value = data;
                // encode UTF8-coded bytes to text format
                string text = Encoding.UTF8.GetString(data);

                // show received message
                //print(">> " + text);

                // store new massage as latest message
                lastReceivedPacket = text;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // return the latest message
    public string getLatestPacket()
    {
        return lastReceivedPacket;
    }

    void OnApplicationQuit()
    {
        stopThread();
    }

    private void OnDisable()
    {
        stopThread();
    }
    private void OnDestroy()
    {
        stopThread();
    }
    
    // Stop reading UDP messages
    private void stopThread()
    {
        if (readThread.IsAlive)
        {
            readThread.Abort();
        }
        client.Close();
    }
}