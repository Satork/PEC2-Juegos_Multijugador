using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Complete
{
    public class TankShooting : NetworkBehaviour
    {
        public Rigidbody m_Shell;                   // Prefab of the shell
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held

        private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released
        private InputAction m_FireAction;           // Fire Action reference (Unity 2020 New Input System)
        private bool isDisabled;					// To avoid enabling / disabling Input System when tank is destroyed

        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_MinLaunchForce;

            isDisabled = false;
        }

        private void OnDisable() => isDisabled = true;

        private void Start() {
	        // Unity 2020 New Input System
            // Get a reference to the EventSystem for this player
            var ev = GameObject.Find ("EventSystem").GetComponent<EventSystem>();

            // Find the Action Map for the Tank actions and enable it
            var playerActionMap = ev.GetComponent<PlayerInput>().actions.FindActionMap ("Tank");
            playerActionMap.Enable();

            // Find the 'Fire' action
            m_FireAction = playerActionMap.FindAction ("Fire");

            m_FireAction.Enable();
            m_FireAction.performed += OnFire;
        }


        // Event called when this player's 'Fire' action is triggered by the New Input System
        public void OnFire(InputAction.CallbackContext obj) {
	        if (isDisabled || !isLocalPlayer) return;
	        // When the value read is higher than the default Button Press Point, the key has been pressed
	        if (obj.ReadValue<float>() >= InputSystem.settings.defaultButtonPressPoint) CmdFire();
        }
        
        [Server]
        public void NpcFire() {
	        //Debug.Log("Fire!");
		    
	        // Create an instance of the shell and store a reference to it's rigidbody
	        var shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);

	        // Set the shell's velocity to the launch force in the fire position's forward direction
	        shellInstance.AddForce(m_CurrentLaunchForce * m_FireTransform.forward, ForceMode.Impulse);

	        // Change the clip to the firing clip and play it
	        m_ShootingAudio.clip = m_FireClip;
	        m_ShootingAudio.Play();

	        // Reset the launch force.  This is a precaution in case of missing button events
	        m_CurrentLaunchForce = m_MinLaunchForce;
            
	        NetworkServer.Spawn(shellInstance.gameObject);
        }
        
        [Command]
	    public void CmdFire() {
		    //Debug.Log("Fire!");
		    
            // Create an instance of the shell and store a reference to it's rigidbody
            var shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation);

            // Set the shell's velocity to the launch force in the fire position's forward direction
            shellInstance.AddForce(m_CurrentLaunchForce * m_FireTransform.forward, ForceMode.Impulse);

            // Change the clip to the firing clip and play it
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();

            // Reset the launch force.  This is a precaution in case of missing button events
            m_CurrentLaunchForce = m_MinLaunchForce;
            
            NetworkServer.Spawn(shellInstance.gameObject);
        }
    }
}