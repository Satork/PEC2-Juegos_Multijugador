using System;
using System.Collections;
using System.Collections.Generic;
using Camera;
using Mirror;
using Tank;
using UnityEngine;
using UnityEngine.UI;

namespace Managers {
	public class GameManager : NetworkBehaviour {
		
		public float m_StartDelay = 3f;
		//public float m_EndDelay = 3f;

		
		public Text m_Message;

		private WaitForSeconds m_StartWait;
		//private WaitForSeconds m_EndWait;

		private void OnEnable() {
			m_StartWait = new WaitForSeconds(m_StartDelay);
			//m_EndWait = new WaitForSeconds(m_EndDelay);
			StartCoroutine(GameLoop());
		}

		private IEnumerator GameLoop() {
			yield return StartCoroutine(RoundStart());

			yield return StartCoroutine(RoundPlay());
		}

		private IEnumerator RoundStart() {
			m_Message.text = "Round Starting";
			yield return m_StartWait;
		}
		
		private IEnumerator RoundPlay() {
			m_Message.text = string.Empty;
			while (true) {
				yield return null;
			}
		}
	}
}