using System;
using UnityEngine;

public class TODO : MonoBehaviour
{
    //Сделать максимальынй радиус для запуска. Превышение будет возвпращать заданное максимальное значение радиуса
    //Вылет персонажа за край карты || UPD: уменьшить радиус оттягивания при запуске персонажа
            //Слишком сильное должно ограничевать зум и силу полета до заданных в LaunchManager
    //Добавить противников
}

public class SomeClass : IDisposable
{
    private bool disposed = false;

    // реализация интерфейса IDisposable.
    public void Dispose()
    {
        // освобождаем неуправляемые ресурсы
        Dispose(true);
        // подавляем финализацию
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        if (disposing)
        {
            // Освобождаем управляемые ресурсы
        }
        // освобождаем неуправляемые объекты
        disposed = true;
    }

    // Деструктор
    ~SomeClass()
    {
        Dispose(false);
    }
}
