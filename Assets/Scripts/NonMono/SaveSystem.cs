using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

public static class SaveSystem
{
    public static void Save(List<Painting> list)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/data.info";
        FileStream stream = new FileStream(path, FileMode.Create);

        List<Painting> data = list;

        formatter.Serialize(stream, data);
        stream.Close();
    }

    public static List<Painting> Load()
    {
        string path = Application.persistentDataPath + "/data.info";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            List<Painting> data = formatter.Deserialize(stream) as List<Painting>;

            stream.Close();

            return data;
        }
        else
        {
            return null;
        }
    }
}
