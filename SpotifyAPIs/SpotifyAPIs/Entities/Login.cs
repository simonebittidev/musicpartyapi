using System;
using Google.Cloud.Firestore;
using SpotifyAPIs.Interface;

namespace SpotifyAPIs.Entities
{
    [FirestoreData]
    public class Login : IFirebaseEntity
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string AccessToken { get; set; }

        public Login()
        {

        }

        public Login(string id, string accessToken)
        {
            Id = id;
            AccessToken = accessToken;
        }
    }
}

