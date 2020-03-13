using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Net;
using System;
using System.IO;
using easyar;
using TMPro;


public class Database : MonoBehaviour
{
    public GameObject prefab, tracker; 
    RectTransform canvas;
    List<Painting> paintings = new List<Painting>();
    List<GameObject> generatedprefabs = new List<GameObject>();
    Dictionary<string, object> obj;
    Texture2D image;
    bool finished = false, prefabsadded = false, sizesetted = false;

    void Start()
    {
        image = new Texture2D(1, 1);
        //I was trying to fix problem with this part of code, that must check google play updates(if i get i right)
        //but any firebase methods isnt work on my phone with this build. But this part of code must be to be sure that
        //google play is up to date, becouse firebase using googleplay
        /*
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });*/

        //I was trying to give permissions to fix my problem with andriod
        /*
        #if PLATFORM_ANDROID
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
            }
        #endif
        */

        //Checking if we already have saved data file, if yes we just load it. 
        //If no, we go to download all data. In right here must be database check and update if its needed.
        if (File.Exists($"{Application.persistentDataPath}/data.info"))
        {
            paintings = SaveSystem.Load();
        }
        else
        {
            SetupFirebase();
        }
    }

    private void Update()
    {
        //This maked becouse Firebase database downloading async, so we need t obe sure downloading is ended. (There migth be better way in firebase methods)
        if (finished)
        {
            SetupPaintings(obj);
            SaveSystem.Save(paintings);
            finished = false;
        }

        if (!prefabsadded && paintings.Count != 0)
        {

            for (int i = 0; i < paintings.Count; i++)
            {
                Instantiate(prefab);
                generatedprefabs.Add(prefab);
                byte[] tmpBytes = File.ReadAllBytes(paintings[i].filename);
                image.LoadImage(tmpBytes);
                //Changing size and position of canvas in prefab depending on image size
                generatedprefabs[i].GetComponent<ImageTargetController>().ImageFileSource.Path = paintings[i].filename;
                generatedprefabs[i].GetComponent<ImageTargetController>().Tracker = tracker.GetComponent<ImageTrackerFrameFilter>();
                generatedprefabs[i].GetComponentInChildren<TextMeshProUGUI>().text = paintings[i].info;
                CheckSize(image, generatedprefabs[i].GetComponentInChildren<RectTransform>());
            }
            prefabsadded = true;
        }
        if (prefabsadded && !sizesetted)
        {
            for (int i = 0; i < paintings.Count; i++)
            {

                sizesetted = true;
            }
        }
    }

    void CheckSize(Texture2D image, RectTransform canvas)
    {
        //Strange but with flaot values it gets rounded
        double temp = (double)image.width / (double)image.height;
        double temp2 = (double)image.height / (double)image.width * 10;
        canvas.anchoredPosition = new Vector2((float)temp, 0);
        canvas.sizeDelta = new Vector2(10, (float)temp2);
    }

    void SetupFirebase()
    {
        //This lines works only if editor(if I can trust people on forums, but app works fine in editor without it)
        /*
        //Set up the Editor before calling into the realtime database.
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://artask-23437.firebaseio.com/Paintings");
        //Get the root reference location of the database.
        DatabaseReference reference = FirebaseDatabase.DefaultInstance.RootReference;
        //Create Dictionary object for further use
        */

        FirebaseDatabase.DefaultInstance.GetReference("Paintings").GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                //Handle the error...
                Debug.Log("Error with downloading DB");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                obj = (Dictionary<string, object>)snapshot.Value;
                finished = true;
            }
        });
    }

    void SetupPaintings(Dictionary<string, object> obj)
    {
        int i = 0;
        foreach (string k in obj.Keys)
        {
            i++;
            using (WebClient client = new WebClient())
            {
                string filename = $"/{i}.jpg";
                string filepath = Application.persistentDataPath + filename;
                //Its a way faster if downloading will be async, but there will be erros with opening not downloaded files
                client.DownloadFile(new Uri(obj[k].ToString()), filepath);
                paintings.Add(new Painting(filepath, k));
            }
        }
    }
}
