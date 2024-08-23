using UnityEngine;

namespace Angry_Girls
{
    // Базовый Компонент обеспечивает базовую функциональность хранения
    // экземпляра посредника внутри объектов компонентов.
    public abstract class BaseMediatorComponent : MonoBehaviour
    {
        protected IMediator _mediator; 

        public BaseMediatorComponent(IMediator mediator = null)
        {
            _mediator = mediator;
        }

        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }
    }
}