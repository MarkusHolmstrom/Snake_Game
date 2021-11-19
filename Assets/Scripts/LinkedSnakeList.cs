using System.Collections;
using System.Collections.Generic;

namespace SnakeList
{
    // Linked list, got help from lecture, class mates and this link:
    // https://stackoverflow.com/questions/3823848/creating-a-very-simple-linked-list
    public class LinkedSnakeList<T> : IEnumerable
    {
        private SnakeNode<T> _head;
        private int _length = 0;

        public LinkedSnakeList()
        {
            _head = null;
        }

        public void AddFirst(T node)
        {
            SnakeNode<T> toAdd = new SnakeNode<T>(node);
            toAdd.Next = _head;
            _head = toAdd;
            _length++;
        }

        public void AddLast(T node)
        {
            SnakeNode<T> newNode = new SnakeNode<T>(node);
            newNode.Next = null;
            _length++;

            if (_head == null)
            {
                _head = new SnakeNode<T>(node);
                return;
            }

            SnakeNode<T> temp = _head;
            while (temp.Next != null)
            {
                temp = temp.Next;
            }
            temp.Next = newNode;
        }

        public int Count()
        {
            return _length;
        }

        public IEnumerator<T> Enumerator()
        {
            SnakeNode<T> current = _head;
            while (current != null)
            {
                yield return current.Node;
                current = current.Next;
            }
        }

        public IEnumerator GetEnumerator()
        {
            return Enumerator();
        }
    }

    public class SnakeNode<T>
    {
        public T Node;
        public SnakeNode<T> Next;

        public SnakeNode(T node)
        {
            Node = node;
            Next = null;
        }

    }
}
