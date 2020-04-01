using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

namespace Monochrome_Notes
{
    public class MyNote : MonoBehaviour
    {
        public struct NotePos
        {
            public float notesTimeg;
            public LineNum lineNum;

            public NotePos(float _noteTimeg, Config.LineNum _lineNum)
            {
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
            }
        }

        public NotePos notePos { get; private set; }

        public void Intialize(NotePos _notePos)
        {
            notePos = _notePos;
        }
    }
}

