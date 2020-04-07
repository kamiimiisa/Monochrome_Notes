using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

namespace Monochrome_Notes
{
    public class MyNote : MonoBehaviour
    {
        public struct Note
        {
            public float notesTimeg;
            public LineNum lineNum;
            public NoteType noteType;

            public Note(float _noteTimeg, LineNum _lineNum, NoteType _noteType){
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
            }
        }

        public Note notePos { get; private set; }

        public void Intialize(Note _note)
        {
            notePos = _note;
        }
    }
}

