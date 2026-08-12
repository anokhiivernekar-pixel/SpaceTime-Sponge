using UnityEngine;
using TMPro;

public class PlanetFinder : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField inputField;

    [Header("Camera")]
    public Transform cameraTransform;
    public MonoBehaviour cameraMovementScript;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float distanceFromPlanet = 8f;
    public float heightOffset = 3f;

    private Transform targetPlanet;
    private bool moving = false;


    void Update()
    {
        if (moving && targetPlanet != null)
        {
            Vector3 targetPosition =
                targetPlanet.position
                - cameraTransform.forward * distanceFromPlanet
                + Vector3.up * heightOffset;


            cameraTransform.position = Vector3.Lerp(
                cameraTransform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );


            Quaternion targetRotation =
                Quaternion.LookRotation(
                    targetPlanet.position - cameraTransform.position
                );


            cameraTransform.rotation = Quaternion.Lerp(
                cameraTransform.rotation,
                targetRotation,
                moveSpeed * Time.deltaTime
            );


            if (Vector3.Distance(
                cameraTransform.position,
                targetPosition) < 0.1f)
            {
                moving = false;
            }
        }
    }


    public void SearchPlanet()
    {
        string searchName = inputField.text.Trim();

        Debug.Log("Searching for: " + searchName);


        GameObject[] allObjects = FindObjectsByType<GameObject>(
            FindObjectsSortMode.None
        );


        GameObject foundPlanet = null;


        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Equals(
                searchName,
                System.StringComparison.OrdinalIgnoreCase))
            {
                foundPlanet = obj;
                break;
            }
        }


        if (foundPlanet != null)
        {
            Debug.Log("Found planet: " + foundPlanet.name);


            targetPlanet = foundPlanet.transform;


            if (cameraMovementScript != null)
            {
                cameraMovementScript.enabled = false;
            }


            moving = true;
        }
        else
        {
            Debug.Log(
                "Planet not found: " + searchName
            );
        }
    }
}