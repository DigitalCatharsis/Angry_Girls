namespace Angry_Girls
{
    //https://refactoring.guru/ru/design-patterns/mediator
    //https://refactoring.guru/ru/design-patterns/mediator/csharp/example

    // ��������� ���������� ������������� �����, ������������ ������������ ���
    // ����������� ���������� � ��������� ��������. ��������� ����� �����������
    // �� ��� �������  � ���������� ���������� ������ �����������.
    public interface IMediator
    {
        void Notify(object sender, SubcomponentMediator_EventNames eventName);
    }
}
