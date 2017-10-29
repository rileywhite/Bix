using System;

namespace Bix.Repositories.EntityFramework
{
    public interface IAuditingColumns
    {
        string CreatedByColumnName { get; }
        string CreatedAtColumnName { get; }
        string UpdatedByColumnName { get; }
        string UpdatedAtColumnName { get; }
    }
}
