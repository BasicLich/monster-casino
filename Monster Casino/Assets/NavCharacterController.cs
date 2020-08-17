using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavCharacterController : MonoBehaviour
{
    public PlayerAgent agent;
    public GameObject clickLocationEffect;

    NavMeshAgent navAgent;
    private GameObject spawnedClickLocation;

    void Start()
    {
        navAgent = agent.GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (GameManager.instance.eventInProgress)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            float mouseRatioX = Input.mousePosition.x / Screen.width;
            float mouseRatioY = Input.mousePosition.y / Screen.height;
            Vector3 mousePos = new Vector3(mouseRatioX, mouseRatioY, 0f);

            RaycastHit hit;
            if(Physics.Raycast(Camera.main.ViewportPointToRay(mousePos), out hit))
            {
                if (hit.collider.CompareTag("World"))
                {
                    if (spawnedClickLocation != null)
                    {
                        GameObject.Destroy(spawnedClickLocation);
                    }
                    spawnedClickLocation = GameObject.Instantiate(clickLocationEffect);
                    spawnedClickLocation.transform.position = hit.point;
                    spawnedClickLocation.SetActive(true);
                    GameObject.Destroy(spawnedClickLocation, 1f);
                    navAgent.SetDestination(hit.point);
                } else if (hit.collider.CompareTag("Character"))
                {
                    if (spawnedClickLocation != null)
                    {
                        GameObject.Destroy(spawnedClickLocation);
                    }
                    spawnedClickLocation = GameObject.Instantiate(clickLocationEffect);
                    spawnedClickLocation.transform.position = hit.collider.transform.position + Vector3.up * 2f;
                    spawnedClickLocation.SetActive(true);
                    GameObject.Destroy(spawnedClickLocation, 1f);
                    navAgent.SetDestination(hit.point);

                    agent.target = hit.collider.gameObject;
                    hit.collider.gameObject.SendMessage("Poke");
                }
            }
        }
    }
}
