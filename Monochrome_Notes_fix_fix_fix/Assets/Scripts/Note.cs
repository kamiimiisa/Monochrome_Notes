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
            public Line lineNum;
            public NoteType noteType;
            public List<Note> note;

            public NotePos(float _noteTimeg, Line _lineNum, NoteType _noteType){
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
                note = new List<Note>();
            }

            public NotePos(float _noteTimeg, Line _lineNum, NoteType _noteType, List<Note> _note)
            {
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
                note = _note;
            }
        }

        public NotePos notePos { get; private set; }

        public void Intialize(NotePos _notePos)
        {
            notePos = _notePos;
        }
    }
}

