using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Complete;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class GameManager : NetworkManager
    {
        public int m_NumRoundsToWin = 5;            // The number of rounds a single player has to win to win the game
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases
        public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases
        public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc
		//public GameObject m_TankPrefab;             // Reference to the prefab the players will control


        private List<TankManager> m_Tanks;               // A collection of managers for enabling and disabling different aspects of the tanks

        
        private int m_RoundNumber;                  // Which round the game is currently on
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends
        private TankManager m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won
        private TankManager m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won
        private int id;

        public struct SpawnTank : NetworkMessage {
	        public TankManager manager;
        }

        public override void OnStartServer() {
	        base.OnStartServer();
	        
	        NetworkServer.RegisterHandler<SpawnTank>(OnSpawnTank);
        }

        public override void Start() {
            // Create the delays so they only have to be made once
            m_StartWait = new WaitForSeconds (m_StartDelay);
            m_EndWait = new WaitForSeconds (m_EndDelay);

            id = 1;
            m_Tanks = new List<TankManager>();
            //SpawnAllTanks();
            //SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game
        }

        public override void OnServerConnect(NetworkConnectionToClient conn) {
	        StartCoroutine (GameLoop ());
	        base.OnServerConnect(conn);
        }

        public override void OnClientConnect() {
	        base.OnClientConnect();
	        var manager = new TankManager {
		        //TODO: add info when player spawn like name and color
		        m_PlayerNumber = id,
		        m_SpawnPoint = GetStartPosition()
	        };
	        var spawnMessage = new SpawnTank {
				manager = manager
	        };
	        id++;
	        
	        NetworkClient.Send(spawnMessage);
        }

        private void OnSpawnTank(NetworkConnectionToClient conn, SpawnTank message) {

	        if (playerPrefab == null) return;
	        var manager = message.manager;
	        var spawnPoint = manager.m_SpawnPoint;
	        
	        manager.m_Instance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
	        manager.Setup();
	        
	        m_Tanks.Add(manager);
	        UpdateCameraTargets();
	        NetworkServer.AddPlayerForConnection(conn, manager.m_Instance);
        }

        public override void OnClientDisconnect() {
	        try {
		        var manager = NetworkClient.localPlayer.GetComponent<TankManager>();
		        if (manager != null) {
			        m_Tanks.Remove(manager);
			        UpdateCameraTargets();
		        }
	        }
	        catch {
		        // ignored
	        }
        
	        base.OnClientDisconnect();
        }
		
        public override void OnServerDisconnect(NetworkConnectionToClient conn) {
	        m_Tanks.Clear();
	        UpdateCameraTargets();
	        base.OnServerDisconnect(conn);
        }

        private void UpdateCameraTargets() {
	        if (m_Tanks.Count <= 0) {
		        m_CameraControl.m_Targets = null;
		        return;
	        }
	        m_CameraControl.m_Targets = m_Tanks.Select(i => i.m_Instance.transform).ToArray();
        }

        /*[Obsolete]
        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (var i = 0; i < m_Tanks.Count; i++)
            {
                // ... create them, set their player number and references needed for control
                m_Tanks[i].m_Instance =
                    Instantiate (m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation);
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }*/

        /*[Obsolete]
        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks
            var targets = new Transform[m_Tanks.Count];

            // For each of these transforms...
            for (var i = 0; i < targets.Length; i++) {
                // ... set it to the appropriate tank transform
                targets[i] = m_Tanks[i].m_Instance.transform;
            }

            // These are the targets the camera should follow
            m_CameraControl.m_Targets = targets;
        }*/


        // This is called from start and will run each phase of the game one after another
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished
            yield return StartCoroutine (RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished
            yield return StartCoroutine (RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found
            if (m_GameWinner != null)
            {
                // If there is a game winner, restart the level
                SceneManager.LoadScene (0);
            }
            else
            {
                // If there isn't a winner yet, restart this coroutine so the loop continues
                // Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end
                StartCoroutine (GameLoop());
            }
        }

        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move
            ResetAllTanks();
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_StartWait;
        }


        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks
            EnableTankControl();

            // Clear the text from the screen
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
            {
	            // ... return on the next frame
                yield return null;
            }
        }


        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving
            DisableTankControl();

            // Clear the winner from the previous round
            m_RoundWinner = null;

            // See if there is a winner now the round is over
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score
            if (m_RoundWinner != null)
            {
                m_RoundWinner.m_Wins++;
            }

            // Now the winner's score has been incremented, see if someone has one the game
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it
            var message = EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop
            yield return m_EndWait;
        }


        // This is used to check if there is one or fewer tanks remaining and thus the round should end
        private bool OneTankLeft()
        {
            // Skip all rounds logic

            // // Start the count of tanks left at zero.
            // var numTanksLeft = m_Tanks.Count(target => target.m_Instance.activeSelf);
            //
            // // Go through all the tanks...
            //
            // // If there are one or fewer tanks remaining return true, otherwise return false.
            // return numTanksLeft <= 1;

            return false;
        }
        
        
        // This function is to find out if there is a winner of the round
        // This function is called with the assumption that 1 or fewer tanks are currently active
        private TankManager GetRoundWinner() {
	        // Go through all the tanks...
	        return m_Tanks.FirstOrDefault(t => t.m_Instance.activeSelf);

	        // If none of the tanks are active it is a draw so return null
        }


        // This function is to find out if there is a winner of the game
        private TankManager GetGameWinner() {
	        // Go through all the tanks...
	        return m_Tanks.FirstOrDefault(t => t.m_Wins == m_NumRoundsToWin);

	        // If no tanks have enough rounds to win, return null
        }


        // Returns a string message to display at the end of each round
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw
            var message = "DRAW!";

            // If there is a winner then change the message to reflect that
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message
            message = m_Tanks.Aggregate(message, (current, t) => current + (t.m_ColoredPlayerText + ": " + t.m_Wins + " WINS\n"));

            // If there is a game winner, change the entire message to reflect that
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties
        private void ResetAllTanks() {
	        foreach (var tank in m_Tanks) tank.Reset();
        }


        private void EnableTankControl() {
	        foreach (var tank in m_Tanks) tank.EnableControl();
        }


        private void DisableTankControl() {
	        foreach (var tank in m_Tanks) tank.DisableControl();
        }
    }
}