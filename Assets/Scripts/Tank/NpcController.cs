using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Complete;
using Mirror;
using TMPro;
using UnityEngine;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Tank {
	public class NpcController : NetworkBehaviour {
		public TextMeshProUGUI m_TankNameLabel;

		public float m_AttackDistance;
		public float m_Cadence = .8f;

		[HideInInspector, SyncVar(hook = nameof(OnNpcNameChange))] public string m_NpcName;
		[HideInInspector, SyncVar(hook = nameof(OnColorChange))] public Color m_NpcColor;

		private List<MeshRenderer> m_MeshRenderer = new List<MeshRenderer>();
		private TankShooting m_Shooting;
		
		private bool isAttacking;

		public override void OnStartServer() {
			
			m_Shooting = GetComponent<TankShooting>();
			m_MeshRenderer = GetComponentsInChildren<MeshRenderer>().ToList();
			m_NpcColor = Color.yellow;
			m_NpcName = "NPC";
		}

		private void Update() {
			if (isAttacking) return;
			var identities = NetworkServer.connections.Values.ToList().Select(client => client.identity).ToList();
			
			foreach (var identity in identities.Where(identity => identity.TryGetComponent(out PlayerTank _) && CanSeeTarget(identity.transform))) {
				isAttacking = true;
				StartCoroutine(AttackTarget(identity));
			}
		}

		private IEnumerator AttackTarget(NetworkIdentity identity) {
			var target = identity.transform.position - transform.position;
			target.y = 0;

			transform.rotation = Quaternion.LookRotation(target);
			yield return null;
			
			m_Shooting.NpcFire();
			yield return new WaitForSeconds(m_Cadence);

			isAttacking = false;
		}

		private void OnNpcNameChange(string _, string newName) {
			m_TankNameLabel.text = newName;
		}

		private void OnColorChange(Color _, Color newColor) {
			foreach (var meshRenderer in m_MeshRenderer) {
				meshRenderer.material.color = newColor;
			}
		}

		private bool CanSeeTarget(Transform target) {
			var dist = Vector3.Distance(target.position, transform.position);
			if (dist > m_AttackDistance) return false;
			var dir = (target.position - transform.position).normalized;
			return Physics.Raycast(transform.position, dir, dist, -1);
		}

		[Server]
		public void DisableControls() {
			m_Shooting.enabled = false;
		}

		[Server]
		public void EnableControls() {
			m_Shooting.enabled = true;
		}
		
		
		private void OnDrawGizmos() {
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(transform.position, m_AttackDistance);
			Gizmos.color = Color.red;
			var identities = NetworkServer.connections.Values.ToList().Select((client => client.identity)).ToList();
			foreach (var player in identities.Where(identity =>
				identity.TryGetComponent(out PlayerTank _) && CanSeeTarget(identity.transform))) {
				Gizmos.DrawLine(transform.position, player.transform.position);
			}
		}
	}
}