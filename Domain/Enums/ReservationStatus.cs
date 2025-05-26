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
        InProgress,    // Reserva en curso (vehículo recogido)
        Completed,      // Reserva pendiente de pago (vehículo entregado)
        Cancelled,     // Reserva cancelada por el usuario
        Paid,     // Reserva completada (pago realizado)
        Overdue,       // Reserva con devolución tardía
        AutoCancelled, // Reserva cancelada automáticamente (vehículo no disponible)
        Extended       // Reserva extendida
    }

}
