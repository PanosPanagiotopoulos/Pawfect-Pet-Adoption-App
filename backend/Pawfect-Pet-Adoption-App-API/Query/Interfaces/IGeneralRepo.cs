using System.Linq.Expressions;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Interfaces
{
    /// <summary>
    /// Γενική διεπαφή για την υλοποίηση λειτουργιών CRUD για όλα τα μοντέλα
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGeneralRepo<T> where T : class
    {
        /// <summary>
        /// Επιστρέφει το μοντέλο με βάση το αναγνωριστικό.
        /// </summary>
        /// <param name="id">Το αναγνωριστικό.</param>
        Task<T> GetByIdAsync(string id);
        /// <summary>
        /// Προσθέτει ένα νέο στοιχείο.
        /// </summary>
        /// <param name="entity">Τα δεδομένα του στοιχείου.</param>
        Task<string> AddAsync(T entity);
        /// <summary>
        /// Ενημερώνει το μοντέλο.
        /// </summary>
        /// <param name="entity">Τα δεδομένα του στοιχείου (πρέπει να περιλαμβάνει το αναγνωριστικό).</param>
        Task<bool> UpdateAsync(T entity);
        /// <summary>
        /// Διαγράφει το μοντέλο.
        /// </summary>
        /// <param name="id">Το αναγνωριστικό του μοντέλου που θα διαγραφεί.</param>
        Task<bool> DeleteAsync(string id);
        Task<bool> DeleteAsync(T entity);

        /// <summary>
        /// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
        /// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να ελέγξει αν υπάρχει 
        /// ένα στοιχείο που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
        /// </summary>
        /// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
        /// <returns>Ένα boolean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
        /// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να αναζητήσει ένα στοιχείο 
        /// που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
        /// </summary>
        /// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
        /// <returns>Ένα boolean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
        Task<T> FindAsync(Expression<Func<T, bool>> predicate);

    }
}
