using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eventualize.Core
{
    public class EventualizeFoldingLogic<T>
    {
        public readonly Dictionary<string, IFoldingFunction<T>> Logic;

        public EventualizeFoldingLogic(Dictionary<string, IFoldingFunction<T>> logic){
            Logic = logic;
        }

        public T FoldEvent(T oldState, EventualizeEvent someEvent)
        {
            T currentState = oldState;
            IFoldingFunction<T>? foldingFunction;
            if (!Logic.TryGetValue(someEvent.EventType, out foldingFunction)) throw new ArgumentNullException(nameof(someEvent));
            currentState = foldingFunction.Fold(currentState, someEvent);
            return currentState;
        }

    }
}