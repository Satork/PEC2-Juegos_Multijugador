using System.Collections.Generic;
using System.Linq;
using Mirror;
using Network;
using TMPro;
using UnityEngine;

namespace Tank {
	public class PlayerTank : NetworkBehaviour {

		public static readonly HashSet<string> playersNames = new HashSet<string>();

		public TextMeshProUGUI m_PlayerNameLabel;

		[SyncVar(hook = nameof(OnPlayerNameChange))] public string m_PlayerName;
		[SyncVar(hook = nameof(OnPlayerColorChange))] public Color m_PlayerColor;
		[SyncVar(hook = nameof(OnDefaultColorUse))]public bool m_UseDefaultColors;
		
		
		public static PlayerTank instance;
		

		private void Awake() {
			instance = this;
		}

		private void OnPlayerNameChange(string _, string newName) {
			SetPlayerName();
		}

		private void OnPlayerColorChange(Color _, Color newColor) {
			SetOriginalColor();
		}

		private void OnDefaultColorUse(bool _, bool newBool) {
			if (newBool) { 
				SetDefaultColors();
			}
		}
		
		public override void OnStartClient() {
			Debug.Log("Client Started");
			if (!m_UseDefaultColors) return;
			SetDefaultColors();
		}

		public override void OnStopClient() {
			if (!m_UseDefaultColors) {
				SetOriginalColor();
			}
		}

		/*public override void OnStartServer() {
			var authData = (TankAuthenticator.AuthRequestMessage)connectionToClient.authenticationData;
			m_PlayerName = authData.authUsername;
			m_PlayerColor = authData.authColor;
			m_UseDefaultColors = authData.useDefaultColors;
			
			Debug.Log("Server Started");
		}*/

		public void SetDefaultColors() {
			Debug.Log("Setting Colors");
			var targets = FindObjectsOfType<PlayerTank>().ToList();
			Debug.Log($"Targets: {targets.Count}");
			foreach (var target in targets) {
				var color = target.isLocalPlayer ? Color.blue : Color.red;
				var renderers = target.GetComponentsInChildren<MeshRenderer>();

				foreach (var meshRenderer in renderers) {
					meshRenderer.material.color = color;
				}
			}
		}

		public void SetOriginalColor() {
			var renderers = GetComponentsInChildren<MeshRenderer> ();
			foreach (var meshRenderer in renderers) {
				meshRenderer.material.color = m_PlayerColor;
			}
		}
		public void SetPlayerName() {
			m_PlayerNameLabel.text = m_PlayerName;
		}
	}
}