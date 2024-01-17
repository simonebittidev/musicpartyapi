using System;
using Google.Cloud.Firestore;
using SpotifyAPIs.Interface;
using static Grpc.Core.Metadata;

namespace SpotifyAPIs.Provider
{
    public class FirestoreProvider
    {
        private readonly FirestoreDb _fireStoreDb;

        public FirestoreProvider(FirestoreDb fireStoreDb)
        {
            _fireStoreDb = fireStoreDb;
        }

        public async Task<T> AddOrUpdate<T>(T entity, CancellationToken ct) where T : IFirebaseEntity
        {
            var collection = _fireStoreDb.Collection(typeof(T).Name);

            if (string.IsNullOrEmpty(entity.Id)){
                var document = await collection.AddAsync(entity, cancellationToken: ct);
                var snapshot = await document.GetSnapshotAsync(ct);
                return snapshot.ConvertTo<T>();
            }
            else
            {
                var document = collection.Document(entity.Id);
                await document.SetAsync(entity, cancellationToken: ct);
                var snapshot = await document.GetSnapshotAsync(ct);
                return snapshot.ConvertTo<T>();
            }
        }

        public async Task<T> Get<T>(string id, CancellationToken ct) where T : IFirebaseEntity
        {
            var document = _fireStoreDb.Collection(typeof(T).Name).Document(id);
            var snapshot = await document.GetSnapshotAsync(ct);
            return snapshot.ConvertTo<T>();
        }

        public async Task<IReadOnlyCollection<T>> GetAll<T>(CancellationToken ct) where T : IFirebaseEntity
        {
            var collection = _fireStoreDb.Collection(typeof(T).Name);
            var snapshot = await collection.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task<IReadOnlyCollection<T>> WhereEqualTo<T>(string fieldPath, object value, CancellationToken ct) where T : IFirebaseEntity
        {
            return await GetList<T>(_fireStoreDb.Collection(typeof(T).Name).WhereEqualTo(fieldPath, value), ct);
        }

        public async Task<IReadOnlyCollection<T>> WhereArrayContains<T>(string fieldPath, object value, CancellationToken ct) where T : IFirebaseEntity
        {
            return await GetList<T>(_fireStoreDb.Collection(typeof(T).Name).WhereArrayContains(fieldPath, value), ct);
        }

        private static async Task<IReadOnlyCollection<T>> GetList<T>(Query query, CancellationToken ct) where T : IFirebaseEntity
        {
            var snapshot = await query.GetSnapshotAsync(ct);
            return snapshot.Documents.Select(x => x.ConvertTo<T>()).ToList();
        }

        public async Task Delete<T>(string id, CancellationToken ct) where T : IFirebaseEntity
        {
            var collection = _fireStoreDb.Collection(typeof(T).Name);
            var document = collection.Document(id);
            await document.DeleteAsync();
        }
    }
}

