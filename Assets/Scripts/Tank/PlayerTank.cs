using System;
using System.Collections.Generic;
using System.Linq;
using Complete;
using Managers;
using Mirror;
using Network;
using Telepathy;
using TMPro;
using UnityEngine;
// ReSharper disable Unity.InefficientPropertyAccess

namespace Tank {
	public class PlayerTank : NetworkBehaviour {

		public static readonly HashSet<string> playersNames = new HashSet<string>();

		public TextMeshProUGUI m_PlayerNameLabel;
		
		// Tank references
		private TankMovement m_Movement;
		private TankShooting m_Shooting;
		
		[SyncVar(hook = nameof(OnPlayerNameChange))] public string m_PlayerName;
		[SyncVar(hook = nameof(OnPlayerColorChange))] public Color m_PlayerColor;
		[SyncVar(hook = nameof(OnDefaultColorUse))]public bool m_UseDefaultColors;
		[SyncVar(hook = nameof(OnWin))] public int m_Wins; 
		[HideInInspector] public string m_PlayerColoredName;

		public static PlayerTank instance;
		

		private void Awake() {
			instance = this;
		}

		private void Start() {
			m_Movement = GetComponent<TankMovement>();
			m_Shooting = GetComponent<TankShooting>();
		}

		private void OnPlayerNameChange(string _, string newName) {
			SetPlayerName();
			SetColoredName();
		}
		
		private void OnPlayerColorChange(Color _, Color newColor) {
			SetOriginalColor();
			SetColoredName();
		}

		private void OnDefaultColorUse(bool _, bool newBool) {
			if (newBool) { 
				SetDefaultColors();
			}
		}
		
		private void OnWin(int _, int win) {
			m_Wins = win;
		}
		
		public override void OnStartClient() {
			if (!m_UseDefaultColors) return;
			SetDefaultColors();
		}

		public override void OnStopClient() {
			if (!m_UseDefaultColors) {
				SetOriginalColor();
			}
		}

		public override void OnStartServer() {
			var authData = (TankAuthenticator.AuthRequestMessage)connectionToClient.authenticationData;
			m_PlayerName = authData.authUsername;
			m_UseDefaultColors = ServerData.useDefaultColors;
			
			//GameManager.instance.m_Tanks.Add(this);
			//Debug.Log("Server Started");
		}

		public void SetDefaultColors() {
			//Debug.Log("Setting Colors");
			var targets = FindObjectsOfType<PlayerTank>().ToList();
			//Debug.Log($"Targets: {targets.Count}");
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
		
		private void SetColoredName() {
			m_PlayerColoredName = $"<color=#{ColorUtility.ToHtmlStringRGB(m_PlayerColor)}>{m_PlayerName}</color>";
		}
		
		[ClientRpc]
		public void DisableControls() {
			m_Movement.enabled = false;
			m_Shooting.enabled = false;
		}
		
		[ClientRpc]
		public void EnableControls() {
			m_Movement.enabled = true;
			m_Shooting.enabled = true;
		}
		
		[ClientRpc]
		public void ResetPosition(Vector3 pos, Quaternion rot) {
			transform.position = pos;
			transform.rotation = rot;
			gameObject.SetActive(false);
			gameObject.SetActive(true);
		}
	}
}