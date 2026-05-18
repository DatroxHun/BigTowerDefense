using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SpriteHolder", menuName = "Scriptable Objects/SpriteHolder")]
public class SpriteHolder : ScriptableObject
{
    [SerializeField] public List<SpriteElement> elements;

    public virtual Sprite this[string name]
    {
        get => elements.First(x => x.name == name).sprite;
    }
}

[Serializable]
public class SpriteElement
{
    public string name;
    public Sprite sprite;
}
