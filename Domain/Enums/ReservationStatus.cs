using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ReservationStatus
    {
        Pending,
        Confirmed,     // Reserva confirmada y pendiente de recogida
        Cancelled,     // Reserva cancelada por el usuario
        Completed,     // Reserva completada (vehículo devuelto a tiempo)
        Overdue,       // Reserva con devolución tardía
        AutoCancelled, // Reserva cancelada automáticamente (vehículo no disponible)
        Extended       // Reserva extendida
    }

}
