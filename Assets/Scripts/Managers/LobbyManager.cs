using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.Discovery;
using Network;
using TMPro;
using UnityEngine.UI;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
	    private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

        public NetworkDiscovery m_NetworkDiscovery;
        
        [Header("Layouts")]
        public GameObject m_MenuLayout;
        public GameObject m_JoinLayout;
        public GameObject m_PlayerHostLayout;
        public GameObject m_ServerSettingsLayout;
	    
        [Header("UI Elements")]
        public GameObject m_ScrollContent;
		
        [Header("Prefabs")]
        public GameObject m_ServerPrefab;
		
        [Header("Tank Renderers")]
        public GameObject m_JoinTankRender;
        public GameObject m_CreateTankRender;

        [Header("Settings")] public Button m_ServerStartButton;
        
        [Header("Npcs")]
        public int m_NumOfNpcs;
        public GameObject m_NpcsIconsLayout;

        [Header("Rounds")] 
        public int m_NumOfRounds;
        public TextMeshProUGUI m_RoundsLabel;

        private const int _MAX_NPCS = 4;

        private GameObject currentTab;
        private GameObject previousTab;
        
        private readonly List<GameObject> instances = new List<GameObject>();
        private readonly List<GameObject> layouts = new List<GameObject>();
        private readonly List<GameObject> npcIcons = new List<GameObject>();

        public bool UseDefaultColors {
	        get => ServerData.useDefaultColors;
	        set => ServerData.useDefaultColors = value;
        }

        private void Start() {
	        foreach (Transform child in m_NpcsIconsLayout.transform) {
		        npcIcons.Add(child.gameObject);
		        child.gameObject.SetActive(false);
	        }

	        layouts.Add(m_MenuLayout);
	        layouts.Add(m_JoinLayout);
	        layouts.Add(m_PlayerHostLayout);
	        layouts.Add(m_ServerSettingsLayout);

	        currentTab = m_MenuLayout;
	        previousTab = currentTab;

	        foreach (var layout in layouts) {
		        layout.SetActive(layout.Equals(currentTab));
	        }

	        OnNpcNumberChange();
	        OnRoundNumChange();
	        
	        m_JoinTankRender.SetActive(true);
	        m_CreateTankRender.SetActive(false);
        }

        public void NewServer(){
	        OpenSettings();
        }

        public void StartServer() {
	        discoveredServers.Clear();
	        NetworkManager.singleton.StartServer();
	        m_NetworkDiscovery.AdvertiseServer();

        }
        public void NewGame(){
            ActivateLayout(m_PlayerHostLayout);
            m_CreateTankRender.SetActive(true);
            m_JoinTankRender.SetActive(false);
        }
        public void JoinGame(){
            discoveredServers.Clear();
            m_NetworkDiscovery.StartDiscovery();
            ActivateLayout(m_JoinLayout);
        }
        
        public void StartHost() {
	        discoveredServers.Clear();
	        NetworkManager.singleton.StartHost();
	        m_NetworkDiscovery.AdvertiseServer();
        }

        public void ReturnMainMenu() {
	        previousTab = m_MenuLayout;
	        instances.ForEach(Destroy);
	        instances.Clear();
	        discoveredServers.Clear();
	        ActivateLayout(previousTab);
	        m_CreateTankRender.SetActive(false);
	        m_JoinTankRender.SetActive(true);
        }

        public void OpenSettings() {
	        m_ServerStartButton.gameObject.SetActive(currentTab == m_MenuLayout);
	        previousTab = currentTab;
	        ActivateLayout(m_ServerSettingsLayout);
	        m_CreateTankRender.SetActive(false);
	        m_JoinTankRender.SetActive(false);
        }

        public void ReturnSettings() {
	        ActivateLayout(previousTab);
	        if (previousTab.Equals(m_MenuLayout)) {
		        m_CreateTankRender.SetActive(false);
		        m_JoinTankRender.SetActive(true);
	        }
	        else {
		        m_CreateTankRender.SetActive(true);
		        m_JoinTankRender.SetActive(false);
	        }
        }

        public void AddNpc() {
	        m_NumOfNpcs++;
	        if (m_NumOfNpcs > _MAX_NPCS) m_NumOfNpcs = _MAX_NPCS;
			OnNpcNumberChange();
        }

        public void SubtractNpc() {
	        m_NumOfNpcs--;
	        if (m_NumOfNpcs < 0) m_NumOfNpcs = 0;
	        OnNpcNumberChange();
        }

        private void OnNpcNumberChange() {
	        RenderNpcIcons();
	        ServerData.numOfNpcs = m_NumOfNpcs;
        }
        
        private void RenderNpcIcons() {
	        var index = 0;
	        foreach (var icon in npcIcons) {
		        icon.SetActive(index < m_NumOfNpcs);
		        index++;
	        }
        }

        public void AddRound() {
	        m_NumOfRounds++;
	        OnRoundNumChange();
        }

        public void SubtractRound() {
	        m_NumOfRounds--;
	        if (m_NumOfRounds < 0) m_NumOfRounds = 0;
	        OnRoundNumChange();
        }

        private void OnRoundNumChange() {
	        m_RoundsLabel.text = m_NumOfRounds == 0 ? "<size=100>∞" : $"{m_NumOfRounds}";
	        ServerData.numOfRounds = m_NumOfRounds;
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

        private void Connect(ServerResponse info)
        {
            m_NetworkDiscovery.StopDiscovery();
            NetworkManager.singleton.StartClient(info.uri);
        }

        private void ActivateLayout(GameObject tab) {
	        foreach (var layout in layouts) {
		        layout.SetActive(layout.Equals(tab));
	        }

	        currentTab = tab;
        }
    }

}

