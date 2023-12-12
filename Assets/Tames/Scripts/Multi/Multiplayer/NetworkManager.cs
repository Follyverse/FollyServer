using Multi;
using RiptideNetworking;
using RiptideNetworking.Utils;
using System;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

     public Server Server { get; private set; }

    //   public static string commandIP;
    //   [SerializeField] private string ip;
    //   [SerializeField] private ushort port;

    private void Awake()
    {
        Singleton = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
        //  Debug.Log("starete");
             Server = new Server();
        Server.ClientDisconnected += PlayerLeft;
        Server.Start(ushort.Parse(TCPServer.MainPort), 2000);
     
    }

    private void FixedUpdate()
    {
    
         Server.Tick();

    }

    private void OnApplicationQuit()
    {
            
                Server.Stop();
 
    } 

    private void FailedToConnect(object sender, EventArgs e)
    {
        Debug.Log("NM: failed");
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
       Debug.Log("disconnected: "+e.Id+" "+ Server.Clients.Length);
      Player.Disconnect(e.Id);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
       
    }
}
