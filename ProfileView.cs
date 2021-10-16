using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class ProfileView : MonoBehaviour
{
    //Inputs
    [SerializeField] GameObject userId;

    //Texts
    [SerializeField] Text seeUserName;
    [SerializeField] Text seeID;
    [SerializeField] Text seeMoney;
    [SerializeField] Text playingMatch;

    //Transforms
    [SerializeField] RectTransform getSearchPanel;

    //Firebase ref
    FirebaseAuth auth;
    DatabaseReference reference;


    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    
  
    public void SearchFriend()
    {
        if (userId.GetComponent<TMP_InputField>().text != null && userId.GetComponent<TMP_InputField>().text != "")
        {
            reference.Child("OyunVerileri").Child("usernames").Child(userId.GetComponent<TMP_InputField>().text).GetValueAsync().ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    Debug.Log("Database Hata");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    string friendId;
                    friendId = snapshot.Child("id").Value.ToString();
                    print(friendId);

                        reference.Child("OyunVerileri").Child(friendId).GetValueAsync().ContinueWithOnMainThread(tasks => {
                            if (task.IsFaulted)
                            {
                                Debug.Log("Database Hata");
                            }
                            else if (task.IsCompleted)
                            {

                                DataSnapshot snapshots = tasks.Result;
                                seeUserName.text = "" + snapshots.Child("username").Value.ToString();
                                seeMoney.text = "" + snapshots.Child("money").Value.ToString() + "€";
                                seeID.text = "" + friendId;
                                print($"ID : {friendId} MONEY : {snapshots.Child("money").Value}");








                            }
                        });
                    








                   
            

                    if (snapshot.Child("activeMatch").Child("name").Value != null)
                    {
                        playingMatch.text = "Active Match : "+ snapshot.Child("activeMatch").Child("name").Value.ToString();
                    }
                  
                }
            });
        }
    }

   
}
