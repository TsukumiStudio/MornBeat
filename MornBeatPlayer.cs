using UnityEngine;

namespace MornLib
{
    public sealed class MornBeatPlayer : MonoBehaviour
    {
        [SerializeField] private MornBeatMusic _music;
        [SerializeField] private TextAsset _score;
        [SerializeField] private AudioSource _source;
        private string[] _lines;
        private int _nextReadLineIndex;

        private void Awake()
        {
            _lines = _score.text.Split('\n');
            _nextReadLineIndex = 0;
        }

        private void LateUpdate()
        {
        }
    }
}