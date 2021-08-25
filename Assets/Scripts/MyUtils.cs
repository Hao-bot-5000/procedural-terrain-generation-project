using System;

public static class MyUtils {
    public static float NextFloat(System.Random random) {
        return (float) random.NextDouble();
    }

    public static float NextFloat(System.Random random, float maxValue) {
        if (maxValue < 0) throw new ArgumentOutOfRangeException("'maxValue' must be greater than zero.");

        return (float) (random.NextDouble() * maxValue);
    }

    public static float NextFloat(System.Random random, float minValue, float maxValue) {
        if (maxValue < minValue) throw new ArgumentOutOfRangeException("'maxValue' must be greater than 'minValue'.");

        double range = maxValue - minValue;
        return (float) (random.NextDouble() * range + minValue);
    }
}