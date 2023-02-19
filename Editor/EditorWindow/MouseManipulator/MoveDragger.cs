using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace PLATEAU
{
    public class MoveDragger : Dragger
    {
        public Action<MouseMoveEvent, VisualElement> moveEvent;

        public MoveDragger(Action<MouseMoveEvent, VisualElement> moveEvent)
        {
            this.moveEvent = moveEvent;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            base.target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            base.target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        //
        // 概要:
        //     Called to unregister event callbacks from the target element.
        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMoveEvent);
            base.target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }


        public void OnMouseMoveEvent(MouseMoveEvent e)
        {
            OnMouseMove(e);
            var graphElement = e.target as GraphElement;
            if ((graphElement == null || graphElement.IsMovable()) && m_Active)
            {
                moveEvent.Invoke(e, target);
            }
        }
    }
}
