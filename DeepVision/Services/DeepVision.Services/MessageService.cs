using DeepVision.Services.Interfaces;

namespace DeepVision.Services
{
    public class MessageService : IMessageService
    {
        public string GetMessage()
        {
            return "Hello from the Message Service";
        }
    }
}
