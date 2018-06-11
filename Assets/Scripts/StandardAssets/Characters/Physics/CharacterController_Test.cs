using UnityEngine;

namespace StandardAssets.Characters.Physics
{
    public class CharacterController_Test : MonoBehaviour
    {
        /*
     * TODO: Additional considerations for physics:
     * 
     * Should do some calculations for slopes like the PhysX implimentation does... Will confirm with Alec/Anthony if we want different behaviour for slopes, maybe a decrease in momentum
     * 
     * Will impliment the ability to autostep, as described in the NVidia PhysX documentation
     * 
     * Add in something like a ceiling height for jumping when there is an ridgid body above the player
     * 
     * Add in a variable to increase the radius of ground checks to avoid the strange grounding issues Alec/Dave described
     * 
     * Find a better way to check ground collision vector Line:21
     * 
     * Impliment strafing?
     */


        #region Public Variables

        [Header("Player")] //Can we use headers in the Unity Project? It would be better
        public float playerHeight = 2f;    
        public float playerRadius = 0.5f;

        [Header("Gravity")]
        public float gravitationalForce = 2f;

        [Header("Movement")]
        public float movementSpeed = 1f;

        [Header("Jump")]    
        public float jumpForce = 6;

    
        [Header("Grounding")]
        public Vector3 groundVector = new Vector3(0, -0.1f, 0); //Testing code.


        #endregion

        #region Private Variables

        private Vector3 m_currentMovementVector;    
        private Vector3 m_movementVector;

        #endregion

        private const string HORIZONTAL_AXIS_NAME = "Horizontal";
        private const string VERTICAL_AXIS_NAME = "Vertical";

        // Update is called once per frame
        void Update ()
        {
            //Debug.Log(string.Format("Player Grounded State: {0}", isGrounded.ToString()));

            Gravity(); //Always check Gravity first since its a passive force.
            CheckIfGrounded();
            SimpleMove();
            Move();
            Jump();

        
            //TODO: A collision check probably, so I don't jump into solid objects

        }

        /// <summary>
        /// Idicator for the current character being on the damn ground
        /// </summary>
        public bool isGrounded { get; set; } 

        #region Gravitational Force

        private void Gravity()
        {
            //TODO: maybe add a modifier for gravity affecting grounded state (i.e. So the player can have simulated weight)
            if(!isGrounded)
            {
                m_currentMovementVector.y -= gravitationalForce;            
            }
        }

        private void CheckIfGrounded()
        {
            Ray groundingRay = new Ray(transform.TransformPoint(new Vector3(0, 1.2f, 0)), Vector3.down);
            RaycastHit groundHit = new RaycastHit();
            isGrounded = false;
            if(UnityEngine.Physics.SphereCast(groundingRay, 0.17f, out groundHit))
            {
                ConfirmGrounded(groundHit);
            }
            else
            {
                isGrounded = false;
            }
        }

        private void ConfirmGrounded(RaycastHit groundingRay)
        {
            Collider[] colliders = new Collider[3];

            int numberOfHits = UnityEngine.Physics.OverlapSphereNonAlloc(transPoint(groundVector), playerRadius, colliders);

            isGrounded = false; //Not grounded until proven otherwise.

            for(int x =0; x < numberOfHits; x++)
            {
                if(colliders[x].transform == groundingRay.transform)
                {
               
                    isGrounded = true; //Yay we hit the ground.

                    //TODO: Handle some jumping/ landing mechanics here, impliment that landing action in the IPhysics interface.
                }
            }
                
            /* TODO: Check some stuff for falling here, like if we run off the edge of a thing.
         * Considerations:
         * Max acceptable falling distance
         * Check if the player actually meant to jump or if this is a legit fall (should probably indicate some player input for jumping [initiate the jumpatron])
         */

        }

        #endregion

        #region Movement

        /// <summary>
        /// Simple Stupid Movement of the PC
        /// </summary>
        private void SimpleMove()
        {
            m_movementVector = new Vector3(UnityEngine.Input.GetAxis(HORIZONTAL_AXIS_NAME), 0, UnityEngine.Input.GetAxis(VERTICAL_AXIS_NAME));
            m_currentMovementVector += m_movementVector;
        }

        /// <summary>
        /// speed. distance. time.
        /// </summary>
        private void Move()
        {
            Vector3 playerMovementVector = new Vector3(m_currentMovementVector.x, m_currentMovementVector.y, m_currentMovementVector.z) * movementSpeed;
            playerMovementVector = transDirection(playerMovementVector);

            transform.position += playerMovementVector * Time.deltaTime;

            m_currentMovementVector = Vector3.zero;
        }

        private void Jump()
        {
            //TODO: Impliment a double jump because UT2K4 taught me double jump is magic
            if(isGrounded)//Can only jump from a grounded position
            {
                //TESTING CODE!!!!
                if(UnityEngine.Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 jumpHeight = transform.position += (Vector3.up * playerRadius) * playerHeight;
                    //Arbitrary calculation for jump height 
                    //Need to probably specify this as a variable, maybe make it a point at which I need to start checking for the grounded state 
                    //i.e. Max height where the ray casting magic starts?)

                    //TODO: Lerp so this looks smoother.

                    m_currentMovementVector.y = jumpForce; //TODO: Maybe lerp this to make it a smooth curve.
                }
            }
        }

        #endregion



        #region Helpers

        /// <summary>
        /// Returns the Transform Point of a given Vector3
        /// </summary>
        private Vector3 transPoint(Vector3 point)
        {
            return transform.TransformPoint(point);
        }

        /// <summary>
        /// Returns the Transform Direction of a given Vector3
        /// </summary>
        private Vector3 transDirection(Vector3 point)
        {
            return transform.TransformDirection(point);
        }

        #endregion



    }
}