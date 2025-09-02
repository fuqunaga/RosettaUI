#ifndef HLSL_INCLUDE_ROSETTA_UI_SDF_BEZIER_SPLINE
#define HLSL_INCLUDE_ROSETTA_UI_SDF_BEZIER_SPLINE


/// https://www.shadertoy.com/view/3tdSDj
float SdSegment(float2 p, float2 a, float2 b)
{
    float2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}
///


/// https://www.shadertoy.com/view/fd3fR4
float FindQuinticRootNewtonRaphson(float x, float c[6])
{
    const float tolerance = 1e-7;
    const float epsilon = 1e-14; /* Treshold for infinitly small values. */
    float deriv = c[4] + x * (2. * c[3] + x * (3. * c[2] + x * (4. * c[1] + x * 5. * c[0])));
    bool quit = abs(deriv) <= epsilon;

    for (int i = 0; (i < 15) && (quit == false); ++i)
    {
        float f = (c[5] + x * (c[4] + x * (c[3] + x * (c[2] + x * (c[1] + x * c[0])))));
        x -= f / deriv;
        deriv = c[4] + x * (2. * c[3] + x * (3. * c[2] + x * (4. * c[1] + x * 5. * c[0])));
        quit = (abs(f) <= tolerance) || (abs(deriv) <= epsilon);
    }

    return x;
}

#define INITIALLY_FAR 9e30
#define P(t) (P0 + t*(C + t*(B + t*A)))

float CubicBezierSegmentSdfL2(
    const float2 NDC,
    const float2 P0,
    const float2 P1,
    const float2 P2,
    const float2 P3)
{
    float2 A = -P0 + 3. * P1 - 3. * P2 + P3,
           B = 3. * P0 - 6. * P1 + 3. * P2,
           C = -3. * P0 + 3. * P1,
           D = P0 - NDC;

    float coef[6] =
    {
        6. * dot(A, A),
        10. * dot(A, B),
        4. * (2. * dot(A, C) + dot(B, B)),
        6. * (dot(A, D) + dot(B, C)),
        2. * (2. * dot(B, D) + dot(C, C)),
        2. * dot(C, D)
    };

    float min_sq_dist = INITIALLY_FAR;

    const float N = 20.0;
    float dt = 1.0 / N;
    float2 diff;

    for (float i = 1.; i < N; i += 1.0)
    {
        const float t = FindQuinticRootNewtonRaphson(
            dt * i,
            coef);

        diff = float2(P(clamp(t, 0.0, 1.0))) - NDC;

        min_sq_dist = min(min_sq_dist, dot(diff, diff));
    }

    diff = P0 - NDC; // vec2(P(0)) - NDC
    min_sq_dist = min(min_sq_dist, dot(diff, diff));
    diff = P3 - NDC; // vec2(P(1)) - NDC
    min_sq_dist = min(min_sq_dist, dot(diff, diff));

    return sqrt(min_sq_dist);
}
///


#endif /* HLSL_INCLUDE_ROSETTA_UI_SDF_BEZIER_SPLINE */
