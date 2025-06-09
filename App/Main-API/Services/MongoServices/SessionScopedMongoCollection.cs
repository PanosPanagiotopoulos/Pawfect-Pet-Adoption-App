using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Search;

namespace Pawfect_Pet_Adoption_App_API.Services.MongoServices
{
    public class SessionScopedMongoCollection<T> : IMongoCollection<T>
    {
        private readonly IMongoCollection<T> _innerCollection;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionScopedMongoCollection(IMongoCollection<T> innerCollection, IHttpContextAccessor httpContextAccessor)
        {
            _innerCollection = innerCollection ?? throw new ArgumentNullException(nameof(innerCollection));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private IClientSessionHandle GetCurrentSession()
        {
            return _httpContextAccessor.HttpContext?.Items["MongoSession"] as IClientSessionHandle;
        }

        #region Properties
        public CollectionNamespace CollectionNamespace => _innerCollection.CollectionNamespace;
        public IMongoDatabase Database => _innerCollection.Database;
        public IBsonSerializer<T> DocumentSerializer => _innerCollection.DocumentSerializer;
        public IMongoIndexManager<T> Indexes => _innerCollection.Indexes;
        public MongoCollectionSettings Settings => _innerCollection.Settings;
        public IMongoSearchIndexManager SearchIndexes => _innerCollection.SearchIndexes;
        #endregion

        #region Aggregate Methods
        public IAsyncCursor<TResult> Aggregate<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.Aggregate(session, pipeline, options, cancellationToken);
            }
            return _innerCollection.Aggregate(pipeline, options, cancellationToken);
        }

        public IAsyncCursor<TResult> Aggregate<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.Aggregate(session, pipeline, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.AggregateAsync(session, pipeline, options, cancellationToken);
            }
            return _innerCollection.AggregateAsync(pipeline, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.AggregateAsync(session, pipeline, options, cancellationToken);
        }

