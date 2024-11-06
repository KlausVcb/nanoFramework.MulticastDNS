namespace nanoFramework.MulticastDNS.Entities
{
    public class Response : Message
    {
        public Response() : base() => flags |= 0x8000;

        public void AddAnswer(Resource resource)
        {
            answers.Add(resource);
        }
    }
}
