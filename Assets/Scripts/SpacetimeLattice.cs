using UnityEngine;
using System.Collections.Generic;


public class SpacetimeLattice : MonoBehaviour
{

    [Header("Grid")]
    public int gridSize = 20;
    public float spacing = 2f;
    public int layers = 20;
    public float layerHeight = 2f;


    [Header("Auto Fit")]
    [Tooltip("If enabled, the grid is scaled at runtime so the cube always encloses the farthest gravity body, instead of using the fixed spacing/layerHeight above.")]
    public bool autoFitToBodies = true;

    [Tooltip("Multiplier on the farthest body's distance. 1 = cube edge touches it exactly, 1.2 = 20% breathing room.")]
    public float fitPadding = 1.2f;

    [Tooltip("1 = evenly spaced grid lines. Higher values bunch grid lines up near the center (where the inner planets are) and spread them out toward the edges (where only the outer planets are). Without this, a grid stretched out to Pluto has almost no resolution left near Mercury/Venus/Earth and their warps become invisible.")]
    public float gridCurveExponent = 2.5f;

    // Half the total world-space extent of the cube along XZ and Y. Replaces
    // spacing/layerHeight as the actual driver of point placement once
    // gridCurveExponent != 1 — spacing/layerHeight above are only used as a
    // fallback when autoFitToBodies is off.
    float halfExtentXZ;
    float halfExtentY;


    [Header("Gravity")]
    public float gravitationalConstant = 6.6743e-11f;
    public float curvatureMultiplier = 5e-19f;

    [Tooltip("Prevents the well from spiking to a sharp point right next to a body. Increase for a gentler dip.")]
    public float minDistance = 1f;


    [Header("Bodies")]
    public List<CelestialMass> gravityBodies;


    [Header("Line")]
    public Material lineMaterial;
    public float lineWidth = 0.02f;


    [Header("Curvature Color")]
    [Tooltip("Requires a shader that reads vertex colors, e.g. 'Sprites/Default' or an Unlit shader with a Vertex Color node.")]
    public bool colorByCurvature = true;
    public Color flatColor = new Color(0.1f, 0.3f, 1f);   // blue, far from any mass
    public Color warpedColor = new Color(0.3f, 1f, 0.5f); // green, close to a mass

    [Tooltip("Lower = the green pocket stays tighter around each body. Higher = the color bleeds further out.")]
    public float colorFalloffPower = 0.4f;

    // Each body's strongest curvature value found anywhere on the grid,
    // used to normalize that body's own color independently of the others.
    // Without this, the Sun's curvature would dwarf every planet's and
    // only the Sun would ever show green.
    float[] perBodyMaxCurvature;



    void Start()
    {

        gravityBodies = new List<CelestialMass>(
            FindObjectsByType<CelestialMass>(FindObjectsSortMode.None)
        );


        if (autoFitToBodies && gravityBodies.Count > 0)
        {
            FitGridToBodies();
        }
        else
        {
            halfExtentXZ = (gridSize / 2f) * spacing;
            halfExtentY = (layers / 2f) * layerHeight;
        }


        if (colorByCurvature)
        {
            MeasureMaxCurvaturePerBody();
        }


        GenerateLattice();

        if (runMassTestOnStart)
            TestMassAffectsBend();

    }



    [Header("Debug")]
    [Tooltip("Confirmed working — leave this off unless you're actively debugging, otherwise it logs every time you press Play.")]
    public bool runMassTestOnStart = false;

    [Tooltip("The fixed distance (world units) used by the mass test below. Same distance for every body so the comparison is apples-to-apples.")]
    public float testDistance = 5f;

    // Confirms mass is actually driving the bend. Every body is measured at
    // the SAME fixed distance, so any difference in the logged curvature/
    // displacement values comes purely from mass - if a heavier body doesn't
    // show a bigger number here, mass isn't the thing affecting the bend.
    // Right-click the component header (or the ⋮ menu) in the Inspector and
    // choose "Test Mass Affects Bend" to re-run this anytime, in or out of
    // Play mode.
    [ContextMenu("Test Mass Affects Bend")]
    public void TestMassAffectsBend()
    {

        List<CelestialMass> bodies = gravityBodies;

        if (bodies == null || bodies.Count == 0)
        {
            bodies = new List<CelestialMass>(
                FindObjectsByType<CelestialMass>(FindObjectsSortMode.None)
            );
        }

        if (bodies.Count == 0)
        {
            Debug.LogWarning("SpacetimeLattice: no CelestialMass bodies found in the scene.");
            return;
        }

        List<CelestialMass> sorted = new List<CelestialMass>(bodies);
        sorted.Sort((a, b) => b.mass.CompareTo(a.mass));

        Debug.Log($"--- Mass vs Bend test (all measured at distance = {testDistance}) ---");

        foreach (CelestialMass body in sorted)
        {

            float curvature = gravitationalConstant * body.mass / (testDistance * testDistance);
            float displacementMagnitude = curvature * curvatureMultiplier;

            Debug.Log($"{body.name}: mass={body.mass:E3}  ->  displacement={displacementMagnitude:E4}");

        }

        Debug.Log("If mass is correctly affecting the bend, bodies should be in the SAME order here as they are by mass above - heavier body, bigger displacement, every time.");

    }



