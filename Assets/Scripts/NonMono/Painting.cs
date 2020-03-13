using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Painting
{
    public string filename, info;

    public Painting(string filename, string info)
    {
        this.filename = filename;
        this.info = info;
    }
}
