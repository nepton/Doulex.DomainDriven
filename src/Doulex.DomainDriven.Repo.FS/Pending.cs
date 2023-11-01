using System;

namespace Doulex.DomainDriven.Repo.FS;

/// <summary>
/// 在 Add 或 Update 时，临时保存到 UnitOfWork 的队列中, 等待 SaveChangesAsync 时再进行持久化
/// </summary>
internal class Pending
{
    /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
    public Pending(Type type, PendingAction action, object? obj)
    {
        Type   = type;
        Action = action;
        Object = obj;
    }

    public Type Type { get; }

    public object? Object { get; }

    public PendingAction Action { get; }
}
