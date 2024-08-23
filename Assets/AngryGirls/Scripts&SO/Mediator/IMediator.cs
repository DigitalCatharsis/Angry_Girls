namespace Angry_Girls
{
    //https://refactoring.guru/ru/design-patterns/mediator
    //https://refactoring.guru/ru/design-patterns/mediator/csharp/example

    // Интерфейс Посредника предоставляет метод, используемый компонентами для
    // уведомления посредника о различных событиях. Посредник может реагировать
    // на эти события  и передавать исполнение другим компонентам.
    public interface IMediator
    {
        void Notify(object sender, SubcomponentMediator_EventNames eventName);
    }
}
