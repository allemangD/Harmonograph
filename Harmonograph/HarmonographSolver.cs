using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace Harmonograph
{
    public class HarmonographSolver : IEnumerable<PendulumSolver>
    {
        public const double Tau = Math.PI / 2;

        public List<PendulumSolver> Pendulums { get; } = new List<PendulumSolver>();

        public void Add(PendulumSolver pendulum) => Pendulums.Add(pendulum);

        public Vector2d At(double time)
        {
            return Pendulums.Select(p => p.At(time)).Aggregate(Vector2d.Zero, Vector2d.Add);
        }

        public IEnumerator<PendulumSolver> GetEnumerator()
        {
            return Pendulums.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Pendulums).GetEnumerator();
        }
    }
}