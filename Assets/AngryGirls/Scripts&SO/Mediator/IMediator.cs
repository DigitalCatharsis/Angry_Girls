using System;

namespace Angry_Girls
{
    //https://refactoring.guru/ru/design-patterns/mediator
    //https://refactoring.guru/ru/design-patterns/mediator/csharp/example

    // ��������� ���������� ������������� �����, ������������ ������������ ���
    // ����������� ���������� � ��������� ��������. ��������� ����� �����������
    // �� ��� �������  � ���������� ���������� ������ �����������.
    public interface IMediator<T> where T : Enum
    {
        //public void Notify(object sender, T eventName);
    }
}
