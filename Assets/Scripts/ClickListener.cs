using UnityEngine;
using UnityEngine.UI;

public class ClickListener : MonoBehaviour
{
    public Transform scraper;
    public GameObject spiralGeneratorPrefab;
    GameObject spiralGenerator;
    bool scraperDown = false;


    // Update is called once per frame
    void Update()
    {
        // Check if the left mouse button is pressed and the scraper is up
        if (Input.GetKeyDown(KeyCode.Mouse0) && !scraperDown)
        {
            // Move the scraper 1 unit down // TODO this could be made smooth
            scraper.position = scraper.position + Vector3.down;
            scraperDown = true;

            // Create an instance of spiral generator and give it the scraper object to follow
            spiralGenerator = Instantiate(spiralGeneratorPrefab, scraper.position, Quaternion.identity);
            spiralGenerator.GetComponent<FollowScraper>().scraper = scraper;
        }

        // Check if the left mouse button is released and the scraper is down
        if (Input.GetKeyUp(KeyCode.Mouse0) && scraperDown)
        {
            // Move the scraper 1 unit up   // TODO this could be made smooth
            scraper.position = scraper.position + Vector3.up;
            scraperDown = false;

            // Tell the spiralGenerator to stop scraping
            spiralGenerator.GetComponent<MeshGenerator>().StopScraping();
        }
    }
}
