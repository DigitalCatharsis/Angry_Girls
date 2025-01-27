using System;

namespace Angry_Girls
{
    //https://refactoring.guru/ru/design-patterns/mediator
    //https://refactoring.guru/ru/design-patterns/mediator/csharp/example

    // Интерфейс Посредника предоставляет метод, используемый компонентами для
    // уведомления посредника о различных событиях. Посредник может реагировать
    // на эти события  и передавать исполнение другим компонентам.
    public interface IMediator<T> where T : Enum
    {
        //public void Notify(object sender, T eventName);
    }
}
