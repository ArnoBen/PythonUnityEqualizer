using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Variable/Bytes")]
public class BytesVariable : ScriptableObject
{
    public delegate void OnChange();
    public OnChange onChange;
    [SerializeField] byte[] defaultValue;
    byte[] value;
    public byte[] Value
    {
        get { return value; }
        set { this.value = value; if(onChange != null) onChange(); }
    }

    void OnEnable()
    {
        Value = defaultValue;
    }
}