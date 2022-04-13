using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

namespace UI {
	public class GameUI : NetworkBehaviour {

		[Header("Layouts")] public GameObject m_ServerLayout;
		public GameObject m_HostLayout;
		public GameObject m_ClientLayout;

		private void Start() {
			var layouts = new List<GameObject> {
				m_ServerLayout,
				m_HostLayout,
				m_ClientLayout
			};

			if (isServerOnly)
				foreach (var layout in layouts)
					layout.SetActive(layout.Equals(m_ServerLayout));
			else if (isClientOnly)
				foreach (var layout in layouts)
					layout.SetActive(layout.Equals(m_ClientLayout));
			else
				foreach (var layout in layouts)
					layout.SetActive(layout.Equals(m_HostLayout));
		}

		public void StopServer() {
			NetworkManager.singleton.StopServer();
		}

		public void StopHost() {
			NetworkManager.singleton.StopHost();
		}

		public void StopClient() {
			NetworkManager.singleton.StopClient();
		}
	}
}