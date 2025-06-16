using MongoDB.Driver;
using System.Linq.Expressions;

namespace Main_API.Repositories.Interfaces
{
    /// <summary>
    /// Γενική διεπαφή για την υλοποίηση λειτουργιών CRUD για όλα τα μοντέλα
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMongoRepository<T> where T : class
    {
        /// <summary>
        /// Προσθέτει ένα νέο στοιχείο.
        /// </summary>
        /// <param name="entity">Τα δεδομένα του στοιχείου.</param>
        Task<String> AddAsync(T entity, IClientSessionHandle session = null);
        Task<List<String>> AddManyAsync(List<T> entities, IClientSessionHandle session = null);
		/// <summary>
		/// Ενημερώνει το μοντέλο.
		/// </summary>
		/// <param name="entity">Τα δεδομένα του στοιχείου (πρέπει να περιλαμβάνει το αναγνωριστικό).</param>
		Task<String> UpdateAsync(T entity, IClientSessionHandle session = null);
        Task<List<String>> UpdateManyAsync(List<T> entities, IClientSessionHandle session = null);
		/// <summary>
		/// Διαγράφει το μοντέλο.
		/// </summary>
		/// <param name="id">Το αναγνωριστικό του μοντέλου που θα διαγραφεί.</param>
		Task<Boolean> DeleteAsync(String id, IClientSessionHandle session = null);
		Task<Boolean> DeleteAsync(T entity, IClientSessionHandle session = null);
		Task<List<Boolean>> DeleteAsync(List<String> ids, IClientSessionHandle session = null);
		Task<List<Boolean>> DeleteAsync(List<T> entities, IClientSessionHandle session = null);

		/// <summary>
		/// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
		/// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να ελέγξει αν υπάρχει 
		/// ένα στοιχείο που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
		/// </summary>
		/// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
		/// <returns>Ένα Booleanean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
		Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null);

        /// <summary>
        /// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
        /// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να αναζητήσει ένα στοιχείο 
        /// που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
        /// </summary>
        /// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
        /// <returns>Ένα Booleanean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
        Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null);
        Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields, IClientSessionHandle session = null);

        Task<List<T>> FindManyAsync(Expression<Func<T, Boolean>> predicate, IClientSessionHandle session = null);

        Task<List<T>> FindManyAsync(Expression<Func<T, Boolean>> predicate, List<String> fields, IClientSessionHandle session = null);

    }
}
