using System.Collections.Generic;
using System.Linq;
using Camera;
using Mirror;
using Network;
using Tank;
using UnityEngine;

namespace Managers {
	public class GameNetworkManager : NetworkManager {
		public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
			var authData = (TankAuthenticator.AuthRequestMessage)conn.authenticationData;
			var startPos = GetStartPosition();
			var instance = startPos != null
				? Instantiate(playerPrefab, startPos.position, startPos.rotation)
				: Instantiate(playerPrefab);

			if (instance.TryGetComponent(out PlayerTank player)) {
				player.m_PlayerName = authData.authUsername;
				player.m_PlayerColor = authData.authColor;
			}

			NetworkServer.AddPlayerForConnection(conn, instance);
			var players = NetworkServer.connections.Values.ToList()
				.Select(client => client.identity).ToList();

			foreach (var networkIdentity in players) {
				if (CameraControl.instance.m_Targets.Contains(networkIdentity))
					CameraControl.instance.m_Targets.Remove(networkIdentity);
				CameraControl.instance.m_Targets.Add(networkIdentity);
			}
		}

		public override void OnServerDisconnect(NetworkConnectionToClient conn) {
			if (conn.authenticationData != null) {
				var authData = (TankAuthenticator.AuthRequestMessage)conn.authenticationData;
				PlayerTank.playersNames.Remove(authData.authUsername);
			}

			CameraControl.instance.m_Targets.Remove(conn.identity);
			base.OnServerDisconnect(conn);
		}
	}
}