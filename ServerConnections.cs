using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class ServerConnections : MonoBehaviour
{


    public static string userName,roomName;
    public static int currentLimit;


     public static FirebaseAuth auth;
  public static   DatabaseReference reference;

   public static bool isRoom,isSceneChanged;



    void Awake()
    {

       
        auth = FirebaseAuth.DefaultInstance;
    }
    private void Start()
    {
        GameObject.DontDestroyOnLoad(this.gameObject);

        reference = FirebaseDatabase.DefaultInstance.RootReference;





        reference.Child("OyunVerileri").Child(auth.CurrentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Fail");

            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;

                userName = snapshot.Child("KullaniciIsim").Value.ToString();
            }

        });
    }
    
     void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Tıklandı");
            StartServer(10,true);
        }
    }




    public static void ChangeScene()
    {
        isSceneChanged = true;
        ClientUser.OnJoined();
    }


    public void PlayGame()
    {
        StartServer(10,false);
    }


    public static void StartServer(int _serverList,bool arenaMode)
    {
        reference.Child("servers").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Fail");
                
            }
            else if (task.IsCompleted)
            {
               
                DataSnapshot snapshot = task.Result;
                for (int i = 1; i < _serverList; i++)
                {

                if (int.Parse(snapshot.Child("room" + i).Child("limit").Value.ToString()) == 2 || int.Parse(snapshot.Child("room" + i).Child("limit").Value.ToString()) == 1)
                    {
                        Debug.Log(i + " numaralı odaya giriş sağlandı");
                        
                        OnJoinedTheRoom("room"+i,int.Parse(snapshot.Child("room"+i).Child("limit").Value.ToString()),userName);
                        return;
                    }




                   else if (int.Parse(snapshot.Child("room" + i).Child("limit").Value.ToString()) == 0)
                    {
                        Debug.Log("Oda dolu");
                        return;
                    }

                }
            }
        });




     void OnJoinedTheRoom(string _roomName, int _currentLimit,string _userName)
        {
            roomName = _roomName;
            currentLimit = _currentLimit;


            isRoom = true;
            if (_currentLimit == 2)
            {
                reference.Child("servers").Child(_roomName).Child("p1").SetValueAsync(_userName);
            }
            else if (_currentLimit == 1)
            {
                reference.Child("servers").Child(_roomName).Child("p2").SetValueAsync(_userName);
            }

            ClientUser.OnJoined();
            _currentLimit--;
            reference.Child("servers").Child(_roomName).Child("limit").SetValueAsync(_currentLimit);
           

        }



       


    }
    private void OnApplicationQuit()
    {
        reference.Child("servers").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            DataSnapshot snapshot = task.Result;
            if (isRoom)
            {
                reference.Child("servers").Child(roomName).Child("limit").SetValueAsync(+1);

                for (int i = 1; i <= 2; i++)
                {
                    if (snapshot.Child(roomName).Child("p"+i).Value.ToString() == userName)
                    {
                        reference.Child("servers").Child(roomName).Child("p"+i).RemoveValueAsync();
                        return;
                    }
                   
                }
            }


        });
      
    }
    public static void OnExitServer()
    {
        reference.Child("servers").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            DataSnapshot snapshot = task.Result;
         
                reference.Child("servers").Child(roomName).Child("limit").SetValueAsync(+1);

                for (int i = 1; i < 3; i++)
                {
                    Debug.Log(i);
                    if (snapshot.Child(roomName).Child("p" + i).Value.ToString() == userName && isRoom)
                    {
                        Debug.Log("UserName " + userName);
                        reference.Child("servers").Child(roomName).Child("p" + i).RemoveValueAsync();
                        
                        
                    }
                    else if (snapshot.Child(roomName).Child("p" + i).Value.ToString() != userName || snapshot.Child(roomName).Child("p" + i).Value.ToString() == null && isRoom)
                    {
                        Debug.Log("Değil");
                    }

                
            }
            

        });
        SceneManager.LoadScene("GameScene");
    }
}
