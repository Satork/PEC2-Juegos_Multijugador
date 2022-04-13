using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine.UI;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
	    private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

        public NetworkDiscovery networkDiscovery;
        
        [Header("Layouts")]
        public GameObject m_MenuLayout;
        public GameObject m_JoinLayout;
        public GameObject m_PlayerHostLayout;
	    
        [Header("UI Elements")]
        public GameObject m_ScrollContent;
		
        [Header("Prefabs")]
        public GameObject m_ServerPrefab;
		
        [Header("Tank Renderers")]
        public GameObject m_JoinTankRender;
        public GameObject m_CreateTankRender;
        
        
        private readonly List<GameObject> instances = new List<GameObject>();
        private readonly List<GameObject> layouts = new List<GameObject>();
        
        private void Start() {
	        layouts.Add(m_MenuLayout);
	        layouts.Add(m_JoinLayout);
	        layouts.Add(m_PlayerHostLayout);

	        foreach (var layout in layouts) {
		        layout.SetActive(layout.Equals(m_MenuLayout));
	        }
	        
	        m_JoinTankRender.SetActive(true);
	        m_CreateTankRender.SetActive(false);
        }

        public void NuevoServidor(){
            discoveredServers.Clear();
            NetworkManager.singleton.StartServer();
            networkDiscovery.AdvertiseServer();
        }
        public void NuevaPartida(){
            foreach (var layout in layouts) {
	            layout.SetActive(layout.Equals(m_PlayerHostLayout));
            }
            m_CreateTankRender.SetActive(true);
            m_JoinTankRender.SetActive(false);
        }
        public void UnirsePartida(){
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            foreach (var layout in layouts) {
	            layout.SetActive(layout.Equals(m_JoinLayout));
            }
        }

        public void Return() {
	        instances.ForEach(Destroy);
	        instances.Clear();
	        discoveredServers.Clear();
	        foreach (var layout in layouts) {
		        layout.SetActive(layout.Equals(m_MenuLayout));
	        }
	        m_CreateTankRender.SetActive(false);
	        m_JoinTankRender.SetActive(true);
        }

        public void StartHost() {
	        discoveredServers.Clear();
	        NetworkManager.singleton.StartHost();
	        networkDiscovery.AdvertiseServer();
        }

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

