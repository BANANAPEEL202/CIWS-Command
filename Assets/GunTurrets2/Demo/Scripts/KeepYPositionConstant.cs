using UnityEngine;

public class KeepYPositionConstant : MonoBehaviour
{
    // Reference to the ParticleSystem
    public ParticleSystem particleSystem2;
    // Desired constant y-position for the particles
    public float constantYPosition = -0.39f;

    void Update()
    {
        if (particleSystem2 != null)
        {
            // Get the main module of the ParticleSystem
            var mainModule = particleSystem2.main;

            // Get the current particle positions
            var particles = new ParticleSystem.Particle[particleSystem2.particleCount];
            particleSystem2.GetParticles(particles);

            // Loop through all particles and adjust their y-position
            for (int i = 0; i < particles.Length; i++)
            {
                Vector3 position = particles[i].position;
                position.y = constantYPosition;  // Set the y position to the constant value
                particles[i].position = position;
            }

            // Set the particles back to the ParticleSystem
            particleSystem2.SetParticles(particles, particles.Length);
        }
    }
}
