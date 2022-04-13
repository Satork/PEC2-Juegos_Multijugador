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
				player.m_UseDefaultColors = authData.useDefaultColors;
			}

			NetworkServer.AddPlayerForConnection(conn, instance);
			CameraControl.instance.m_Targets.Clear();
			var identities = NetworkServer.connections.Values.Select(client => client.identity).ToList();
			CameraControl.instance.m_Targets.AddRange(identities);
			
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