    // One pass over every grid vertex, tracking each body's own strongest
    // curvature value separately (indexed to match gravityBodies). This is
    // what lets a small moon and the Sun each get a visible color hotspot
    // at their own closest approach, instead of everything being judged
    // against whichever body happens to be most massive.
    void MeasureMaxCurvaturePerBody()
    {

        int half = gridSize / 2;
        int yHalf = layers / 2;
        perBodyMaxCurvature = new float[gravityBodies.Count];

        for (int x = -half; x <= half; x++)
        {
            for (int y = 0; y <= layers; y++)
            {
                for (int z = -half; z <= half; z++)
                {

                    Vector3 point = new Vector3(
                        MapCoordinate(x, half, halfExtentXZ),
                        MapCoordinate(y - yHalf, yHalf, halfExtentY),
                        MapCoordinate(z, half, halfExtentXZ)
                    );

                    for (int b = 0; b < gravityBodies.Count; b++)
                    {

                        CelestialMass body = gravityBodies[b];
                        Vector3 direction = body.transform.position - point;
                        float distance = direction.magnitude;

                        if (distance < minDistance)
                            distance = minDistance;

                        float curvature = gravitationalConstant * body.mass / (distance * distance);

                        if (curvature > perBodyMaxCurvature[b])
                            perBodyMaxCurvature[b] = curvature;

                    }

                }
            }
        }

    }



    Color GetCurvatureColor(float normalizedIntensity)
    {

        // Power curve pulls most of the line toward flatColor and only lets
        // points very close to a mass reach warpedColor - matches the tight
        // glowing pocket in the reference image rather than a smooth bleed.
        float t = Mathf.Pow(Mathf.Clamp01(normalizedIntensity), colorFalloffPower);

        return Color.Lerp(flatColor, warpedColor, t);

    }



    // Builds an 8-key gradient (Unity's Gradient hard limit) by sampling
    // curvature intensity at evenly spaced points along the line.
    void ApplyCurvatureGradient(LineRenderer line, float[] intensities)
    {

        int sampleCount = Mathf.Min(8, intensities.Length);
        GradientColorKey[] colorKeys = new GradientColorKey[sampleCount];
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {

            float t = (sampleCount == 1) ? 0f : (float)i / (sampleCount - 1);
            int index = Mathf.RoundToInt(t * (intensities.Length - 1));

            colorKeys[i] = new GradientColorKey(GetCurvatureColor(intensities[index]), t);
            alphaKeys[i] = new GradientAlphaKey(1f, t);

        }

        Gradient gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        line.colorGradient = gradient;

    }



    // Sets the cube's total half-extent so it reaches past the farthest
    // planet. Grid line COUNT (gridSize/layers) stays exactly as set in the
    // Inspector — only how far out the cube reaches changes.
    void FitGridToBodies()
    {

        float maxDist = 0f;

        foreach (CelestialMass body in gravityBodies)
        {

            Vector3 pos = body.transform.position;

            maxDist = Mathf.Max(maxDist,
                Mathf.Abs(pos.x),
                Mathf.Abs(pos.y),
                Mathf.Abs(pos.z));

        }

        if (maxDist <= 0f)
            return;

        halfExtentXZ = maxDist * fitPadding;
        halfExtentY = maxDist * fitPadding;

    }



    // Maps an integer grid index to a world-space coordinate. With
    // gridCurveExponent == 1 this is a plain linear grid (old behavior).
    // With gridCurveExponent > 1, indices near the center map close
    // together and indices near the edge spread far apart — so a cube
    // that reaches all the way to Pluto still has fine resolution right
    // around the Sun, Mercury, Venus, Earth, etc.
    float MapCoordinate(int index, int halfRange, float halfExtent)
    {

        if (halfRange == 0)
            return 0f;

        float t = (float)index / halfRange; // -1..1

        float magnitude = Mathf.Pow(Mathf.Abs(t), gridCurveExponent) * halfExtent;

        return Mathf.Sign(t) * magnitude;

    }



