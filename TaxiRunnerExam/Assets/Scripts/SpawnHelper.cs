using UnityEngine;

public static class SpawnHelper
{
    public static bool IsLaneBlocked(float spawnX, float spawnY, float minGapY, string[] tagsToCheck)
    {
        foreach (string tag in tagsToCheck)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objects)
            {
                if (Mathf.Abs(obj.transform.position.x - spawnX) < 0.2f)
                {
                    if (Mathf.Abs(obj.transform.position.y - spawnY) < minGapY)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public static bool IsRowTooCrowded(float spawnY, float rowGapY, string[] tagsToCheck)
    {
        foreach (string tag in tagsToCheck)
        {
            GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);

            foreach (GameObject obj in objects)
            {
                if (Mathf.Abs(obj.transform.position.y - spawnY) < rowGapY)
                {
                    return true;
                }
            }
        }

        return false;
    }
}