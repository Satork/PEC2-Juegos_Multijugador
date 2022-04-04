using System.Linq;
using Mirror;
using UnityEngine;

namespace Camera
{
    public class CameraControl : NetworkBehaviour
    {
        public float m_DampTime = 0.2f;                 // Approximate time for the camera to refocus
        public float m_ScreenEdgeBuffer = 4f;           // Space between the top/bottom most target and the screen edge
        public float m_MinSize = 6.5f;                  // The smallest orthographic size the camera can be
        
        public readonly SyncList<Transform> m_Targets = new SyncList<Transform>();		// All the targets the camera needs to encompass


        private UnityEngine.Camera m_Camera;                        // Used for referencing the camera
        private float m_ZoomSpeed;                      // Reference speed for the smooth damping of the orthographic size
        private Vector3 m_MoveVelocity;                 // Reference velocity for the smooth damping of the position
        private Vector3 m_DesiredPosition;              // The position the camera is moving towards
        

        private void Awake()
        {
	        m_Camera = GetComponentInChildren<UnityEngine.Camera> ();
        }

        private void FixedUpdate() {
	        
	        // Move the camera towards a desired position
            Move();

            // Change the size of the camera based
            Zoom();
        }
        


        private void Move()
        {
            // Find the average position of the targets
            FindAveragePosition();

            // Smoothly transition to that position
            transform.position = Vector3.SmoothDamp (transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
        }


        private void FindAveragePosition()
        {

	        var averagePos = new Vector3();
            var numTargets = 0;

            // Go through all the targets and add their positions together
            if (m_Targets.Count > 0) {
	            foreach (var target in m_Targets.Where(target => target.gameObject.activeSelf)) {
		            // Add to the average and increment the number of targets in the average
		            averagePos += target.position;
		            numTargets++;
	            }
            }

            // If there are targets divide the sum of the positions by the number of them to find the average
            if (numTargets > 0) averagePos /= numTargets;

            // Keep the same y value
            averagePos.y = transform.position.y;

            // The desired position is the average position
            m_DesiredPosition = averagePos;
        }


        private void Zoom()
        {
            // Find the required size based on the desired position and smoothly transition to that size
            var requiredSize = FindRequiredSize();
            var orthographicSize = Mathf.SmoothDamp (m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
            //RpcSyncCameraWithClients(orthographicSize);
            m_Camera.orthographicSize = orthographicSize;
        }


        private float FindRequiredSize ()
        {
            // Find the position the camera rig is moving towards in its local space
            var desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);

            // Start the camera's size calculation at zero
            var size = 0f;
            if (m_Targets.Count > 0) {
	            // Go through all the targets...
	            foreach (var desiredPosToTarget in from target in m_Targets where target.gameObject.activeSelf select transform.InverseTransformPoint(target.position) into targetLocalPos select targetLocalPos - desiredLocalPos) {
		            // Choose the largest out of the current size and the distance of the tank 'up' or 'down' from the camera
		            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));

		            // Choose the largest out of the current size and the calculated size based on the tank being to the left or right of the camera
		            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
	            }
            }

            // Add the edge buffer to the size
            size += m_ScreenEdgeBuffer;

            // Make sure the camera's size isn't below the minimum
            size = Mathf.Max (size, m_MinSize);

            return size;
        }


        public void SetStartPositionAndSize()
        {
            // Find the desired position
            FindAveragePosition ();
            
            // Set the camera's position to the desired position without damping
            transform.position = m_DesiredPosition;

            // Find and set the required size of the camera
            var orthographicSize = FindRequiredSize();
            //RpcSyncCameraWithClients(orthographicSize);
            m_Camera.orthographicSize = orthographicSize;
        }

        public void ResetPosition() {
	        m_DesiredPosition = Vector3.zero;
        }
    }
}