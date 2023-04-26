using System.Collections.Generic;
using UnityEngine;

namespace BoschingMachine.Logic.Scripts.Player
{
    public static class CursorLock
    {
        public static readonly List<CursorReservation> Reservations = new();
        
        public class CursorReservation
        {
            public readonly CursorLockMode lockMode;
            private bool pushed;

            public CursorReservation(CursorLockMode lockMode)
            {
                this.lockMode = lockMode;
            }

            public void Push()
            {
                if (pushed) return;
                Reservations.Add(this);
                Update();
                pushed = true;
            }
            
            public void Pop()
            {
                if (!pushed) return;
                Reservations.Remove(this);
                Update();
                pushed = false;
            }
        }

        private static void Update()
        {
            Cursor.lockState = Reservations.Count == 0 ? CursorLockMode.None : Reservations[^1].lockMode;
        }
    }
}
