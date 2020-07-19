using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScraper : MonoBehaviour
{
  public Transform scraper;
  public Vector3 offset;

  // Update is called once per frame
  void Update()
  {
      transform.position = scraper.position + offset;
  }
}
