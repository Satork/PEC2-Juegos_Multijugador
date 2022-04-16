using System.Linq;
using Camera;
using Mirror;
using Network;
using Tank;
using UnityEngine;

namespace Managers {
	public class GameNetworkManager : NetworkManager {
        private bool npcCreated = false;
		public GameObject npcPrefab;

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

			if(!npcCreated){
				Debug.Log("EPA");
				CreateNpcs();
			    npcCreated = true;
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
		public void CreateNpcs(){
			Vector3 spawnAleatorio1 = new Vector3(UnityEngine.Random.Range(-10,11), 0, UnityEngine.Random.Range(-10,11));
			var instance1 = Instantiate(npcPrefab, spawnAleatorio1, Quaternion.identity);
            NetworkManager.singleton.spawnPrefabs.Add(instance1);
			if (instance1.TryGetComponent(out PlayerTank npc1)) {
				CameraControl.instance.m_Targets.Add(npc1.GetComponent<NetworkIdentity>());
			}
			
		}
	}
}