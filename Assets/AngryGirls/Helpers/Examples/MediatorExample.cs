using Angry_Girls;
using System;
using UnityEngine;

namespace TestNameSpace  //dont forget about namespace
{
    //https://refactoring.guru/ru/design-patterns/mediator
    //https://refactoring.guru/ru/design-patterns/mediator/csharp/example
    // Интерфейс Посредника предоставляет метод, используемый компонентами для
    // уведомления посредника о различных событиях. Посредник может реагировать
    // на эти события  и передавать исполнение другим компонентам.
    public interface IMediator
    {
        void Notify(object sender, string eventName);
    }

    // Конкретные Посредники реализуют совместное поведение, координируя
    // отдельные компоненты.
    public class TestMeidator : IMediator
    {
        private TestComponent _testComponent;
        private TestComponent2 _testComponent2;

        public TestMeidator(TestComponent testComponent, TestComponent2 testComponent2)
        {
            _testComponent = testComponent;
            _testComponent.SetMediator(this);
            _testComponent2 = testComponent2;
            _testComponent2.SetMediator(this);
        }
        public void Notify(object sender, string eventName)
        {
            if (eventName == "A")
            {
                ColorDebugLog.Log("this" + "'s" + " Subcomponent's Mediator reacts on " + SubcomponentMediator_EventNames.Launch_Unit + " and triggers following operations:", System.Drawing.KnownColor.ControlLightLight);
                this._testComponent2.DoC();
            }
            if (eventName == "D")
            {
                Debug.Log("Mediator reacts on D and triggers following operations:");
                this._testComponent.DoB();
                this._testComponent2.DoC();
            }
        }
    }

    // Базовый Компонент обеспечивает базовую функциональность хранения
    // экземпляра посредника внутри объектов компонентов.
    public abstract class BaseMediatorComponent
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

    // Конкретные Компоненты реализуют различную функциональность. Они не
    // зависят от других компонентов. Они также не зависят от каких-либо
    // конкретных классов посредников.
    public class TestComponent: BaseMediatorComponent
    {
        public void DoA()
        {
            ColorDebugLog.Log(this + " proceed ProcessLaunch", System.Drawing.KnownColor.ControlLightLight);

            this._mediator.Notify(this, "A");
        }

        public void DoB()
        {
            Debug.Log("Component 1 does B.");

            this._mediator.Notify(this, "B");
        }
    }

    public class TestComponent2 : BaseMediatorComponent
    {
        public void DoC()
        {
            Debug.Log("Component 2 does C.");

            this._mediator.Notify(this, "C");
        }

        public void DoD()
        {
            Debug.Log("Component 2 does D.");

            this._mediator.Notify(this, "D");
        }
    }

    public class MediatorExample : MonoBehaviour
    {
        private void Awake()
        {
            var testComp = new TestComponent();
            var testComp2 = new TestComponent2();

            new TestMeidator(testComp,testComp2);

            ColorDebugLog.Log(this.name + " triggers operation" + SubcomponentMediator_EventNames.Launch_Unit, System.Drawing.KnownColor.ControlLightLight);
            testComp.DoA();

            Debug.Log("Client triggers operation D.");
            testComp2.DoD();
        }
    }
}