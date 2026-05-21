using UnityEngine;
using Vertigo.Wheel.Data;

namespace Vertigo.Wheel.Runtime
{
    public sealed class WheelGameConfigSource : MonoBehaviour
    {
        [SerializeField] private WheelGameSettings _settings;

        public WheelGameSettings Settings { get { return _settings; } }
    }
}
