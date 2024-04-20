using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace network.Message;

public class ParseObject : IMessage<ParseObject>
{
    public void MergeFrom(ParseObject message)
    {
        throw new NotImplementedException();
    }

    public void MergeFrom(CodedInputStream input)
    {
        throw new NotImplementedException();
    }

    public void WriteTo(CodedOutputStream output)
    {
        throw new NotImplementedException();
    }

    public int CalculateSize()
    {
        throw new NotImplementedException();
    }

    public MessageDescriptor Descriptor { get; }
    public bool Equals(ParseObject? other)
    {
        throw new NotImplementedException();
    }

    public ParseObject Clone()
    {
        throw new NotImplementedException();
    }
}