using System;

namespace Lox
{
    public class LoopControl : Exception
    {
        private readonly LoopControlType _type; 

        public LoopControlType Type { get => _type; }

        public LoopControl(LoopControlType type)
        {
            _type = type;
        }
    }

    public enum LoopControlType
    {
        BREAK,
        CONTINUE,
    }
}