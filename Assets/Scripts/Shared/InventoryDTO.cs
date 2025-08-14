using System;

// simples e serializável; Mirror gera (Weaver) para tipos básicos
[Serializable]
public struct ItemDTO
{
    public int id;
    public string name;
    public int qty;
}
