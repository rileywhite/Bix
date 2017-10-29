using System;

namespace Bix.Repositories.EntityFramework
{
    //[Serializable]
    public class EntityFrameworkRepositoryException : Exception
    {
        public Guid LogInstance { get; private set; } = Guid.NewGuid();

        public EntityFrameworkRepositoryException() { }
        public EntityFrameworkRepositoryException(string message) : base(message) { }
        public EntityFrameworkRepositoryException(string message, Exception innerException) : base(message, innerException) { }

        // see https://github.com/dotnet/coreclr/issues/2715 for discussion on this
        //private VelocityApiException(SerializationInfo info, StreamingContext context)
        //{
        //    this.LogInstance = Guid.Parse(info.GetString("LogInstance"));
        //}

        //[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        //public override void GetObjectData(SerializationInfo info, StreamingContext context)
        //{
        //    base.GetObjectData(info, context);
        //    info.AddValue("LogInstance", this.LogInstance.ToString());
        //}
    }
}
