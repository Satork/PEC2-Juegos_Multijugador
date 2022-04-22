using Camera;
using Mirror;
using Network;
using Tank;
using UnityEngine;

namespace Managers {
	public class NpcManager : NetworkBehaviour {
		public GameObject m_NpcPrefab;

		public static NpcManager instance;

		private void Start() {
			instance = this;
		}

		public override void OnStartServer() {
			SpawnNpcs(ServerData.numOfNpcs);
		}
		
		[Server]
		public void SpawnNpcs(int num){
			for (var i = 0; i < num; i++) {
				var randSpawnPos =
					new Vector3(Random.Range(-40, 41), 3, Random.Range(-40, 41));
				var instanceGO = Instantiate(m_NpcPrefab, randSpawnPos, Quaternion.identity);
				NetworkServer.Spawn(instanceGO);
				//spawnedNpcs.Add(instance.GetComponent<NetworkIdentity>());
				CameraControl.instance.m_Targets.Add(instanceGO.GetComponent<NetworkIdentity>());
				GameManager.instance.m_Npcs.Add(instanceGO.GetComponent<NpcController>());
			}
		}
	}
}