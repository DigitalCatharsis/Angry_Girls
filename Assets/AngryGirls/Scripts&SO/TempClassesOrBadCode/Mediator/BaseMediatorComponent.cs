using System;
using UnityEngine;

namespace Angry_Girls
{
    // Базовый Компонент обеспечивает базовую функциональность хранения
    // экземпляра посредника внутри объектов компонентов.
    public abstract class BaseMediatorComponent<T> : MonoBehaviour where T : Enum
    {
        protected IMediator<T> _mediator;

        public BaseMediatorComponent(IMediator<T> mediator = null)
        {
            _mediator = mediator;
        }

        public void SetMediator(IMediator<T> mediator)
        {
            _mediator = mediator;
        }
    }
}