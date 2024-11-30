using System.Collections.Generic;
using System.Linq;

public static class RoomIDGenerator
{
    private static HashSet<string> existingIDs = new HashSet<string>();
    private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int idLength = 4;

    public static string GenerateUniqueID()
    {
        string newID;
        do
        {
            newID = new string(Enumerable.Repeat(chars, idLength)
                .Select(s => s[UnityEngine.Random.Range(0, s.Length)]).ToArray());
        } while (existingIDs.Contains(newID));

        existingIDs.Add(newID);
        return newID;
    }

    public static void RemoveID(string id)
    {
        existingIDs.Remove(id);
    }
}
