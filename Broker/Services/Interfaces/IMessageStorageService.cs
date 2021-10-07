using Broker.Models;

namespace Broker.Services.Interfaces
{
    public interface IMessageStorageService
    {
        void Add(Message message);

        Message GetNext();

        // verficam daca in storage mai sunt mesaje sau nu
        bool IsEmpty();
    }
}