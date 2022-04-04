using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Complete;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers {
	public class GameNetworkManager : NetworkManager {
		public GameManager m_GameManager;
		public struct SpawnTank : NetworkMessage {
			//TODO: add player Name and Color
		}

		public override void OnStartServer() {
			base.OnStartServer();
			Debug.Log("Start Server");
			m_GameManager.ResetRoundNum();
			NetworkServer.RegisterHandler<SpawnTank>(OnTankSpawn);
		}

		public override void OnClientConnect() {
			base.OnClientConnect();
			m_GameManager.gameObject.SetActive(true);
			Debug.Log("Client Connect");
			NetworkClient.Send(new SpawnTank());
		}

		private void OnTankSpawn(NetworkConnectionToClient conn, SpawnTank message) {
			var spawnLoc = GetStartPosition();
			var _instance = Instantiate(playerPrefab, spawnLoc.position, spawnLoc.rotation);
			var tankManager = new TankManager() {
				m_Instance = _instance,
				m_PlayerID = conn.connectionId,
				m_SpawnPoint = spawnLoc
				//TODO: m_PlayerName = message.playerName,
				//TODO: m_PlayerColor = message.playerColor
			};
			//UpdateCameraTargets();
			NetworkServer.AddPlayerForConnection(conn, _instance);
			//Debug.Log($"Count NetworkConnections OnTankSpawn: {NetworkServer.connections.Count}");
			m_GameManager.DoUpdate();
		}
		public override void OnServerDisconnect(NetworkConnectionToClient conn) {
			//UpdateCameraTargets();
			base.OnServerDisconnect(conn);
			m_GameManager.DoUpdate();
			if (!conn.identity.isServer) {
				m_GameManager.StopAllCoroutines();
			}
		}

		public override void OnStopServer() {
			m_GameManager.m_Tanks.Clear();
			//m_GameManager.m_CameraControl.m_Targets.Clear();
			m_GameManager.StopAllCoroutines();
			m_GameManager.ResetRoundNum();
			//SceneManager.LoadScene(offlineScene);
		}
	}
}