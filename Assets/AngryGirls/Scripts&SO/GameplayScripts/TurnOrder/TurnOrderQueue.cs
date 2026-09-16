using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Defines an action represented in the turn order.
    /// </summary>
    public enum TurnOrderActionType
    {
        Launch,
        Alternate,
        End
    }

    /// <summary>
    /// Represents a single future action in the turn queue.
    /// </summary>
    public sealed class TurnOrderAction
    {
        public TurnOrderActionType ActionType { get; }
        public CControl Character { get; }

        public TurnOrderAction(
            TurnOrderActionType actionType,
            CControl character)
        {
            ActionType = actionType;
            Character = character;
        }
    }

    /// <summary>
    /// Linked-list based turn order queue.
    /// </summary>
    public sealed class TurnOrderQueue
    {
        private readonly LinkedList<TurnOrderAction> _actions = new();

        /// <summary>
        /// Gets the first node in the queue.
        /// </summary>
        public LinkedListNode<TurnOrderAction> First =>
            _actions.First;

        /// <summary>
        /// Gets the last node in the queue.
        /// </summary>
        public LinkedListNode<TurnOrderAction> Last =>
            _actions.Last;

        /// <summary>
        /// Gets the number of actions in the queue.
        /// </summary>
        public int Count =>
            _actions.Count;

        /// <summary>
        /// Clears the queue.
        /// </summary>
        public void Clear()
        {
            _actions.Clear();
        }

        /// <summary>
        /// Adds a launch action.
        /// </summary>
        public LinkedListNode<TurnOrderAction> AddLaunch(
            CControl character)
        {
            return Add(
                TurnOrderActionType.Launch,
                character);
        }

        /// <summary>
        /// Adds an Alternate action.
        /// </summary>
        public LinkedListNode<TurnOrderAction> AddAlternate(
            CControl character)
        {
            return Add(
                TurnOrderActionType.Alternate,
                character);
        }

        /// <summary>
        /// Adds the final END action.
        /// </summary>
        public LinkedListNode<TurnOrderAction> AddEnd()
        {
            return Add(
                TurnOrderActionType.End,
                null);
        }

        private LinkedListNode<TurnOrderAction> Add(
            TurnOrderActionType actionType,
            CControl character)
        {
            return _actions.AddLast(
                new TurnOrderAction(
                    actionType,
                    character));
        }
    }
}