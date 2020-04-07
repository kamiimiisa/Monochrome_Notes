using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

namespace Monochrome_Notes
{
    public class Note : MonoBehaviour
    {
        public struct NotePos
        {
            public float notesTimeg;
            public LineNum lineNum;
            public NoteType noteType;

            public NotePos(float _noteTimeg, LineNum _lineNum, NoteType _noteType){
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
            }
        }

        public NotePos notePos { get; private set; }

        public void Intialize(NotePos _notePos)
        {
            notePos = _notePos;
        }
    }
}

