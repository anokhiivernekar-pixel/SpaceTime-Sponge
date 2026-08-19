using System;
using System.Collections.Generic;
using UnityEngine;

public class MassDemoLattice : MonoBehaviour
{
    // =========================================================
    // LATTICE SIZE
    // =========================================================

    [Header("LATTICE SIZE")]

    [Range(4, 60)]
    public int horizontalCells = 20;

    [Range(2, 30)]
    public int verticalCells = 6;

    [Tooltip("Size of each visible cube.")]
    public float cellSize = 3f;

    [Tooltip("Extra points along each line so the bending looks smooth.")]
    [Range(1, 10)]
    public int subdivisionsPerCell = 4;


    // =========================================================
    // LINE APPEARANCE
    // =========================================================

    [Header("LINE APPEARANCE")]

    public Material lineMaterial;

    public Color lineColor =
        new Color(0.55f, 0.15f, 1f, 1f);

    [Range(0.001f, 0.05f)]
    public float lineWidth = 0.003f;


    // =========================================================
    // MASS OBJECT
    // =========================================================

    [Header("MASS OBJECT")]

    [Tooltip("Drag MassSphere here.")]
    public Transform massSphere;


    // =========================================================
    // MASS DATA
    // =========================================================

    [Header("MASS DATA")]

    [Tooltip("Log10 of the user's mass.")]
    public double log10Mass = 0.0;

    [Tooltip("True when the user has entered a valid mass.")]
    public bool hasMass = false;


    // =========================================================
    // CURVATURE STRENGTH
    // =========================================================

    [Header("CURVATURE STRENGTH")]

    [Tooltip("Base amount of distortion.")]
    public float baseCurvatureStrength = 0.6f;

    [Tooltip("How much curvature grows as mass increases.")]
    public float strengthPerMassLevel = 0.28f;

    [Range(0.1f, 1.5f)]
    public float strengthExponent = 0.90f;

    [Tooltip("Maximum allowed visual distortion.")]
    public float maximumCurvatureStrength = 15f;


    // =========================================================
    // CURVATURE RANGE
    // =========================================================

    [Header("CURVATURE RANGE")]

    [Tooltip("Starting radius of the distortion.")]
    public float baseGravityRange = 3f;

    [Tooltip("How much farther the distortion spreads as mass increases.")]
    public float rangePerMassLevel = 0.45f;

    [Range(0.1f, 1.5f)]
    public float rangeExponent = 0.85f;

    public float minimumGravityRange = 3f;

    public float maximumGravityRange = 20f;


    // =========================================================
    // CURVE SHAPE
    // =========================================================

    [Header("CURVE SHAPE")]

    [Tooltip("Prevents a sharp spike near the sphere.")]
    public float softeningDistance = 1.5f;


    // =========================================================
    // INTERNAL
    // =========================================================

    private readonly List<LatticeLine> latticeLines =
        new List<LatticeLine>();

    private Transform generatedParent;

    private Material runtimeMaterial;

    private int oldHorizontalCells;
    private int oldVerticalCells;
    private int oldSubdivisions;

    private float oldCellSize;
    private float oldLineWidth;


    // =========================================================
    // LINE DATA
    // =========================================================

    private class LatticeLine
    {
        public LineRenderer renderer;

        public Vector3[] originalPoints;

        public Vector3[] warpedPoints;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        oldHorizontalCells =
            horizontalCells;

        oldVerticalCells =
            verticalCells;

        oldSubdivisions =
            subdivisionsPerCell;

        oldCellSize =
            cellSize;

        oldLineWidth =
            lineWidth;

        GenerateLattice();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        // Rebuild if grid size changes.

        if (
            horizontalCells != oldHorizontalCells ||
            verticalCells != oldVerticalCells ||
            subdivisionsPerCell != oldSubdivisions ||
            !Mathf.Approximately(cellSize, oldCellSize)
        )
        {
            oldHorizontalCells =
                horizontalCells;

            oldVerticalCells =
                verticalCells;

            oldSubdivisions =
                subdivisionsPerCell;

            oldCellSize =
                cellSize;

            GenerateLattice();
        }


        // Update line width.

        if (
            !Mathf.Approximately(
                lineWidth,
                oldLineWidth
            )
        )
        {
            oldLineWidth =
                lineWidth;

            UpdateLineWidth();
        }


        // Update gravitational distortion.

        UpdateCurvature();
    }


    // =========================================================
    // SET MASS
    // =========================================================

    public void SetMassLog10(double newLog10Mass)
    {
        log10Mass =
            newLog10Mass;

        hasMass =
            true;
    }


