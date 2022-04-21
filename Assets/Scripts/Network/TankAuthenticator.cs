using System.Collections;
using System.Collections.Generic;
using Mirror;
using Tank;
using UnityEngine;

/*
    Documentation: https://mirror-networking.gitbook.io/docs/components/network-authenticators
    API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkAuthenticator.html
*/

namespace Network {
	public class TankAuthenticator : NetworkAuthenticator {
		private readonly HashSet<NetworkConnection> connectionsPendingDisconnect = new HashSet<NetworkConnection>();

		
		[Header("Player Data")] public string m_PlayerUsername;
		public Color m_PlayerUserColor;

		#region Messages

		public struct AuthRequestMessage : NetworkMessage {
			public string authUsername;
			public Color authColor;
		}

		public struct AuthResponseMessage : NetworkMessage {
			public byte code;
			public string message;
		}

		#endregion

		#region Server

		/// <summary>
		/// Called on server from StartServer to initialize the Authenticator
		/// <para>Server message handlers should be registered in this method.</para>
		/// </summary>
		public override void OnStartServer() {
			// register a handler for the authentication request we expect from client
			NetworkServer.RegisterHandler<AuthRequestMessage>(OnAuthRequestMessage, false);
		}
		
		/// <summary>
		/// Called on server from StopServer to reset the Authenticator
		/// <para>Server message handlers should be registered in this method.</para>
		/// </summary>
		public override void OnStopServer() {
			// unregister the handler for the authentication request
			NetworkServer.UnregisterHandler<AuthRequestMessage>();
		}
		
		/// <summary>
		/// Called on server from OnServerAuthenticateInternal when a client needs to authenticate
		/// </summary>
		/// <param name="conn">Connection to client.</param>
		public override void OnServerAuthenticate(NetworkConnectionToClient conn) {
		}

		/// <summary>
		/// Called on server when the client's AuthRequestMessage arrives
		/// </summary>
		/// <param name="conn">Connection to client.</param>
		/// <param name="msg">The message payload</param>
		public void OnAuthRequestMessage(NetworkConnectionToClient conn, AuthRequestMessage msg) {


			if (!PlayerTank.playersNames.Contains(msg.authUsername)) {

				PlayerTank.playersNames.Add(msg.authUsername);
				
				conn.authenticationData = msg;

				var authResponseMessage = new AuthResponseMessage {
					code = 100,
					message = "Success"
				};

				conn.Send(authResponseMessage);

				// Accept the successful authentication
				ServerAccept(conn);
			}
			else {
				connectionsPendingDisconnect.Add(conn);

				// create and send msg to client so it knows to disconnect
				var authResponseMessage = new AuthResponseMessage {
					code = 200,
					message = "Username already in use...try again"
				};

				conn.Send(authResponseMessage);

				// must set NetworkConnection isAuthenticated = false
				conn.isAuthenticated = false;

				// disconnect the client after 1 second so that response message gets delivered
				StartCoroutine(DelayedDisconnect(conn, 1f));
			}
		}

		IEnumerator DelayedDisconnect(NetworkConnectionToClient conn, float waitTime)
		{
			yield return new WaitForSeconds(waitTime);

			// Reject the unsuccessful authentication
			ServerReject(conn);

			yield return null;

			// remove conn from pending connections
			connectionsPendingDisconnect.Remove(conn);
		}
		
		#endregion

		#region Client

		public void SetUsername(string username) {
			m_PlayerUsername = username;
		}

		public void SetColor(Color color) {
			m_PlayerUserColor = color;
		}
		
		/// <summary>
		/// Called on client from StartClient to initialize the Authenticator
		/// <para>Client message handlers should be registered in this method.</para>
		/// </summary>
		public override void OnStartClient() {
			// register a handler for the authentication response we expect from server
			NetworkClient.RegisterHandler<AuthResponseMessage>(OnAuthResponseMessage, false);
		}

		/// <summary>
		/// Called on client from OnClientAuthenticateInternal when a client needs to authenticate
		/// </summary>
		public override void OnClientAuthenticate() {
			var authRequestMessage = new AuthRequestMessage {
				authUsername = m_PlayerUsername,
				authColor = m_PlayerUserColor,
			};

			NetworkClient.connection.Send(authRequestMessage);
		}

		/// <summary>
		/// Called on client when the server's AuthResponseMessage arrives
		/// </summary>
		/// <param name="msg">The message payload</param>
		public void OnAuthResponseMessage(AuthResponseMessage msg) {
			if (msg.code == 100) {
				Debug.Log($"Auth response: {msg.message}");
				// Authentication has been accepted
				ClientAccept();
			}
			else {
				Debug.LogError($"Auth Response: {msg.message}");
				NetworkManager.singleton.StopHost();
			}
		}

		#endregion
	}
}