using System.Linq;
using Camera;
using Mirror;
using Network;
using Tank;

namespace Managers {
	public class GameNetworkManager : NetworkManager {

		public override void OnServerAddPlayer(NetworkConnectionToClient conn) {
			var authData = (TankAuthenticator.AuthRequestMessage)conn.authenticationData;
			var startPos = GetStartPosition();
			var instance = startPos != null
				? Instantiate(playerPrefab, startPos.position, startPos.rotation)
				: Instantiate(playerPrefab);

			if (instance.TryGetComponent(out Tank.PlayerTank player)) {
				player.playerName = authData.authUsername;
				player.playerColor = authData.authColor;
			}
			
			NetworkServer.AddPlayerForConnection(conn, instance);
			CameraControl.instance.m_Targets.Clear();
			var identities = NetworkServer.connections.Values.Select(client =>  client.identity);
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