using UnityEngine;

class Utility {
    public static (int, int) GetTwoUniqueInRange(int minInclusive, int maxInclusive) {
        if (minInclusive == maxInclusive)
            throw new System.Exception("Would infinite loop generating two unique numbers");

        int uniqueOne = Random.Range(minInclusive, maxInclusive);
        int uniqueTwo;

        do {
            uniqueTwo = Random.Range(minInclusive, maxInclusive);
        } while (uniqueOne == uniqueTwo);

        return (uniqueOne, uniqueTwo);
    }
}
