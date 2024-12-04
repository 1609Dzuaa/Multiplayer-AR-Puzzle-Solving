using System;
using Unity.Collections;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString32Bytes Name;
    public int Score;

    public PlayerData(FixedString32Bytes name, int score)
    {
        Name = name;
        Score = score;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Name);
        serializer.SerializeValue(ref Score);
    }
    public bool Equals(PlayerData other)
    {
        return Name == other.Name &&
               Score == other.Score;
    }
}