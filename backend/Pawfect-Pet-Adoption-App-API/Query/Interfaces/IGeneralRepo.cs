﻿using System.Linq.Expressions;

namespace Pawfect_Pet_Adoption_App_API.Repositories.Interfaces
{
    /// <summary>
    /// Γενική διεπαφή για την υλοποίηση λειτουργιών CRUD για όλα τα μοντέλα
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGeneralRepo<T> where T : class
    {
        /// <summary>
        /// Προσθέτει ένα νέο στοιχείο.
        /// </summary>
        /// <param name="entity">Τα δεδομένα του στοιχείου.</param>
        Task<String> AddAsync(T entity);
        Task<List<String>> AddManyAsync(List<T> entities);
		/// <summary>
		/// Ενημερώνει το μοντέλο.
		/// </summary>
		/// <param name="entity">Τα δεδομένα του στοιχείου (πρέπει να περιλαμβάνει το αναγνωριστικό).</param>
		Task<String> UpdateAsync(T entity);
        Task<List<String>> UpdateManyAsync(List<T> entities);
		/// <summary>
		/// Διαγράφει το μοντέλο.
		/// </summary>
		/// <param name="id">Το αναγνωριστικό του μοντέλου που θα διαγραφεί.</param>
		Task<Boolean> DeleteAsync(String id);
		Task<Boolean> DeleteAsync(T entity);
		Task<List<Boolean>> DeleteAsync(List<String> ids);
		Task<List<Boolean>> DeleteAsync(List<T> entities);

		/// <summary>
		/// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
		/// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να ελέγξει αν υπάρχει 
		/// ένα στοιχείο που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
		/// </summary>
		/// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
		/// <returns>Ένα Booleanean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
		Task<Boolean> ExistsAsync(Expression<Func<T, Boolean>> predicate);

        /// <summary>
        /// Ελέγχει αν υπάρχει ένα στοιχείο με βάση την δοθείσα συνθήκη. 
        /// Το πρόγραμμα μπορεί να περάσει μια γενική συνθήκη και να αναζητήσει ένα στοιχείο 
        /// που ταιριάζει με τις παραμέτρους που χρειάζεται για την κλήση.
        /// </summary>
        /// <param name="predicate">Η συνθήκη για τον έλεγχο της ύπαρξης ενός στοιχείου.</param>
        /// <returns>Ένα Booleanean που υποδηλώνει αν υπάρχει ένα στοιχείο που ταιριάζει με τη συνθήκη.</returns>
        Task<T> FindAsync(Expression<Func<T, Boolean>> predicate);
        Task<T> FindAsync(Expression<Func<T, Boolean>> predicate, List<String> fields);

	}
}
