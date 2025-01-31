using UnityEngine;


public class Jumping : MonoBehaviour
{
    Rigidbody rb;
    public float jumpHeight = 10;
    public bool grounded;
    public int maxJumpCount = 2;
    public int jumpsRemaining = 0;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
       
      



    }

  
    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            grounded = false;
            Physics.gravity = Vector3.Lerp(Physics.gravity, new Vector3(0, -200f, 0), Time.deltaTime); //постепенно увеличивает гравитацию.
        }
        else
        {
            Physics.gravity = new Vector3(0, -30, 0);
        } //Возвращает гравитацию в исходное положение при падении на землю
    }

   
}