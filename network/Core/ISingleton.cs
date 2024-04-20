namespace network.Core;

public interface ISingleton
{
    void CleanInstance();

    bool Suspended { get; }

    void SuspendThis(bool b);
}