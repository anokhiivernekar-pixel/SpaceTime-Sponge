using System.Collections.Generic;
using UnityEngine;

public class SpacetimeLattice : MonoBehaviour
{
    // =========================================================
    // LATTICE SIZE
    // =========================================================

    [Header("LATTICE SIZE")]

    [Tooltip("Number of cubes across the X and Z directions.")]
    [Range(4, 100)]
    public int horizontalCells = 20;

    [Tooltip("Number of cubes vertically. Lower this to make the lattice shorter.")]
    [Range(2, 100)]
    public int verticalCells = 8;

    [Tooltip("Size of every individual cube. Keep this the same if you want the cubes to stay the same size.")]
    public float cellSize = 3f;


    // =========================================================
    // LINE APPEARANCE
    // =========================================================

    [Header("LINE APPEARANCE")]

    public Material lineMaterial;

    public Color lineColor =
        new Color(0.55f, 0.15f, 1f, 1f);

    [Range(0.001f, 0.1f)]
    public float lineWidth = 0.006f;


    // =========================================================
    // GRAVITY / CURVATURE
    // =========================================================

    [Header("GRAVITY / CURVATURE")]

    [Tooltip("Maximum distance around a body where the lattice can curve.")]
    public float gravityRange = 15f;

    [Tooltip("Overall strength of the curvature.")]
    public float curvatureStrength = 4f;

    [Tooltip(
        "Controls how visible smaller planets are compared with the Sun. " +
        "Lower values make planets more noticeable."
    )]
    [Range(0.05f, 1f)]
    public float massVisualExponent = 0.10f;

    [Tooltip("Prevents very sharp spikes near the center of objects.")]
    public float softeningDistance = 2.5f;

    [Tooltip("Minimum gravity range given to smaller bodies.")]
    [Range(0.05f, 1f)]
    public float minimumRangeFraction = 0.35f;


    // =========================================================
    // UPDATES
    // =========================================================

    [Header("UPDATES")]

    [Tooltip("Automatically finds every object with CelestialMass.")]
    public bool automaticallyFindBodies = true;

    [Tooltip("How often Unity searches for CelestialMass objects.")]
    public float bodySearchInterval = 1f;


    // =========================================================
    // INTERNAL DATA
    // =========================================================

    private readonly List<LatticeLine> latticeLines =
        new List<LatticeLine>();

    private readonly List<CelestialMass> gravityBodies =
        new List<CelestialMass>();

    private readonly List<BodyWarpData> bodyWarpData =
        new List<BodyWarpData>();

    private Transform generatedParent;

    private float bodySearchTimer = 0f;


    // Used to detect Inspector changes during Play Mode

    private int previousHorizontalCells;
    private int previousVerticalCells;
    private float previousCellSize;
    private float previousLineWidth;


    // =========================================================
    // LATTICE LINE CLASS
    // =========================================================

    private class LatticeLine
    {
        public LineRenderer renderer;

        public Vector3[] originalPoints;

        public Vector3[] warpedPoints;
    }


    // =========================================================
    // BODY WARP DATA
    // =========================================================

    private struct BodyWarpData
    {
        public Vector3 position;

        public float massInfluence;

        public float range;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        previousHorizontalCells =
            horizontalCells;

        previousVerticalCells =
            verticalCells;

        previousCellSize =
            cellSize;

        previousLineWidth =
            lineWidth;


        FindGravityBodies();

        GenerateLattice();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // -----------------------------------------------------
        // REBUILD GRID IF SIZE SETTINGS CHANGE
        // -----------------------------------------------------

        if (
            horizontalCells != previousHorizontalCells ||
            verticalCells != previousVerticalCells ||
            !Mathf.Approximately(cellSize, previousCellSize)
        )
        {
            previousHorizontalCells =
                horizontalCells;

            previousVerticalCells =
                verticalCells;

            previousCellSize =
                cellSize;


            GenerateLattice();
        }


        // -----------------------------------------------------
        // UPDATE LINE WIDTH LIVE
        // -----------------------------------------------------

        if (!Mathf.Approximately(lineWidth, previousLineWidth))
        {
            previousLineWidth =
                lineWidth;

            UpdateLineWidths();
        }


