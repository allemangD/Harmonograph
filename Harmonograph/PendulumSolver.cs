using System;
using OpenTK;

namespace Harmonograph
{
    public class PendulumSolver
    {
        public const double Tau = Math.PI * 2;

        public Vector2d Amplitude { get; set; }
        public double Period { get; set; }
        public double Phase { get; set; }
        public double Friction { get; set; }

        public PendulumSolver(Vector2d amplitude, double period, double phase,
            double friction = 0.01)
        {
            Amplitude = amplitude;
            Period = period;
            Phase = phase;
            Friction = friction;
        }

        public Vector2d At(double time)
        {
            var phase = (Phase + Period * time) / Tau;

            return Amplitude * Math.Sin(phase) * Math.Exp(-time * Friction / Tau);
        }
    }
}