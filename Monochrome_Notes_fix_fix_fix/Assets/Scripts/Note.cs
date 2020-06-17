using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Config;

namespace Monochrome_Notes {
    public class Note : MonoBehaviour {
        public struct NotePos {
            public float notesTimeg;
            public Line lineNum;
            public NoteType noteType;
            public List<Note> noteList;

            public NotePos(float _noteTimeg, Line _lineNum, NoteType _noteType) {
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
                noteList = new List<Note>();
            }

            public NotePos(float _noteTimeg, int _lineNum, int _noteType) {
                notesTimeg = _noteTimeg;
                lineNum = (Line)System.Enum.ToObject(typeof(Line), _lineNum);
                noteType = (NoteType)System.Enum.ToObject(typeof(NoteType), _noteType - 1);
                noteList = new List<Note>();
            }

            public NotePos(float _noteTimeg, int _lineNum, NoteType _noteType) {
                notesTimeg = _noteTimeg;
                lineNum = (Line)System.Enum.ToObject(typeof(Line), _lineNum);
                noteType = _noteType;
                noteList = new List<Note>();
            }

            public NotePos(float _noteTimeg, Line _lineNum, NoteType _noteType, List<Note> _noteList) {
                notesTimeg = _noteTimeg;
                lineNum = _lineNum;
                noteType = _noteType;
                noteList = _noteList;
            }

            public NotePos(float _noteTimeg, int _lineNum, int _noteType, List<Note> _noteList) {
                notesTimeg = _noteTimeg;
                lineNum = (Line)System.Enum.ToObject(typeof(Line), _lineNum);
                noteType = (NoteType)System.Enum.ToObject(typeof(NoteType), _noteType - 1);
                noteList = _noteList;
            }

            public NotePos(float _noteTimeg, int _lineNum, NoteType _noteType, List<Note> _noteList) {
                notesTimeg = _noteTimeg;
                lineNum = (Line)System.Enum.ToObject(typeof(Line), _lineNum);
                noteType = _noteType;
                noteList = _noteList;
            }
        }

        public NotePos notePos { get; private set; }

        public void Initialize(NotePos _notePos) {
            notePos = _notePos;
        }
    }
}

