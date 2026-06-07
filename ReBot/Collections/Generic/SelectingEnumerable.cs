using System;
using System.Collections;
using System.Collections.Generic;

namespace ReBot.Collections.Generic
{
    internal readonly struct SelectingEnumerable<TEnumerableSource, TResult, TSource>(TEnumerableSource source, Func<TSource, TResult> selector) : IEnumerable<TResult>
        where TEnumerableSource : IEnumerable<TSource>
    {
        public IEnumerator<TResult> GetEnumerator()
        {
            return new SelectingEnumerator<IEnumerator<TSource>>(source.GetEnumerator(), selector);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        internal readonly struct SelectingEnumerator<TEnumeratorSource>(TEnumeratorSource source, Func<TSource, TResult> selector) : IEnumerator<TResult>
            where TEnumeratorSource : IEnumerator<TSource>
        {
            public TResult Current => selector.Invoke(source.Current);

            public void Dispose()
            {
                source.Dispose();
            }

            public bool MoveNext()
            {
                return source.MoveNext();
            }

            public void Reset()
            {
                source.Reset();
            }

            object IEnumerator.Current => Current;
        }
    }
}
