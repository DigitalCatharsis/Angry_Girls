using UnityEngine;

namespace Angry_Girls
{
    // ������� ��������� ������������ ������� ���������������� ��������
    // ���������� ���������� ������ �������� �����������.
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