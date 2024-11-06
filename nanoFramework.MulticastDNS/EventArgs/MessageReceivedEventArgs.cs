using nanoFramework.MulticastDNS.Entities;

namespace nanoFramework.MulticastDNS.EventArgs
{
    public class MessageReceivedEventArgs
    {
        public MessageReceivedEventArgs(Message message) => Message = message;

        public Response Response { get; set; }

        public Message Message { get; }
    }
}
