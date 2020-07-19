using UnityEngine;

public class ScraperMovement : MonoBehaviour
{

    public Rigidbody scraper;
    public Vector3 velocity = new Vector3(0, 0, 2);


    // Update is called once per frame
    void Update()
    {
        scraper.velocity = velocity;
    }
}
