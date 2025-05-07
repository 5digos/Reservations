using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum ReservationEventType
    {
        Created,                // Se creó la reserva
        Modified,               // Usuario modificó los detalles de la reserva
        Cancelled,              // Usuario canceló la reserva
        PickedUp,               // El vehículo fue retirado
        Returned,               // El vehículo fue devuelto
        Extended,               // Se extendió la reserva
        ReminderSent,           // Se envió recordatorio de vencimiento
        OverdueNotificationSent,// Se notificó devolución tardía
        AutoCancelled,          // Reserva cancelada automáticamente
        Paid                    // Se registró el pago al devolver
    }
}
