using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class Async : MonoBehaviour
{
    Thread SocketThread;
    volatile bool keepReading = false;
    Socket listener;
    Socket handler;

    // Objeto que vamos a mover (asignado desde Unity o buscado por nombre)
    public GameObject esfera;

    // Variable compartida para la nueva posición
    Vector3 nuevaPosicion = Vector3.zero;
    volatile bool actualizarPosicion = false;

    void Start()
    {
        Application.runInBackground = true;

        if (esfera == null)
            esfera = GameObject.Find("Esfera");

        startServer();
    }

    void Update()
    {
        // Mueve la esfera si hay nueva posición
        if (actualizarPosicion)
        {
            esfera.transform.position = nuevaPosicion;
            actualizarPosicion = false;
        }
    }

    void startServer()
    {
        SocketThread = new Thread(networkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    void networkCode()
    {
        byte[] bytes;
        IPAddress IPAdr = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(IPAdr, 1201);
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            while (true)
            {
                keepReading = true;
                Debug.Log("Esperando conexión...");

                handler = listener.Accept();
                Debug.Log("Cliente conectado");

                byte[] SendBytes = System.Text.Encoding.ASCII.GetBytes("Conectado al servidor<EOF>");
                handler.Send(SendBytes);

                while (keepReading)
                {
                    bytes = new byte[1024];
                    string data = "";

                    int bytesRec = handler.Receive(bytes);
                    if (bytesRec <= 0)
                    {
                        keepReading = false;
                        handler.Disconnect(true);
                        break;
                    }

                    data = System.Text.Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    Debug.Log("Mensaje recibido: " + data);

                    if (data.Contains("<EOF>"))
                    {
                        string cleanData = data.Replace("<EOF>", "");

                        if (cleanData.StartsWith("POS:"))
                        {
                            string pos = cleanData.Substring(4);
                            string[] valores = pos.Split(',');

                            if (valores.Length == 3 &&
                                float.TryParse(valores[0], out float x) &&
                                float.TryParse(valores[1], out float y) &&
                                float.TryParse(valores[2], out float z))
                            {
                                nuevaPosicion = new Vector3(x, y, z);
                                actualizarPosicion = true;
                                Debug.Log("Nueva posición asignada: " + nuevaPosicion);
                            }
                        }

                        break;
                    }

                    Thread.Sleep(1);
                }

                handler.Close();
                Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error en el servidor: " + e.ToString());
        }
    }

    void stopServer()
    {
        keepReading = false;

        if (SocketThread != null)
        {
            SocketThread.Abort();
        }

        if (handler != null && handler.Connected)
        {
            handler.Disconnect(false);
            Debug.Log("Desconectado!");
        }
    }

    void OnDisable()
    {
        stopServer();
    }
}
