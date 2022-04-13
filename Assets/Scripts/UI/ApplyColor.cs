using System;
using Managers;
using Network;
using UnityEngine;

namespace UI {
	public class ApplyColor : MonoBehaviour {
		public TankAuthenticator m_Authenticator;
		public FlexibleColorPicker m_ColorPicker;
		public Material m_TankMaterial;
		public Material m_StartTankMaterial;

		private void Start() {
			m_TankMaterial.color = m_StartTankMaterial.color;
			m_ColorPicker.OnColorChange += ColorPickerOnOnColorChange;
		}

		private void ColorPickerOnOnColorChange(object sender, EventArgs e) {
			m_TankMaterial.color = m_ColorPicker.color;
			m_Authenticator.SetColor(m_ColorPicker.color);
		}
	}
}