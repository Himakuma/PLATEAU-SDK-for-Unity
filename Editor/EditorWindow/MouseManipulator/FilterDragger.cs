using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace PLATEAU
{
    public class FilterDragger : Dragger
    {
        public Func<bool> filter;

        public FilterDragger(Func<bool> filter)
        {
            this.filter = filter;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            base.target.RegisterCallback<MouseDownEvent>(OnMouseDownFilter);
            base.target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            base.target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        //
        // 概要:
        //     Called to unregister event callbacks from the target element.
        protected override void UnregisterCallbacksFromTarget()
        {
            base.target.UnregisterCallback<MouseDownEvent>(OnMouseDownFilter);
            base.target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            base.target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }


        public void OnMouseDownFilter(MouseDownEvent e)
        {
            if (filter.Invoke())
            {
                OnMouseDown(e);
            }
        }
    }
}
