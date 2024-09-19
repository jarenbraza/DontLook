using UnityEngine;

class Utility {
    public static (int, int) GetTwoUniqueInRange(int minInclusive, int maxExclusive) {
        if (minInclusive == maxExclusive)
            throw new System.Exception("Would infinite loop generating two unique numbers");

        int uniqueOne = Random.Range(minInclusive, maxExclusive);
        int uniqueTwo;

        do {
            uniqueTwo = Random.Range(minInclusive, maxExclusive);
        } while (uniqueOne == uniqueTwo);

        return (uniqueOne, uniqueTwo);
    }

    public static (int, int) GetTwoUniqueInRange(int maxExclusive) {
        if (0 == maxExclusive)
            throw new System.Exception("Would infinite loop generating two unique numbers");

        int uniqueOne = Random.Range(0, maxExclusive);
        int uniqueTwo;

        do {
            uniqueTwo = Random.Range(0, maxExclusive);
        } while (uniqueOne == uniqueTwo);

        return (uniqueOne, uniqueTwo);
    }
}
