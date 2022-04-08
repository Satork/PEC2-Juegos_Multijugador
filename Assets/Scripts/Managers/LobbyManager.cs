using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using UnityEngine.UI;

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;

        public NetworkDiscovery networkDiscovery;

        
        public GameObject servidores;
        public RectTransform ParentPanel;

        public void NuevoServidor(){
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();

        }
        public void NuevaPartida(){
            discoveredServers.Clear();
            NetworkManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();
        }
        public void UnirsePartida(){
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();

        }
        /* TODO: falta que se muestre el listado de los servidores disponibles */
        void Update(){
            foreach (ServerResponse info in discoveredServers.Values)
                Connect(info);                
        }
        
        public void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;

        }
        void Connect(ServerResponse info)
        {
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }
        
    }

}

