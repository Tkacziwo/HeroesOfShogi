using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransformationCalculator
{
    /// <summary>
    /// Creates 2 linear transformation from beginning to center and center to end. Then creates linear transformation between created
    /// linear transformation to achieve quadratic transformation on a curve.
    /// Automatically calculates height and center point of transformation to make longer movements have higher point.
    /// </summary>
    /// <param name="endPosition">Target position</param>
    public static List<Vector3> QuadraticTransformation(Vector3 startPosition, Vector3 endPosition)
    {
        List<Vector3> curvePath = new();
        float maxHeight;
        float multiplier = 0.3f;

        double distance = Math.Sqrt(Math.Pow(startPosition.x - endPosition.x, 2) +
            Math.Pow(startPosition.z - endPosition.z, 2));
        maxHeight = (float)distance * multiplier;
        maxHeight += startPosition.y;
        maxHeight = Math.Max(0.5f, maxHeight);
        /*
         * Center point -> Pcenter = ((startPosition.x + endPosition.x)/2, maxHeight, (startPosition.z + endPosition.z)/2)
         * Linear Interpolation between points startPosition and CenterPosition, center and end point and then
         * between interpolations
         */
        Vector3 centerPosition = new((
            startPosition.x + endPosition.x) * 0.5f,
            maxHeight,
            (startPosition.z + endPosition.z) * 0.5f);

        int steps = (int)Math.Ceiling((double)distance) * 20;
        float stepsMultiplier = 1f / (float)steps;
        if (distance >= 4.0)
        {
            steps /= 2;
            stepsMultiplier *= 2f;
        }
        for (int i = 0; i <= steps; i++)
        {
            float t = i * stepsMultiplier;
            Vector3 Linear0 = (1 - t) * startPosition + (t * centerPosition);
            Vector3 Linear1 = (1 - t) * centerPosition + (t * endPosition);

            Vector3 Quadratic = (1 - t) * Linear0 + (t * Linear1);
            curvePath.Add(Quadratic);
        }

        return curvePath;
    }

    public static List<Vector3> LinearAttackTransformation(Vector3 startPosition, Vector3 enemyPosition)
    {
        Vector3 centerPosition = new((
           startPosition.x + enemyPosition.x) * 0.5f,
           startPosition.y,
           (startPosition.z + enemyPosition.z) * 0.5f);

        return new()
        {
            centerPosition,
            startPosition,
        };
    }
}