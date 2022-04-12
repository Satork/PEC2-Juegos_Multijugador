using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
	    private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        Vector2 scrollViewPos = Vector2.zero;

        public NetworkDiscovery networkDiscovery;
        public GameObject m_MenuLayout;
        public GameObject m_JoinLayout;
        public GameObject m_ScrollContent;

        public GameObject m_ServerPrefab;

        private List<GameObject> instances = new List<GameObject>();

        private void Start() {
	        m_MenuLayout.SetActive(true);
	        m_JoinLayout.SetActive(false);
        }

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
            m_MenuLayout.SetActive(false);
            m_JoinLayout.SetActive(true);
        }

        public void Return() {
	        instances.ForEach(Destroy);
	        instances.Clear();
	        discoveredServers.Clear();
	        m_MenuLayout.SetActive(true);
	        m_JoinLayout.SetActive(false);
        }
        /* TODO: falta que se muestre el listado de los servidores disponibles */

        private IEnumerator UpdateServers() {
	        var posY = -25;
	        if (discoveredServers.Count == instances.Count) yield break;
	        Debug.Log($"UpdateServers: {discoveredServers.Count}");
	        var index = 0;
	        foreach (var server in discoveredServers) {
		        var serverInstance = Instantiate(m_ServerPrefab, m_ScrollContent.transform);
		        var serverRect = serverInstance.GetComponent<RectTransform>();
		        var serverButton = serverInstance.GetComponent<Button>();
		        var serverText = serverInstance.GetComponent<TextMeshProUGUI>();

		        serverRect.localPosition +=  Vector3.up * posY * index;
				

		        index++;
		        serverText.text = $"ServerID: {server.Value.uri}";

		        serverButton.onClick.AddListener(delegate { Connect(server.Value); });

		        instances.Add(serverInstance);
	        }
	        yield return null;
        }

        public void OnDiscoveredServer(ServerResponse info)
        {
            discoveredServers[info.serverId] = info;
            Debug.Log($"OnDiscoveredServer: {discoveredServers.Count}");
            StartCoroutine(UpdateServers());

        }
        void Connect(ServerResponse info)
        {
            networkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }
        
    }

}

