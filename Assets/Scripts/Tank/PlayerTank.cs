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

		[SyncVar(hook = nameof(OnPlayerNameChange))] public string playerName;
		[SyncVar(hook = nameof(OnPlayerColorChange))] public Color playerColor;
		[SyncVar]public bool useDefaultColors;

		private void OnPlayerNameChange(string _, string newName) {
			m_PlayerNameLabel.text = playerName;
		}

		private void OnPlayerColorChange(Color _, Color newColor) {
			if (useDefaultColors) return;
			// Get all of the renderers of the tank
			var renderers = GetComponentsInChildren<MeshRenderer> ();

			// Go through all the renderers...
			foreach (var meshRenderer in renderers) {
				// ... set their material color to the color specific to this tank
				meshRenderer.material.color = playerColor;
			}
		}

		private void OnUseDefaultColors(bool _, bool newBool) {
			var targets = NetworkServer.connections.Select(conn => conn.Value.identity).ToList();
			foreach (var target in targets) {
				var color = target.isLocalPlayer ? Color.blue : Color.red;
				var renderers = target.GetComponentsInChildren<MeshRenderer>();

				foreach (var meshRenderer in renderers) {
					meshRenderer.material.color = color;
				}
			}
		}
		
		public override void OnStartServer() {
			var authData = (TankAuthenticator.AuthRequestMessage)connectionToClient.authenticationData;
			playerName = authData.authUsername;
			useDefaultColors = authData.useDefaultColors;
			if (!useDefaultColors) {
				playerColor = authData.authColor;
			}
		}
	}
}