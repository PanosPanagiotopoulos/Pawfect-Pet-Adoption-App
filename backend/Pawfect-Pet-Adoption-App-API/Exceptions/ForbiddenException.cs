using System.Net;
using System.Runtime.Serialization;

namespace Pawfect_Pet_Adoption_App_API.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a user attempts an unauthorized action.
    /// </summary>
    [Serializable]
    public class ForbiddenException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code associated with this exception (403 Forbidden).
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;

        /// <summary>
        /// Gets the permission that was denied, if applicable.
        /// </summary>
        public String[] Permissions { get; }

        /// <summary>
        /// Gets the resource involved in the forbidden action, if applicable.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
        /// </summary>
        public ForbiddenException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public ForbiddenException(String message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a specified error message,
        /// permission, and resource.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="permission">The permission that was denied.</param>
        /// <param name="resourceType">The resource involved in the forbidden action.</param>
        public ForbiddenException(String message, Type resourceType, params String[] permissions)
            : base(message)
        {
            Permissions = permissions;
            ResourceType = resourceType;
        }

        public ForbiddenException(String message, params String[] permissions)
            : base(message)
        {
            Permissions = permissions;
            ResourceType = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public ForbiddenException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ForbiddenException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public ForbiddenException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Permissions = [info.GetString(nameof(Permissions))];
            ResourceType = Type.GetType(info.GetString(nameof(ResourceType)));
        }

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Permissions), String.Join("_", Permissions));
            info.AddValue(nameof(ResourceType), ResourceType.Name);
        }
    }
}
