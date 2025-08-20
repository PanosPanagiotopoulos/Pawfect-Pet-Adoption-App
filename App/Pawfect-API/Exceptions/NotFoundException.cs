using System.Net;
using System.Runtime.Serialization;

namespace Pawfect_API.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a resource is not found.
    /// </summary>
    [Serializable]
    public class NotFoundException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code associated with this exception (404 Not Found).
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.NotFound;

        public String EntityId { get; private set; }

        public Type EntityType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        public NotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message
        /// and entity ID.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="entityId">The ID of the entity that was not found.</param>
        public NotFoundException(String message, String entityId)
            : base(message)
        {
            EntityId = entityId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message,
        /// entity ID, and entity type.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="entityId">The ID of the entity that was not found.</param>
        /// <param name="entityType">The type of the entity that was not found.</param>
        public NotFoundException(String message, String entityId, Type entityType)
            : this(message, entityId)
        {
            EntityType = entityType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public NotFoundException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public NotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            EntityId = info.GetString(nameof(EntityId));
            EntityType = Type.GetType(info.GetString(nameof(EntityType)));
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
            info.AddValue(nameof(EntityId), EntityId);
            info.AddValue(nameof(EntityType), EntityType?.AssemblyQualifiedName);
        }
    }
}