    // =========================================================
    // CLEAR MASS
    // =========================================================

    public void ClearMass()
    {
        log10Mass =
            0.0;

        hasMass =
            false;
    }


    // =========================================================
    // GENERATE LATTICE
    // =========================================================

    private void GenerateLattice()
    {
        ClearGeneratedLattice();


        GameObject parent =
            new GameObject(
                "Generated Mass Lattice"
            );


        parent.transform.SetParent(
            transform,
            false
        );


        generatedParent =
            parent.transform;


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


        int horizontalSegments =
            horizontalCells *
            subdivisionsPerCell;


        int verticalSegments =
            verticalCells *
            subdivisionsPerCell;


        float subdivisionSize =
            cellSize /
            subdivisionsPerCell;


        // =====================================================
        // X DIRECTION LINES
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
                        horizontalSegments + 1
                    ];


                float yPosition =
                    -halfVertical +
                    y * cellSize;


                float zPosition =
                    -halfHorizontal +
                    z * cellSize;


                for (
                    int p = 0;
                    p <= horizontalSegments;
                    p++
                )
                {
                    float xPosition =
                        -halfHorizontal +
                        p * subdivisionSize;


                    points[p] =
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
        // Y DIRECTION LINES
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
                        verticalSegments + 1
                    ];


                float xPosition =
                    -halfHorizontal +
                    x * cellSize;


                float zPosition =
                    -halfHorizontal +
                    z * cellSize;


                for (
                    int p = 0;
                    p <= verticalSegments;
                    p++
                )
                {
                    float yPosition =
                        -halfVertical +
                        p * subdivisionSize;


                    points[p] =
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
        // Z DIRECTION LINES
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
                        horizontalSegments + 1
                    ];


                float xPosition =
                    -halfHorizontal +
                    x * cellSize;


                float yPosition =
                    -halfVertical +
                    y * cellSize;


                for (
                    int p = 0;
                    p <= horizontalSegments;
                    p++
                )
                {
                    float zPosition =
                        -halfHorizontal +
                        p * subdivisionSize;


                    points[p] =
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
    // CREATE LINE
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
            2;


        // =====================================================
        // MATERIAL
        // =====================================================

        if (lineMaterial != null)
        {
            line.sharedMaterial =
                lineMaterial;
        }
        else
        {
            if (runtimeMaterial == null)
            {
                Shader shader =
                    Shader.Find(
                        "Sprites/Default"
                    );


                if (shader == null)
                {
                    shader =
                        Shader.Find(
                            "Universal Render Pipeline/Unlit"
                        );
                }


                if (shader != null)
                {
                    runtimeMaterial =
                        new Material(
                            shader
                        );


                    if (
                        runtimeMaterial.HasProperty(
                            "_Color"
                        )
                    )
                    {
                        runtimeMaterial.SetColor(
                            "_Color",
                            lineColor
                        );
                    }


                    if (
                        runtimeMaterial.HasProperty(
                            "_BaseColor"
                        )
                    )
                    {
                        runtimeMaterial.SetColor(
                            "_BaseColor",
                            lineColor
                        );
                    }
                }
            }


            if (runtimeMaterial != null)
            {
                line.sharedMaterial =
                    runtimeMaterial;
            }
        }


        // =====================================================
        // SAVE ORIGINAL POINTS
        // =====================================================

        Vector3[] original =
            new Vector3[
                points.Length
            ];


        Vector3[] warped =
            new Vector3[
                points.Length
            ];


        for (
            int i = 0;
            i < points.Length;
            i++
        )
        {
            original[i] =
                points[i];

            warped[i] =
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
            original;

        latticeLine.warpedPoints =
            warped;


        latticeLines.Add(
            latticeLine
        );
    }


    // =========================================================
    // UPDATE CURVATURE
    // =========================================================

    private void UpdateCurvature()
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
                latticeLine.warpedPoints[
                    pointIndex
                ] =
                    WarpPoint(
                        latticeLine.originalPoints[
                            pointIndex
                        ]
                    );
            }