    void GenerateLattice()
    {

        int half = gridSize / 2;
        int yHalf = layers / 2;


        // --- Lines running along Y (vertical pillars) for every (x, z) ---
        for (int x = -half; x <= half; x++)
        {

            for (int z = -half; z <= half; z++)
            {

                LineRenderer line = CreateLine();
                Vector3[] points = new Vector3[layers + 1];
                float[] intensities = new float[layers + 1];

                for (int y = 0; y <= layers; y++)
                {

                    Vector3 point = new Vector3(
                        MapCoordinate(x, half, halfExtentXZ),
                        MapCoordinate(y - yHalf, yHalf, halfExtentY),
                        MapCoordinate(z, half, halfExtentXZ)
                    );

                    Vector3 displacement = BendSpace(point, out float intensity);
                    point += displacement;
                    points[y] = point;
                    intensities[y] = intensity;

                }

                line.positionCount = points.Length;
                line.SetPositions(points);

                if (colorByCurvature)
                    ApplyCurvatureGradient(line, intensities);

            }

        }


        // --- Lines running along Z for every (y, x) ---
        for (int y = 0; y <= layers; y++)
        {

            for (int x = -half; x <= half; x++)
            {

                LineRenderer line = CreateLine();
                Vector3[] points = new Vector3[gridSize + 1];
                float[] intensities = new float[gridSize + 1];

                for (int z = -half; z <= half; z++)
                {

                    Vector3 point = new Vector3(
                        MapCoordinate(x, half, halfExtentXZ),
                        MapCoordinate(y - yHalf, yHalf, halfExtentY),
                        MapCoordinate(z, half, halfExtentXZ)
                    );

                    Vector3 displacement = BendSpace(point, out float intensity);
                    point += displacement;
                    points[z + half] = point;
                    intensities[z + half] = intensity;

                }

                line.positionCount = points.Length;
                line.SetPositions(points);

                if (colorByCurvature)
                    ApplyCurvatureGradient(line, intensities);

            }

        }


        // --- Lines running along X for every (y, z) ---
        // This is the set that was missing — it's what turns the grid
        // from a flat fan of lines into an actual cube-shaped lattice.
        for (int y = 0; y <= layers; y++)
        {

            for (int z = -half; z <= half; z++)
            {

                LineRenderer line = CreateLine();
                Vector3[] points = new Vector3[gridSize + 1];
                float[] intensities = new float[gridSize + 1];

                for (int x = -half; x <= half; x++)
                {

                    Vector3 point = new Vector3(
                        MapCoordinate(x, half, halfExtentXZ),
                        MapCoordinate(y - yHalf, yHalf, halfExtentY),
                        MapCoordinate(z, half, halfExtentXZ)
                    );

                    Vector3 displacement = BendSpace(point, out float intensity);
                    point += displacement;
                    points[x + half] = point;
                    intensities[x + half] = intensity;

                }

                line.positionCount = points.Length;
                line.SetPositions(points);

                if (colorByCurvature)
                    ApplyCurvatureGradient(line, intensities);

            }

        }

    }



    Vector3 BendSpace(Vector3 position, out float colorIntensity)
    {

        Vector3 displacement = Vector3.zero;
        float strongestNormalized = 0f;


        for (int b = 0; b < gravityBodies.Count; b++)
        {

            CelestialMass body = gravityBodies[b];

            Vector3 direction = body.transform.position - position;
            float distance = direction.magnitude;

            // Clamp instead of skipping — skipping produces a hard edge,
            // clamping produces a smooth, bounded dip near each mass.
            if (distance < minDistance)
                distance = minDistance;

            float curvature =
                gravitationalConstant *
                body.mass /
                (distance * distance);

            displacement +=
                direction.normalized *
                curvature *
                curvatureMultiplier;

            // How strong is this specifically compared to what THIS body
            // ever achieves on the grid? Keeps small/close bodies visible
            // even when a much bigger body is elsewhere in the scene.
            if (perBodyMaxCurvature != null && perBodyMaxCurvature[b] > 0f)
            {

                float normalized = curvature / perBodyMaxCurvature[b];

                if (normalized > strongestNormalized)
                    strongestNormalized = normalized;

            }

        }

        colorIntensity = strongestNormalized;
        return displacement;

    }



    LineRenderer CreateLine()
    {

        GameObject obj = new GameObject("Spacetime Line");
        obj.transform.parent = this.transform;

        LineRenderer lr = obj.AddComponent<LineRenderer>();

        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.useWorldSpace = true;

        return lr;

    }

}