using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MassDemoController : MonoBehaviour
{
    // =========================================================
    // UI
    // =========================================================

    [Header("UI")]

    [Tooltip("Drag MassInput here.")]
    public TMP_InputField massInput;

    [Tooltip("Drag ApplyMassButton here.")]
    public Button applyButton;


    // =========================================================
    // OBJECTS
    // =========================================================

    [Header("OBJECTS")]

    [Tooltip("Drag MassSphere here.")]
    public GameObject massSphere;

    [Tooltip("Drag SpacetimeLattice here.")]
    public MassDemoLattice lattice;


    // =========================================================
    // SPHERE SIZE
    // =========================================================

    [Header("SPHERE SIZE")]

    [Tooltip("Starting visual diameter.")]
    public float baseSphereDiameter = 0.7f;

    [Tooltip("How much the sphere grows as mass increases.")]
    public float sizeIncreasePerMassLevel = 0.18f;

    [Tooltip("Controls how quickly sphere size increases.")]
    [Range(0.1f, 1.5f)]
    public float sphereGrowthExponent = 0.75f;

    public float minimumSphereDiameter = 0.5f;

    public float maximumSphereDiameter = 3.5f;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        // Start with no sphere and a flat lattice.

        ClearMass();


        // Connect Apply Mass button.

        if (
            applyButton != null
        )
        {
            applyButton.onClick.AddListener(
                ApplyMass
            );
        }


        // Allow Enter key too.

        if (
            massInput != null
        )
        {
            massInput.onSubmit.AddListener(
                OnSubmit
            );
        }
    }


    // =========================================================
    // ENTER KEY
    // =========================================================

    private void OnSubmit(
        string value
    )
    {
        ApplyMass();
    }


    // =========================================================
    // APPLY MASS
    // =========================================================

    public void ApplyMass()
    {
        // -----------------------------------------------------
        // CHECK INPUT
        // -----------------------------------------------------

        if (
            massInput == null
        )
        {
            Debug.LogError(
                "MassInput is not assigned."
            );

            return;
        }


        string input =
            massInput.text.Trim();


        // -----------------------------------------------------
        // EMPTY = NO MASS
        // -----------------------------------------------------

        if (
            string.IsNullOrWhiteSpace(
                input
            )
        )
        {
            ClearMass();

            return;
        }


        // Allow commas:
        //
        // 1,000
        // 1,000,000

        input =
            input.Replace(
                ",",
                ""
            );


        // =====================================================
        // CONVERT MASS INTO LOG10
        // =====================================================

        if (
            !TryGetLog10Mass(
                input,
                out double log10Mass
            )
        )
        {
            Debug.LogWarning(
                "Invalid mass. Try 50, 50000000, 5.972e24, or 1.989e30."
            );


            ClearMass();

            return;
        }


        // =====================================================
        // SHOW SPHERE
        // =====================================================

        if (
            massSphere != null
        )
        {
            massSphere.SetActive(
                true
            );


            // Calculate visual size.

            float diameter =
                CalculateSphereDiameter(
                    log10Mass
                );


            massSphere.transform.localScale =
                Vector3.one *
                diameter;
        }
        else
        {
            Debug.LogError(
                "MassSphere is not assigned."
            );
        }


        // =====================================================
        // SEND MASS TO LATTICE
        // =====================================================

        if (
            lattice != null
        )
        {
            lattice.SetMassLog10(
                log10Mass
            );
        }
        else
        {
            Debug.LogError(
                "MassDemoLattice is not assigned."
            );
        }


        // =====================================================
        // CONSOLE INFORMATION
        // =====================================================

        Debug.Log(
            "Mass applied: " +
            input +
            " kg | log10(mass) = " +
            log10Mass.ToString("F3")
        );
    }


    // =========================================================
    // SPHERE SIZE
    // =========================================================

    private float CalculateSphereDiameter(
        double log10Mass
    )
    {
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


        float growth =
            Mathf.Pow(
                Mathf.Max(
                    massLevel,
                    0.0001f
                ),
                sphereGrowthExponent
            );


        float diameter =
            baseSphereDiameter +
            growth *
            sizeIncreasePerMassLevel;


        return Mathf.Clamp(
            diameter,
            minimumSphereDiameter,
            maximumSphereDiameter
        );
    }


    // =========================================================
    // GET LOG10 MASS
    // =========================================================

    private bool TryGetLog10Mass(
        string input,
        out double log10Mass
    )
    {
        log10Mass =
            0.0;


        if (
            string.IsNullOrWhiteSpace(
                input
            )
        )
        {
            return false;
        }


        input =
            input
                .Trim()
                .ToLowerInvariant();


        // Negative mass is not allowed.

        if (
            input.StartsWith("-")
        )
        {
            return false;
        }


        // Remove optional + sign.

        if (
            input.StartsWith("+")
        )
        {
            input =
                input.Substring(1);
        }


        // =====================================================
        // SCIENTIFIC NOTATION
        //
        // Supports:
        //
        // 5.972e24
        // 1.989e30
        // 2e50
        // 1e1000
        // =====================================================

        int eIndex =
            input.IndexOf('e');


        if (
            eIndex >= 0
        )
        {
            // Only allow one E.

            if (
                input.IndexOf(
                    'e',
                    eIndex + 1
                ) >= 0
            )
            {
                return false;
            }


            string mantissaText =
                input.Substring(
                    0,
                    eIndex
                );


            string exponentText =
                input.Substring(
                    eIndex + 1
                );


            if (
                string.IsNullOrWhiteSpace(
                    mantissaText
                ) ||
                string.IsNullOrWhiteSpace(
                    exponentText
                )
            )
            {
                return false;
            }


            // -------------------------------------------------
            // MANTISSA
            // -------------------------------------------------

            bool validMantissa =
                double.TryParse(
                    mantissaText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double mantissa
                );


            // -------------------------------------------------
            // EXPONENT
            // -------------------------------------------------

            bool validExponent =
                long.TryParse(
                    exponentText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out long exponent
                );


            if (
                !validMantissa ||
                !validExponent
            )
            {
                return false;
            }


            if (
                mantissa <= 0.0 ||
                double.IsNaN(
                    mantissa
                ) ||
                double.IsInfinity(
                    mantissa
                )
            )
            {
                return false;
            }


            // Example:
            //
            // 5.972e24
            //
            // log10 = log10(5.972) + 24

            log10Mass =
                Math.Log10(
                    mantissa
                ) +
                exponent;


            return true;
        }


        // =====================================================
        // NORMAL / VERY LARGE PLAIN NUMBER
        // =====================================================

        return TryGetLog10FromPlainNumber(
            input,
            out log10Mass
        );
    }


    // =========================================================
    // PARSE PLAIN NUMBER
    // =========================================================

    private bool TryGetLog10FromPlainNumber(
        string input,
        out double log10Mass
    )
    {
        log10Mass =
            0.0;


        if (
            string.IsNullOrWhiteSpace(
                input
            )
        )
        {
            return false;
        }


        // =====================================================
        // VALIDATE
        // =====================================================

        int decimalIndex =
            -1;


        for (
            int i = 0;
            i < input.Length;
            i++
        )
        {
            char c =
                input[i];


            if (
                c == '.'
            )
            {
                // Only one decimal point allowed.

                if (
                    decimalIndex >= 0
                )
                {
                    return false;
                }


                decimalIndex =
                    i;


                continue;
            }


            if (
                !char.IsDigit(c)
            )
            {
                return false;
            }
        }


        if (
            decimalIndex < 0
        )
        {
            decimalIndex =
                input.Length;
        }


        // =====================================================
        // FIND FIRST NON-ZERO DIGIT IN ORIGINAL STRING
        // =====================================================

        int firstSignificantIndex =
            -1;


        for (
            int i = 0;
            i < input.Length;
            i++
        )
        {
            char c =
                input[i];


            if (
                c == '.'
            )
            {
                continue;
            }


            if (
                c != '0'
            )
            {
                firstSignificantIndex =
                    i;

                break;
            }
        }


        if (
            firstSignificantIndex < 0
        )
        {
            return false;
        }


        // =====================================================
        // DETERMINE BASE-10 EXPONENT
        // =====================================================

        int exponent;


        if (
            firstSignificantIndex <
            decimalIndex
        )
        {
            // Example:
            //
            // 50000000
            //
            // first significant = 0
            // decimalIndex = 8
            // exponent = 7

            exponent =
                decimalIndex -
                firstSignificantIndex -
                1;
        }
        else
        {
            // Example:
            //
            // 0.005
            //
            // decimalIndex = 1
            // first significant = 4
            // exponent = -3

            exponent =
                decimalIndex -
                firstSignificantIndex;
        }


        // =====================================================
        // COLLECT FIRST 15 SIGNIFICANT DIGITS
        // =====================================================

        string significantDigits =
            "";


        for (
            int i = firstSignificantIndex;
            i < input.Length;
            i++
        )
        {
            char c =
                input[i];


            if (
                c == '.'
            )
            {
                continue;
            }


            if (
                char.IsDigit(c)
            )
            {
                significantDigits +=
                    c;


                if (
                    significantDigits.Length >=
                    15
                )
                {
                    break;
                }
            }
        }


        if (
            significantDigits.Length == 0
        )
        {
            return false;
        }


        // =====================================================
        // CREATE NORMALIZED MANTISSA
        //
        // Example:
        //
        // significantDigits = "5972"
        //
        // normalized = 5.972
        // =====================================================

        string normalizedText;


        if (
            significantDigits.Length ==
            1
        )
        {
            normalizedText =
                significantDigits;
        }
        else
        {
            normalizedText =
                significantDigits.Substring(
                    0,
                    1
                ) +
                "." +
                significantDigits.Substring(
                    1
                );
        }


        bool valid =
            double.TryParse(
                normalizedText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double normalizedValue
            );


        if (
            !valid ||
            normalizedValue <= 0.0
        )
        {
            return false;
        }


        // =====================================================
        // FINAL LOG10
        // =====================================================

        log10Mass =
            Math.Log10(
                normalizedValue
            ) +
            exponent;


        return true;
    }


    // =========================================================
    // CLEAR MASS
    // =========================================================

    public void ClearMass()
    {
        // Hide sphere.

        if (
            massSphere != null
        )
        {
            massSphere.SetActive(
                false
            );
        }


        // Flatten lattice.

        if (
            lattice != null
        )
        {
            lattice.ClearMass();
        }
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    private void OnDestroy()
    {
        if (
            applyButton != null
        )
        {
            applyButton.onClick.RemoveListener(
                ApplyMass
            );
        }


        if (
            massInput != null
        )
        {
            massInput.onSubmit.RemoveListener(
                OnSubmit
            );
        }
    }
}