using System;
using UnityEngine;

namespace UI {
	public class UIToCamera : MonoBehaviour {
		private UnityEngine.Camera m_Camera;
		private void Start() {
			m_Camera = FindObjectOfType<UnityEngine.Camera>();
		}

		private void Update() {
			transform.LookAt(m_Camera.transform);
		}
	}
}