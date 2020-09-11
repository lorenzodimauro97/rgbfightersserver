using Network.Messages;

namespace Basic
{
    public interface IMessage
    { 
        byte messageCode { get; set; }
    }
}