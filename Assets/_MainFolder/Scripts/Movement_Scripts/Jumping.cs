using UnityEngine;

namespace Movement
{
    public class Jumping : MonoBehaviour
    {
        Rigidbody rb;
        public float jumpHeight = 10;

        public int maxJumpCount = 2;
        public int jumpsRemaining = 0;
        private PlayerMovementAdvanced pm;

        void Start()
        {
            pm = GetComponent<PlayerMovementAdvanced>();
            rb = gameObject.GetComponent<Rigidbody>();
        }

        public void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "Floor")
            {
                pm.grounded = false;
                Physics.gravity = Vector3.Lerp(Physics.gravity, new Vector3(0, -200f, 0), Time.deltaTime); //постепенно увеличивает гравитацию.
            }
            else
            {
                Physics.gravity = new Vector3(0, -30, 0);
            } //Возвращает гравитацию в исходное положение при падении на землю
        }


    }
}