using UnityEngine;

public class KeplerOrbit : MonoBehaviour
{
    [Header("Orbit Size")]
    public float semiMajorAxis;
    public float eccentricity;

    [Header("Orbit Direction")]
    public float perihelionAngle;

    [Header("Orbit Speed")]
    public float orbitalPeriod;

    public Transform orbitCenter;

    private float time;

    void Update()
    {
        time += Time.deltaTime;

        // Mean anomaly
        float M = (2f * Mathf.PI * time) / orbitalPeriod;

        // Solve Kepler Equation
        float E = SolveKepler(M, eccentricity);


        // Ellipse dimensions
        float b = semiMajorAxis *
                  Mathf.Sqrt(1 - eccentricity * eccentricity);


        // Position before rotation
        float x = semiMajorAxis * 
                  (Mathf.Cos(E) - eccentricity);

        float z = b * Mathf.Sin(E);


        Vector3 position = new Vector3(x,0,z);


        // Rotate ellipse
        position =
        Quaternion.Euler(0, perihelionAngle, 0)
        * position;


        transform.position =
        orbitCenter.position + position;
    }


    float SolveKepler(float M, float e)
    {
        float E = M;


        for(int i=0;i<10;i++)
        {
            E -= (E - e*Mathf.Sin(E)-M)
            /
            (1-e*Mathf.Cos(E));
        }


        return E;
    }
}