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
		[Range(0, 4)] public int cantidadEnemigos;

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
			var tanques =  NetworkServer.spawned.Values.ToList();
			foreach (var item in tanques)
			{
				if( item.ToString().Contains("Clone"))
				{
					CameraControl.instance.m_Targets.Add(item);
				}
			}

			if(!npcCreated){
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
			for (int i=0; i<cantidadEnemigos; i++){
				Vector3 spawnAleatorio = new Vector3(UnityEngine.Random.Range(-10,11), 0, UnityEngine.Random.Range(-10,11));
				var instance = Instantiate(npcPrefab, spawnAleatorio, Quaternion.identity);
            	NetworkServer.Spawn(instance);
				if (instance.TryGetComponent(out PlayerTank npc1)) {
					CameraControl.instance.m_Targets.Add(npc1.GetComponent<NetworkIdentity>());
				}
			}
			
			
		}
	}
}