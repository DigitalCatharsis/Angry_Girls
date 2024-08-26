using System;
using UnityEngine;

namespace Angry_Girls
{
    // ������� ��������� ������������ ������� ���������������� ��������
    // ���������� ���������� ������ �������� �����������.
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