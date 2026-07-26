namespace FlyzenApi.Domain.Enums
{
    public enum SeatClass
    {
        Economy,
        Business
    }

    public enum PassengerType
    {
        Adult,
        Child,
        Infant
    }

    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Cancelled
    }

    public enum MealType
    {
        Standard,
        Vegetarian,
        Vegan,
        Halal,
        GlutenFree,
        // Appended (not prepended) so existing MealOption rows' stored int
        // values for Standard..GlutenFree don't shift meaning.
        None
    }

    public enum UserRole
    {
        User,
        Admin
    }

    public enum NotificationType
    {
        ReservationConfirmed,
        PriceChange,
        TripReminder
    }
}
