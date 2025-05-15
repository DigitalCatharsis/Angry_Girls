using System;
using UnityEngine;

public class TODO : MonoBehaviour
{
    //������� ������������ ������ ��� �������. ���������� ����� ����������� �������� ������������ �������� �������
    //����� ��������� �� ���� ����� || UPD: ��������� ������ ����������� ��� ������� ���������
            //������� ������� ������ ������������ ��� � ���� ������ �� �������� � LaunchManager
    //�������� �����������
}

public class SomeClass : IDisposable
{
    private bool disposed = false;

    // ���������� ���������� IDisposable.
    public void Dispose()
    {
        // ����������� ������������� �������
        Dispose(true);
        // ��������� �����������
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            // ����������� ����������� �������
        }
        // ����������� ������������� �������
        disposed = true;
    }

    // ����������
    ~SomeClass()
    {
        Dispose(false);
    }
}
