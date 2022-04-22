using System;
using System.Collections.Generic;
using Managers;
using Network;
using UnityEngine;

namespace UI {
	public class ApplyColor : MonoBehaviour {
		public TankAuthenticator m_Authenticator;
		public FlexibleColorPicker m_ColorPicker;
		[Header("Renderers")] public List<GameObject> m_Tanks = new List<GameObject>();

		private void Awake() {
			foreach (var tank in m_Tanks) {
				ApplyColorOnRenderer(tank, Color.green);
			}
			m_ColorPicker.OnColorChange += ColorPickerOnOnColorChange;
		}

		private void ColorPickerOnOnColorChange(object sender, EventArgs e) {
			foreach (var tank in m_Tanks) {
				ApplyColorOnRenderer(tank, m_ColorPicker.color);
			}
			m_Authenticator.SetColor(m_ColorPicker.color);
		}

		private void ApplyColorOnRenderer(GameObject tank, Color color) {
			var tankRenderers = tank.GetComponentsInChildren<MeshRenderer>();
			foreach (var meshRenderer in tankRenderers) {
				meshRenderer.material.color = color;
			}
		}
	}
}