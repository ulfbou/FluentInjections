
namespace FluentInjections
{
    [Serializable]
    internal class InvalidRegistrationException : Exception
    {
        public InvalidRegistrationException()
        {
        }

        public InvalidRegistrationException(string? message) : base(message)
        {
        }

        public InvalidRegistrationException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}