using Google.Cloud.Firestore;
using SpotifyAPIs.Interface;

namespace SpotifyAPIs.Entities
{
    [FirestoreData]
    public class PartyQueue : IFirebaseEntity
    {
        [FirestoreDocumentId]
        public string Id { get; set; }

        [FirestoreProperty]
        public string[] UsersId { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string Description { get; set; }

        public string OwnerId { get; set; }


        public PartyQueue()
        {

        }

        public PartyQueue(string userId, string partyName, string partyDescription, string ownerId)
        {
            UsersId = new string[]{ userId};
            Name = partyName;
            Description = partyDescription;
            OwnerId = ownerId;
        }
    }
}




