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
        Updated,               // Usuario modificó los detalles de la reserva
        Confirmed,            // Reserva confirmada por el usuario
        Cancelled,              // Usuario canceló la reserva
        VehiclePickedUp,               // El vehículo fue retirado
        VehicleReturned,               // El vehículo fue devuelto
        Extended,               // Se extendió la reserva
        ReminderSent,           // Se envió recordatorio de vencimiento
        OverdueNotificationSent,// Se notificó devolución tardía
        AutoCancelled,          // Reserva cancelada automáticamente
        PaymentSucceeded        // Se registró el pago 
    }
}