            latticeLine.renderer.SetPositions(
                latticeLine.warpedPoints
            );
        }
    }


    // =========================================================
    // WARP A SINGLE POINT
    // =========================================================

    private Vector3 WarpPoint(
        Vector3 originalPoint
    )
    {
        // No entered mass = straight lattice.

        if (
            !hasMass ||
            massSphere == null
        )
        {
            return originalPoint;
        }


        // =====================================================
        // MASS POSITION
        // =====================================================

        Vector3 bodyPosition =
            transform.InverseTransformPoint(
                massSphere.position
            );


        // =====================================================
        // SPHERE RADIUS
        // =====================================================

        float sphereWorldDiameter =
            Mathf.Max(
                Mathf.Abs(
                    massSphere.lossyScale.x
                ),

                Mathf.Abs(
                    massSphere.lossyScale.y
                ),

                Mathf.Abs(
                    massSphere.lossyScale.z
                )
            );


        float sphereWorldRadius =
            sphereWorldDiameter *
            0.5f;


        float latticeScale =
            Mathf.Max(
                Mathf.Abs(
                    transform.lossyScale.x
                ),
                0.0001f
            );


        float sphereRadius =
            sphereWorldRadius /
            latticeScale;


        // =====================================================
        // MASS LEVEL
        //
        // Uses log10 so astronomical masses remain manageable.
        //
        // 50 kg            -> log10 ≈ 1.7
        // 50,000,000 kg    -> log10 ≈ 7.7
        // Earth            -> log10 ≈ 24.8
        // Sun              -> log10 ≈ 30.3
        // =====================================================

        double safeLog =
            Math.Max(
                0.0,
                Math.Min(
                    log10Mass,
                    10000.0
                )
            );


        float massLevel =
            (float)safeLog;


        // =====================================================
        // CURVATURE STRENGTH
        // =====================================================

        float strengthLevel =
            Mathf.Pow(
                Mathf.Max(
                    massLevel,
                    0.0001f
                ),
                strengthExponent
            );


        float strength =
            baseCurvatureStrength +
            strengthLevel *
            strengthPerMassLevel;


        strength =
            Mathf.Clamp(
                strength,
                0.05f,
                maximumCurvatureStrength
            );


        // =====================================================
        // GRAVITY RANGE
        // =====================================================

        float rangeLevel =
            Mathf.Pow(
                Mathf.Max(
                    massLevel,
                    0.0001f
                ),
                rangeExponent
            );


        float gravityRange =
            baseGravityRange +
            rangeLevel *
            rangePerMassLevel;


        gravityRange =
            Mathf.Clamp(
                gravityRange,
                minimumGravityRange,
                maximumGravityRange
            );


        // =====================================================
        // DISTANCE TO BODY
        // =====================================================

        Vector3 directionToBody =
            bodyPosition -
            originalPoint;


        float centerDistance =
            directionToBody.magnitude;


        if (
            centerDistance <
            0.0001f
        )
        {
            return originalPoint;
        }


        // =====================================================
        // DISTANCE FROM SPHERE SURFACE
        //
        // This is very important.
        //
        // A large sphere will not completely hide its
        // curvature because the effect begins from its surface.
        // =====================================================

        float surfaceDistance =
            centerDistance -
            sphereRadius;


        surfaceDistance =
            Mathf.Max(
                0f,
                surfaceDistance
            );


        if (
            surfaceDistance >
            gravityRange
        )
        {
            return originalPoint;
        }


        // =====================================================
        // RANGE FALLOFF
        // =====================================================

        float normalizedDistance =
            surfaceDistance /
            gravityRange;


        normalizedDistance =
            Mathf.Clamp01(
                normalizedDistance
            );


        float falloff =
            1f -
            Mathf.SmoothStep(
                0f,
                1f,
                normalizedDistance
            );


        // =====================================================
        // NEWTON-LIKE DISTANCE FALLOFF
        // =====================================================

        float softenedDistance =
            surfaceDistance /
            Mathf.Max(
                softeningDistance,
                0.01f
            );


        float inverseSquareShape =
            1f /
            (
                1f +
                softenedDistance *
                softenedDistance
            );


        // =====================================================
        // FINAL DISPLACEMENT
        // =====================================================

        float displacement =
            strength *
            falloff *
            (
                0.30f +
                0.70f *
                inverseSquareShape
            );


        Vector3 direction =
            directionToBody.normalized;


        return originalPoint +
               direction *
               displacement;
    }


    // =========================================================
    // UPDATE LINE WIDTH
    // =========================================================

    private void UpdateLineWidth()
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


            latticeLines[i]
                .renderer
                .startWidth =
                    lineWidth;


            latticeLines[i]
                .renderer
                .endWidth =
                    lineWidth;
        }
    }


    // =========================================================
    // DELETE GENERATED LATTICE
    // =========================================================

    private void ClearGeneratedLattice()
    {
        latticeLines.Clear();


        if (
            generatedParent != null
        )
        {
            Destroy(
                generatedParent.gameObject
            );


            generatedParent =
                null;
        }


        Transform oldGrid =
            transform.Find(
                "Generated Mass Lattice"
            );


        if (
            oldGrid != null
        )
        {
            Destroy(
                oldGrid.gameObject
            );
        }
    }
}