        public void AggregateToCollection<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                _innerCollection.AggregateToCollection(session, pipeline, options, cancellationToken);
                return;
            }
            _innerCollection.AggregateToCollection(pipeline, options, cancellationToken);
        }

        public void AggregateToCollection<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            _innerCollection.AggregateToCollection(session, pipeline, options, cancellationToken);
        }

        public Task AggregateToCollectionAsync<TResult>(PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
            }
            return _innerCollection.AggregateToCollectionAsync(pipeline, options, cancellationToken);
        }

        public Task AggregateToCollectionAsync<TResult>(IClientSessionHandle session, PipelineDefinition<T, TResult> pipeline, AggregateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.AggregateToCollectionAsync(session, pipeline, options, cancellationToken);
        }
        #endregion

        #region BulkWrite Methods
        public BulkWriteResult<T> BulkWrite(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.BulkWrite(session, requests, options, cancellationToken);
            }
            return _innerCollection.BulkWrite(requests, options, cancellationToken);
        }

        public BulkWriteResult<T> BulkWrite(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.BulkWrite(session, requests, options, cancellationToken);
        }

        public Task<BulkWriteResult<T>> BulkWriteAsync(IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.BulkWriteAsync(session, requests, options, cancellationToken);
            }
            return _innerCollection.BulkWriteAsync(requests, options, cancellationToken);
        }

        public Task<BulkWriteResult<T>> BulkWriteAsync(IClientSessionHandle session, IEnumerable<WriteModel<T>> requests, BulkWriteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.BulkWriteAsync(session, requests, options, cancellationToken);
        }
        #endregion

        #region Count Methods
        public long Count(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.Count(session, filter, options, cancellationToken);
            }
            return _innerCollection.Count(filter, options, cancellationToken);
        }

        public long Count(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.Count(session, filter, options, cancellationToken);
        }

        public Task<long> CountAsync(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.CountAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.CountAsync(filter, options, cancellationToken);
        }

        public Task<long> CountAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.CountAsync(session, filter, options, cancellationToken);
        }

        public long CountDocuments(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.CountDocuments(session, filter, options, cancellationToken);
            }
            return _innerCollection.CountDocuments(filter, options, cancellationToken);
        }

        public long CountDocuments(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.CountDocuments(session, filter, options, cancellationToken);
        }

        public Task<long> CountDocumentsAsync(FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.CountDocumentsAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.CountDocumentsAsync(filter, options, cancellationToken);
        }

        public Task<long> CountDocumentsAsync(IClientSessionHandle session, FilterDefinition<T> filter, CountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.CountDocumentsAsync(session, filter, options, cancellationToken);
        }
        #endregion

        #region Delete Methods
        public DeleteResult DeleteMany(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
        {
            return DeleteMany(filter, null, cancellationToken);
        }

        public DeleteResult DeleteMany(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DeleteMany(session, filter, options, cancellationToken);
            }
            return _innerCollection.DeleteMany(filter, options, cancellationToken);
        }

        public DeleteResult DeleteMany(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DeleteMany(session, filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
        {
            return DeleteManyAsync(filter, null, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DeleteManyAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.DeleteManyAsync(filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DeleteManyAsync(session, filter, options, cancellationToken);
        }

        public DeleteResult DeleteOne(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
        {
            return DeleteOne(filter, null, cancellationToken);
        }

        public DeleteResult DeleteOne(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DeleteOne(session, filter, options, cancellationToken);
            }
            return _innerCollection.DeleteOne(filter, options, cancellationToken);
        }

        public DeleteResult DeleteOne(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DeleteOne(session, filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, CancellationToken cancellationToken = default)
        {
            return DeleteOneAsync(filter, null, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(FilterDefinition<T> filter, DeleteOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DeleteOneAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.DeleteOneAsync(filter, options, cancellationToken);
        }

        public Task<DeleteResult> DeleteOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, DeleteOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DeleteOneAsync(session, filter, options, cancellationToken);
        }
        #endregion

        #region Distinct Methods
        public IAsyncCursor<TField> Distinct<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.Distinct(session, field, filter, options, cancellationToken);
            }
            return _innerCollection.Distinct(field, filter, options, cancellationToken);
        }

        public IAsyncCursor<TField> Distinct<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.Distinct(session, field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DistinctAsync(session, field, filter, options, cancellationToken);
            }
            return _innerCollection.DistinctAsync(field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TField>> DistinctAsync<TField>(IClientSessionHandle session, FieldDefinition<T, TField> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DistinctAsync(session, field, filter, options, cancellationToken);
        }

        public IAsyncCursor<TItem> DistinctMany<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DistinctMany(session, field, filter, options, cancellationToken);
            }
            return _innerCollection.DistinctMany(field, filter, options, cancellationToken);
        }

        public IAsyncCursor<TItem> DistinctMany<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DistinctMany(session, field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.DistinctManyAsync(session, field, filter, options, cancellationToken);
            }
            return _innerCollection.DistinctManyAsync(field, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TItem>> DistinctManyAsync<TItem>(IClientSessionHandle session, FieldDefinition<T, IEnumerable<TItem>> field, FilterDefinition<T> filter, DistinctOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.DistinctManyAsync(session, field, filter, options, cancellationToken);
        }
        #endregion

        #region EstimatedDocumentCount Methods
        public long EstimatedDocumentCount(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.EstimatedDocumentCount(options, cancellationToken);
        }

        public Task<long> EstimatedDocumentCountAsync(EstimatedDocumentCountOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.EstimatedDocumentCountAsync(options, cancellationToken);
        }
        #endregion

        #region Find Methods
        public IAsyncCursor<TProjection> FindSync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindSync(session, filter, options, cancellationToken);
            }
            return _innerCollection.FindSync(filter, options, cancellationToken);
        }

        public IAsyncCursor<TProjection> FindSync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindSync(session, filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.FindAsync(filter, options, cancellationToken);
        }

        public Task<IAsyncCursor<TProjection>> FindAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindAsync(session, filter, options, cancellationToken);
        }
        #endregion

        #region FindOneAndDelete Methods
        public TProjection FindOneAndDelete<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndDelete(session, filter, options, cancellationToken);
            }
            return _innerCollection.FindOneAndDelete(filter, options, cancellationToken);
        }

        public TProjection FindOneAndDelete<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndDelete(session, filter, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);
            }
            return _innerCollection.FindOneAndDeleteAsync(filter, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndDeleteAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, FindOneAndDeleteOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndDeleteAsync(session, filter, options, cancellationToken);
        }
        #endregion

        #region FindOneAndReplace Methods
        public TProjection FindOneAndReplace<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndReplace(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.FindOneAndReplace(filter, replacement, options, cancellationToken);
        }

        public TProjection FindOneAndReplace<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndReplace(session, filter, replacement, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.FindOneAndReplaceAsync(filter, replacement, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndReplaceAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, FindOneAndReplaceOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndReplaceAsync(session, filter, replacement, options, cancellationToken);
        }
        #endregion

        #region FindOneAndUpdate Methods
        public TProjection FindOneAndUpdate<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndUpdate(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.FindOneAndUpdate(filter, update, options, cancellationToken);
        }

        public TProjection FindOneAndUpdate<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndUpdate(session, filter, update, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
        }

        public Task<TProjection> FindOneAndUpdateAsync<TProjection>(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, FindOneAndUpdateOptions<T, TProjection> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.FindOneAndUpdateAsync(session, filter, update, options, cancellationToken);
        }
        #endregion

        #region Insert Methods
        public void InsertOne(T document, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                _innerCollection.InsertOne(session, document, options, cancellationToken);
                return;
            }
            _innerCollection.InsertOne(document, options, cancellationToken);
        }

        public void InsertOne(IClientSessionHandle session, T document, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            _innerCollection.InsertOne(session, document, options, cancellationToken);
        }

        public Task InsertOneAsync(T document, CancellationToken cancellationToken)
        {
            return InsertOneAsync(document, null, cancellationToken);
        }

        public Task InsertOneAsync(T document, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.InsertOneAsync(session, document, options, cancellationToken);
            }
            return _innerCollection.InsertOneAsync(document, options, cancellationToken);
        }

        public Task InsertOneAsync(IClientSessionHandle session, T document, InsertOneOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.InsertOneAsync(session, document, options, cancellationToken);
        }

        public void InsertMany(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                _innerCollection.InsertMany(session, documents, options, cancellationToken);
                return;
            }
            _innerCollection.InsertMany(documents, options, cancellationToken);
        }

        public void InsertMany(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default)
        {
            _innerCollection.InsertMany(session, documents, options, cancellationToken);
        }

        public Task InsertManyAsync(IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.InsertManyAsync(session, documents, options, cancellationToken);
            }
            return _innerCollection.InsertManyAsync(documents, options, cancellationToken);
        }

        public Task InsertManyAsync(IClientSessionHandle session, IEnumerable<T> documents, InsertManyOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.InsertManyAsync(session, documents, options, cancellationToken);
        }
        #endregion

        #region MapReduce Methods
        public IAsyncCursor<TResult> MapReduce<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.MapReduce(session, map, reduce, options, cancellationToken);
            }
            return _innerCollection.MapReduce(map, reduce, options, cancellationToken);
        }

        public IAsyncCursor<TResult> MapReduce<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.MapReduce(session, map, reduce, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.MapReduceAsync(session, map, reduce, options, cancellationToken);
            }
            return _innerCollection.MapReduceAsync(map, reduce, options, cancellationToken);
        }

        public Task<IAsyncCursor<TResult>> MapReduceAsync<TResult>(IClientSessionHandle session, BsonJavaScript map, BsonJavaScript reduce, MapReduceOptions<T, TResult> options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.MapReduceAsync(session, map, reduce, options, cancellationToken);
        }
        #endregion

        #region OfType Method
        public IMongoCollection<TDerivedDocument> OfType<TDerivedDocument>() where TDerivedDocument : T
        {
            IMongoCollection<TDerivedDocument> innerOfType = _innerCollection.OfType<TDerivedDocument>();
            return new SessionScopedMongoCollection<TDerivedDocument>(innerOfType, _httpContextAccessor);
        }
        #endregion

        #region ReplaceOne Methods
        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.ReplaceOne(filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.ReplaceOne(filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);
        }

        public ReplaceOneResult ReplaceOne(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = default)
        {
            return _innerCollection.ReplaceOne(session, filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
            }
            return _innerCollection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, ReplaceOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
        }

        public Task<ReplaceOneResult> ReplaceOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, T replacement, UpdateOptions options, CancellationToken cancellationToken = default)
        {
            return _innerCollection.ReplaceOneAsync(session, filter, replacement, options, cancellationToken);
        }
        #endregion

        #region Update Methods
        public UpdateResult UpdateMany(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.UpdateMany(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.UpdateMany(filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateMany(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.UpdateMany(session, filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateManyAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.UpdateManyAsync(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.UpdateManyAsync(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateManyAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.UpdateManyAsync(session, filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateOne(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.UpdateOne(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.UpdateOne(filter, update, options, cancellationToken);
        }

        public UpdateResult UpdateOne(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.UpdateOne(session, filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateOneAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.UpdateOneAsync(session, filter, update, options, cancellationToken);
            }
            return _innerCollection.UpdateOneAsync(filter, update, options, cancellationToken);
        }

        public Task<UpdateResult> UpdateOneAsync(IClientSessionHandle session, FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.UpdateOneAsync(session, filter, update, options, cancellationToken);
        }
        #endregion

        #region Watch Methods
        public IChangeStreamCursor<TResult> Watch<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.Watch(session, pipeline, options, cancellationToken);
            }
            return _innerCollection.Watch(pipeline, options, cancellationToken);
        }

        public IChangeStreamCursor<TResult> Watch<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.Watch(session, pipeline, options, cancellationToken);
        }

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = default)
        {
            IClientSessionHandle session = GetCurrentSession();
            if (session != null)
            {
                return _innerCollection.WatchAsync(session, pipeline, options, cancellationToken);
            }
            return _innerCollection.WatchAsync(pipeline, options, cancellationToken);
        }

        public Task<IChangeStreamCursor<TResult>> WatchAsync<TResult>(IClientSessionHandle session, PipelineDefinition<ChangeStreamDocument<T>, TResult> pipeline, ChangeStreamOptions options = null, CancellationToken cancellationToken = default)
        {
            return _innerCollection.WatchAsync(session, pipeline, options, cancellationToken);
        }
        #endregion

        #region Configuration Methods
        public IMongoCollection<T> WithReadConcern(ReadConcern readConcern) =>
            new SessionScopedMongoCollection<T>(_innerCollection.WithReadConcern(readConcern), _httpContextAccessor);

        public IMongoCollection<T> WithWriteConcern(WriteConcern writeConcern) =>
            new SessionScopedMongoCollection<T>(_innerCollection.WithWriteConcern(writeConcern), _httpContextAccessor);

        public IMongoCollection<T> WithReadPreference(ReadPreference readPreference) =>
            new SessionScopedMongoCollection<T>(_innerCollection.WithReadPreference(readPreference), _httpContextAccessor);

        IFilteredMongoCollection<TDerivedDocument> IMongoCollection<T>.OfType<TDerivedDocument>()
        {
            IFilteredMongoCollection<TDerivedDocument> innerOfType = _innerCollection.OfType<TDerivedDocument>();
            return new FilteredSessionScopedMongoCollection<TDerivedDocument>(innerOfType, _httpContextAccessor);
        }
        #endregion

        public class FilteredSessionScopedMongoCollection<TDocument> : SessionScopedMongoCollection<TDocument>, IFilteredMongoCollection<TDocument>
        {
            private readonly IFilteredMongoCollection<TDocument> _innerFilteredCollection;

            public FilteredSessionScopedMongoCollection(IFilteredMongoCollection<TDocument> innerFilteredCollection, IHttpContextAccessor httpContextAccessor)
                : base(innerFilteredCollection, httpContextAccessor)
            {
                _innerFilteredCollection = innerFilteredCollection;
            }

            public FilterDefinition<TDocument> Filter => _innerFilteredCollection.Filter;
        }
    }
}