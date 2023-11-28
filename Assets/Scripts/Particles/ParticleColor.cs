using Mirror;
using UnityEngine;

namespace Particles
{
    public class ParticleColor : NetworkBehaviour
    {
        [SerializeField]
        private new ParticleSystem particleSystem;
        
        [SyncVar(hook = nameof(OnColorChanged))] public Color color;

        public void OnColorChanged(Color oldColor, Color newColor)
        {
            var main = particleSystem.main;
            main.startColor = newColor;
        }
    }
}
