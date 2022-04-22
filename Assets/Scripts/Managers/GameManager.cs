using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Camera;
using Mirror;
using Network;
using Tank;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Managers {
	public class GameManager : NetworkBehaviour {
		[Header("Delays")]
		public float m_StartDelay = 3f;
		public float m_EndDelay = 3f;
		
		[Header("UI")]
		public Text m_Message;
		
		[SyncVar(hook = nameof(SyncNumOfRounds))] private int m_NumOfRounds;
		[SyncVar(hook = nameof(SyncCurrentRound))] private int m_CurrentRound;
		// ReSharper disable once NotAccessedField.Local
		[SyncVar(hook = nameof(OnMessageChange))] private string message;

		public readonly List<PlayerTank> m_Tanks = new List<PlayerTank>();
		public readonly List<NpcController> m_Npcs = new List<NpcController>();

		private PlayerTank m_RoundWinner;
		private PlayerTank m_GameWinner;
		
		private WaitForSeconds m_StartWait;
		private WaitForSeconds m_EndWait;

		public static GameManager instance;

		private void Awake() {
			instance = this;
		}

		private void SyncNumOfRounds(int _, int num) {
			m_NumOfRounds = num;
		}

		private void SyncCurrentRound(int _, int num) {
			m_CurrentRound = num;
		}

		private void OnMessageChange(string _, string msg) {
			m_Message.text = msg;
		}

		public override void OnStartServer() {
			m_NumOfRounds = ServerData.numOfRounds;

			m_StartWait = new WaitForSeconds(m_StartDelay);
			m_EndWait = new WaitForSeconds(m_EndDelay);

			StartCoroutine(GameLoop());
		}

		private IEnumerator GameLoop() {
			yield return null;
			yield return StartCoroutine(RoundStart());

			yield return StartCoroutine(RoundPlay());

			yield return StartCoroutine(RoundEnding());

			if (m_GameWinner != null) {
				RpcDisconnectClients();
				NetworkManager.singleton.StopServer();
			}
			else
				StartCoroutine(GameLoop());
		}
		private IEnumerator RoundStart() {
			if (m_CurrentRound > 0) {
				ResetTanks();
				ReSpawnNpcs();
			}
			
			RpcDisableTankControls();
			ServerDisableNpcControls();
			
			m_CurrentRound++;
			message = $"Round {m_CurrentRound}\nStarting";
			
			yield return m_StartWait;
		}
		private IEnumerator RoundPlay() {
			RpcEnableTankControls();
			ServerEnableNpcControls();
			
			message = string.Empty;
			
			while (!OneTankLeft()) {
				yield return null;
			}
		}
		private IEnumerator RoundEnding() {
			
			RpcDisableTankControls();
			yield return null;
			ServerDisableNpcControls();
			yield return null;
			
			m_RoundWinner = null;
			
			m_RoundWinner = GetRoundWinner();

			if (m_RoundWinner != null) m_RoundWinner.m_Wins++;

			m_GameWinner = GetGameWinner();

			message = EndMessage();

			yield return m_EndWait;
		}

		private string EndMessage() {
			var msg = "DRAW!";

			if (m_RoundWinner != null) msg = $"{m_RoundWinner.m_PlayerColoredName} WINS THE ROUND!";

			msg += "\n\n\n\n";

			msg = m_Tanks.Aggregate(msg,
				(current, tank) => $"{current} {tank.m_PlayerColoredName}: {tank.m_Wins} WINS\n");

			if (m_GameWinner != null) msg = $"{m_GameWinner.m_PlayerColoredName} WINS THE GAME!";

			return msg;
		}

		private bool OneTankLeft() {
			if (m_NumOfRounds == 0)
				return false;

			var tanks = m_Tanks.Count(playerTank => playerTank.gameObject.activeSelf);
			if (m_Tanks.Count > 1)
				return tanks <= 1;
			
			return false;
		}

		private PlayerTank GetRoundWinner() {
			if (!OneTankLeft())
				return null;
			var tank = m_Tanks.FirstOrDefault(playerTank => playerTank.gameObject.activeSelf);
			return tank;
		}

		private PlayerTank GetGameWinner() {
			return m_NumOfRounds == 0 ? null : m_Tanks.FirstOrDefault(tank => tank.m_Wins == m_NumOfRounds);
		}
		
		[ClientRpc]
		private void RpcDisconnectClients() {
			NetworkClient.Disconnect();
			NetworkClient.Shutdown();
		}
		
		private void ResetTanks() {
			var spawnPos = FindObjectsOfType<NetworkStartPosition>().Select(position => position.transform).ToList();
			var randIndex = Random.Range(0, spawnPos.Count);
			spawnPos.ForEach(Debug.Log);
			
			foreach (var playerTank in m_Tanks) {
				playerTank.gameObject.SetActive(true);
				playerTank.ResetPosition(spawnPos[randIndex].position, spawnPos[randIndex].rotation);
				randIndex = (randIndex + 1) % spawnPos.Count;
			}
		}

		[ServerCallback]
		private void ReSpawnNpcs() {
			foreach (var npc in m_Npcs) {
				CameraControl.instance.m_Targets.Remove(npc.netIdentity);
				NetworkServer.Destroy(npc.gameObject);
			}
			m_Npcs.Clear();
			NpcManager.instance.SpawnNpcs(ServerData.numOfNpcs);
		}

		[ClientRpc]
		private void RpcDisableTankControls() {
			foreach (var playerTank in m_Tanks) playerTank.DisableControls();
		}

		[ClientRpc]
		private void RpcEnableTankControls() {
			foreach (var playerTank in m_Tanks) playerTank.EnableControls();
		}

		[Server]
		private void ServerDisableNpcControls() {
			foreach (var npc in m_Npcs) npc.DisableControls();
		}
		
		[Server]
		private void ServerEnableNpcControls() {
			foreach (var npc in m_Npcs) npc.EnableControls();
		}
	}
}