using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString32Bytes Name;
    public int Score;
    public int Rank;

    public PlayerData(FixedString32Bytes name, int score, int rank = 0)
    {
        Name = name;
        Score = score;
        Rank = rank;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Score);
        serializer.SerializeValue(ref Rank);
    }
    public bool Equals(PlayerData other)
    {
        return Name == other.Name &&
               Score == other.Score && 
               Rank == other.Rank;
    }
}