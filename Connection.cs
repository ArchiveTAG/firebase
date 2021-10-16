using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;

public class Connection : MonoBehaviour
{
    //Int Values
    public int roomCount;

    //String values
    [SerializeField] string roomTags;
    [SerializeField] string activeUsersChild;
    string roomName;
    string room = "room";


    //Bools
    bool OnJoinRoom;


    //Firebase Referances
    FirebaseAuth auth;
    DatabaseReference reference;


    #region Unity Methods
    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    private void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    #endregion


    #region ro_Multi

    private void OnConnectionTry(int roomList)
    {
        int i = 1;
        
        while (i >= roomList)
        {
            //roomTags üzerinden verileri aldık ve snapshot oluşturup task.result üzerinde ki verileri aktardık
            reference.Child(roomTags).GetValueAsync().ContinueWithOnMainThread(task => {

                //Bir hata oluşmuş hatayı yazdırıp while'dan çıkıyoruz
                if (task.IsFaulted)
                {
                    //Print ile yazdık isterseniz Debug.LogError vs. de kullanabilirsiniz.
                    print($"Bir hata oluştu : {task.Result}");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    int activeP = int.Parse(snapshot.Child(room + i).Child(activeUsersChild).Value.ToString()); //Aktif bir oyuncu daha önceden bağlanmış mı diye kontrol ediyoruz
                    if (activeP >= 2 || activeP == 1)//Burada oda dolu değilse katılıyor
                    {
                        string _newRoomName = room + i;


                        //Firebase'e bağlandık artık
                        OnConnected(activeP,_newRoomName);
                        return;//While döngüsünden çıkalım
                    }
                    else if (activeP <= 0)
                    {
                        OnConnectionTry(roomCount);
                        return;
                    }
                }
            
            });
            i++;
        }
    }


    private void OnConnected(int _count,string _roomName)
    {
        //Öncelikle kendimizi sıraya ekletelim
        _count--;//Toplam aktif oyuncudan bir kişi çıkarıp kaydediyoruz.
        //Kayıt yaptık
        reference.Child(roomTags).Child(_roomName).Child(activeUsersChild).SetValueAsync(_count);
        roomName = _roomName;

        OnJoinRoom = true ;


    }

    private void OnApplicationPause(bool pause)
    {
        //Uygulama durursa veya çıkış yapılırsa
        if (OnJoinRoom)//Eğer bir odada isek
        {
            reference.Child(roomTags).GetValueAsync().ContinueWithOnMainThread(task => {

               
                if (task.IsFaulted)
                {
                    //Print ile yazdık isterseniz Debug.LogError vs. de kullanabilirsiniz.
                    print($"Bir hata oluştu : {task.Result}");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    int activeP = int.Parse(snapshot.Child(roomName).Child(activeUsersChild).Value.ToString()); 
                    activeP++;
                    reference.Child(roomTags).Child(roomName).Child(activeUsersChild).SetValueAsync(activeP);

                }

            });

        }
        
    }


    #endregion



}