        // -----------------------------------------------------
        // SEARCH FOR PLANETS
        // -----------------------------------------------------

        if (automaticallyFindBodies)
        {
            bodySearchTimer +=
                Time.deltaTime;


            if (bodySearchTimer >= bodySearchInterval)
            {
                bodySearchTimer = 0f;

                FindGravityBodies();
            }
        }


        // -----------------------------------------------------
        // CURVE GRID
        // -----------------------------------------------------

        BuildBodyWarpData();

        UpdateLatticeCurvature();
    }


    // =========================================================
    // FIND ALL CELESTIAL MASS OBJECTS
    // =========================================================

    private void FindGravityBodies()
    {
        gravityBodies.Clear();


        CelestialMass[] foundBodies =
            Object.FindObjectsByType<CelestialMass>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );


        for (int i = 0; i < foundBodies.Length; i++)
        {
            if (foundBodies[i] != null)
            {
                gravityBodies.Add(
                    foundBodies[i]
                );
            }
        }
    }


    // =========================================================
    // GENERATE 3D LATTICE
    // =========================================================

    private void GenerateLattice()
    {
        ClearOldLattice();


        GameObject parentObject =
            new GameObject(
                "Generated 3D Lattice"
            );


        parentObject.transform.SetParent(
            transform,
            false
        );


        generatedParent =
            parentObject.transform;


        // =====================================================
        // GRID DIMENSIONS
        // =====================================================

        float horizontalSize =
            horizontalCells *
            cellSize;


        float verticalSize =
            verticalCells *
            cellSize;


        float halfHorizontal =
            horizontalSize *
            0.5f;


        float halfVertical =
            verticalSize *
            0.5f;


        // =====================================================
        // X-DIRECTION LINES
        //
        // These run LEFT ↔ RIGHT
        // =====================================================

        for (
            int y = 0;
            y <= verticalCells;
            y++
        )
        {
            for (
                int z = 0;
                z <= horizontalCells;
                z++
            )
            {
                Vector3[] points =
                    new Vector3[
                        horizontalCells + 1
                    ];


                float yPosition =
                    -halfVertical +
                    y * cellSize;


                float zPosition =
                    -halfHorizontal +
                    z * cellSize;


                for (
                    int x = 0;
                    x <= horizontalCells;
                    x++
                )
                {
                    float xPosition =
                        -halfHorizontal +
                        x * cellSize;


                    points[x] =
                        new Vector3(
                            xPosition,
                            yPosition,
                            zPosition
                        );
                }


                CreateLine(
                    points,
                    "X Line"
                );
            }
        }


        // =====================================================
        // Y-DIRECTION LINES
        //
        // These run UP ↕ DOWN
        // =====================================================

        for (
            int x = 0;
            x <= horizontalCells;
            x++
        )
        {
            for (
                int z = 0;
                z <= horizontalCells;
                z++
            )
            {
                Vector3[] points =
                    new Vector3[
                        verticalCells + 1
                    ];


                float xPosition =
                    -halfHorizontal +
                    x * cellSize;


                float zPosition =
                    -halfHorizontal +
                    z * cellSize;


                for (
                    int y = 0;
                    y <= verticalCells;
                    y++
                )
                {
                    float yPosition =
                        -halfVertical +
                        y * cellSize;


                    points[y] =
                        new Vector3(
                            xPosition,
                            yPosition,
                            zPosition
                        );
                }


                CreateLine(
                    points,
                    "Y Line"
                );
            }
        }


        // =====================================================
        // Z-DIRECTION LINES
        //
        // These run FORWARD ↔ BACKWARD
        // =====================================================

        for (
            int x = 0;
            x <= horizontalCells;
            x++
        )
        {
            for (
                int y = 0;
                y <= verticalCells;
                y++
            )
            {
                Vector3[] points =
                    new Vector3[
                        horizontalCells + 1
                    ];


                float xPosition =
                    -halfHorizontal +
                    x * cellSize;


                float yPosition =
                    -halfVertical +
                    y * cellSize;


                for (
                    int z = 0;
                    z <= horizontalCells;
                    z++
                )
                {
                    float zPosition =
                        -halfHorizontal +
                        z * cellSize;


                    points[z] =
                        new Vector3(
                            xPosition,
                            yPosition,
                            zPosition
                        );
                }


                CreateLine(
                    points,
                    "Z Line"
                );
            }
        }
    }


    // =========================================================
    // CREATE ONE GRID LINE
    // =========================================================

    private void CreateLine(
        Vector3[] points,
        string lineName
    )
    {
        GameObject lineObject =
            new GameObject(
                lineName
            );


        lineObject.transform.SetParent(
            generatedParent,
            false
        );


        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();


        line.useWorldSpace =
            false;


        line.positionCount =
            points.Length;


        line.startWidth =
            lineWidth;

        line.endWidth =
            lineWidth;


        line.startColor =
            lineColor;

        line.endColor =
            lineColor;


        line.numCapVertices =
            0;

        line.numCornerVertices =
            0;


        // -----------------------------------------------------
        // MATERIAL
        // -----------------------------------------------------

        if (lineMaterial != null)
        {
            line.sharedMaterial =
                lineMaterial;
        }
        else
        {
            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/Unlit"
                );


            if (shader == null)
            {
                shader =
                    Shader.Find(
                        "Sprites/Default"
                    );
            }


            if (shader != null)
            {
                Material generatedMaterial =
                    new Material(
                        shader
                    );


                line.material =
                    generatedMaterial;
            }
        }


        // -----------------------------------------------------
        // COPY ORIGINAL POINTS
        // -----------------------------------------------------

        Vector3[] originalCopy =
            new Vector3[
                points.Length
            ];


        Vector3[] warpedCopy =
            new Vector3[
                points.Length
            ];


        for (
            int i = 0;
            i < points.Length;
            i++
        )
        {
            originalCopy[i] =
                points[i];

            warpedCopy[i] =
                points[i];
        }


        line.SetPositions(
            points
        );


        LatticeLine latticeLine =
            new LatticeLine();


        latticeLine.renderer =
            line;


        latticeLine.originalPoints =
            originalCopy;


        latticeLine.warpedPoints =
            warpedCopy;


        latticeLines.Add(
            latticeLine
        );
    }


    // =========================================================
    // UPDATE LINE WIDTHS
    // =========================================================

    private void UpdateLineWidths()
    {
        for (
            int i = 0;
            i < latticeLines.Count;
            i++
        )
        {
            if (
                latticeLines[i].renderer ==
                null
            )
            {
                continue;
            }


            latticeLines[i].renderer.startWidth =
                lineWidth;


            latticeLines[i].renderer.endWidth =
                lineWidth;
        }
    }


    // =========================================================
    // BUILD GRAVITY DATA
    // =========================================================

    private void BuildBodyWarpData()
    {
        bodyWarpData.Clear();


        if (gravityBodies.Count == 0)
        {
            return;
        }


        // -----------------------------------------------------
        // FIND LARGEST MASS
        //
        // Normally this is the Sun.
        // -----------------------------------------------------

        double largestMass =
            0.0;


        for (
            int i = 0;
            i < gravityBodies.Count;
            i++
        )
        {
            CelestialMass body =
                gravityBodies[i];


            if (body == null)
            {
                continue;
            }


            if (body.mass > largestMass)
            {
                largestMass =
                    body.mass;
            }
        }


        if (largestMass <= 0.0)
        {
            return;
        }


        // -----------------------------------------------------
        // CREATE WARP DATA FOR EVERY BODY
        // -----------------------------------------------------

        for (
            int i = 0;
            i < gravityBodies.Count;
            i++
        )
        {
            CelestialMass body =
                gravityBodies[i];


            if (body == null)
            {
                continue;
            }


            if (body.mass <= 0.0)
            {
                continue;
            }


            // Convert planet world position
            // into lattice local position.

            Vector3 localPosition =
                transform.InverseTransformPoint(
                    body.transform.position
                );


            // -------------------------------------------------
            // RELATIVE MASS
            // -------------------------------------------------

            double massRatioDouble =
                body.mass /
                largestMass;


            float massRatio =
                Mathf.Clamp01(
                    (float)massRatioDouble
                );


            // -------------------------------------------------
            // VISUAL MASS COMPRESSION
            //
            // Real planet masses are tiny compared to the Sun.
            // This keeps the mass order while making planetary
            // curvature visible.
            // -------------------------------------------------

            float visualMass =
                Mathf.Pow(
                    Mathf.Max(
                        massRatio,
                        0.0000001f
                    ),
                    massVisualExponent
                );


            // -------------------------------------------------
            // BODY GRAVITY RANGE
            // -------------------------------------------------

            float rangeMultiplier =
                Mathf.Lerp(
                    minimumRangeFraction,
                    1f,
                    visualMass
                );


            float bodyRange =
                gravityRange *
                rangeMultiplier;


            // -------------------------------------------------
            // STORE DATA
            // -------------------------------------------------

            BodyWarpData data =
                new BodyWarpData();


            data.position =
                localPosition;


            data.massInfluence =
                visualMass;


            data.range =
                bodyRange;


            bodyWarpData.Add(
                data
            );
        }
    }


    // =========================================================
    // UPDATE LATTICE CURVATURE
    // =========================================================

    private void UpdateLatticeCurvature()
    {
        for (
            int lineIndex = 0;
            lineIndex < latticeLines.Count;
            lineIndex++
        )
        {
            LatticeLine latticeLine =
                latticeLines[lineIndex];


            if (
                latticeLine.renderer ==
                null
            )
            {
                continue;
            }


            for (
                int pointIndex = 0;
                pointIndex <
                latticeLine.originalPoints.Length;
                pointIndex++
            )
            {
                Vector3 originalPoint =
                    latticeLine.originalPoints[
                        pointIndex
                    ];


                Vector3 warpedPoint =
                    WarpPoint(
                        originalPoint
                    );


                latticeLine.warpedPoints[
                    pointIndex
                ] =
                    warpedPoint;
            }


            latticeLine.renderer.SetPositions(
                latticeLine.warpedPoints
            );
        }
    }


    // =========================================================
    // WARP ONE GRID POINT
    // =========================================================

    private Vector3 WarpPoint(
        Vector3 originalPoint
    )
    {
        Vector3 finalPoint =
            originalPoint;


        for (
            int i = 0;
            i < bodyWarpData.Count;
            i++
        )
        {
            BodyWarpData body =
                bodyWarpData[i];


            Vector3 directionToBody =
                body.position -
                originalPoint;


            float distance =
                directionToBody.magnitude;


            // -------------------------------------------------
            // OUTSIDE BODY'S CURVATURE RANGE
            // -------------------------------------------------

            if (distance > body.range)
            {
                continue;
            }


            if (distance < 0.0001f)
            {
                continue;
            }


            // -------------------------------------------------
            // SMOOTH EDGE FALLOFF
            // -------------------------------------------------

            float normalizedDistance =
                distance /
                body.range;


            float edgeFalloff =
                1f -
                normalizedDistance;


            edgeFalloff =
                edgeFalloff *
                edgeFalloff;


            // -------------------------------------------------
            // NEWTON-LIKE INVERSE-SQUARE SHAPE
            //
            // Similar to:
            //
            // F = G M m / r²
            //
            // Softening avoids an infinite spike.
            // -------------------------------------------------

            float softenedDistanceSquared =
                distance * distance +
                softeningDistance *
                softeningDistance;


            float softeningSquared =
                softeningDistance *
                softeningDistance;


            float inverseSquareShape =
                softeningSquared /
                softenedDistanceSquared;


            // -------------------------------------------------
            // FINAL DISPLACEMENT
            // -------------------------------------------------

            float displacement =
                curvatureStrength *
                body.massInfluence *
                inverseSquareShape *
                edgeFalloff;


            Vector3 direction =
                directionToBody.normalized;


            finalPoint +=
                direction *
                displacement;
        }


        return finalPoint;
    }


    // =========================================================
    // DELETE OLD LATTICE
    // =========================================================

    private void ClearOldLattice()
    {
        latticeLines.Clear();


        if (generatedParent != null)
        {
            Destroy(
                generatedParent.gameObject
            );


            generatedParent =
                null;
        }


        Transform oldGrid =
            transform.Find(
                "Generated 3D Lattice"
            );


        if (oldGrid != null)
        {
            Destroy(
                oldGrid.gameObject
            );
        }
    }
}