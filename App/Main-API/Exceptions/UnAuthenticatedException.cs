using System.Net;
using System.Runtime.Serialization;

namespace Main_API.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when a resource is not found.
    /// </summary>
    [Serializable]
    public class UnAuthenticatedException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code associated with this exception (404 Not Found).
        /// </summary>
        public HttpStatusCode StatusCode => HttpStatusCode.Forbidden;


        /// <summary>
        /// Initializes a new instance of the <see cref="UnAuthenticatedException"/> class.
        /// </summary>
        public UnAuthenticatedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnAuthenticatedException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public UnAuthenticatedException(String message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="UnAuthenticatedException"/> class with a specified error message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnAuthenticatedException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnAuthenticatedException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        public UnAuthenticatedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
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
        }
    }
}