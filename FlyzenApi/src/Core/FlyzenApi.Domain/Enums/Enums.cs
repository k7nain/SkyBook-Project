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
        TripReminder,
        // Appended (not inserted above) so existing stored notification rows'
        // Type values keep their original meaning.
        FlightDepartureReminder,
        FlightDeparted,
        FlightArrived
    }

    public enum FlightStatus
    {
        Scheduled,
        Departed,
        Completed
    }

    // Distinguishes which admin-notification case already fired for a given
    // flight, so FlightNotificationBackgroundService never re-announces the
    // same event twice (see FlightNotificationLog).
    public enum FlightNotificationEvent
    {
        DepartureWithin24Hours,
        DepartureWithin1Hour,
        Departed,
        Arrived
    }

    // Earned: awarded at booking creation (this app has no payment gateway -
    // booking creation already stands in for "paid", see BookingService).
    // Redeemed: spent as a checkout discount.
    // Reversed: an earlier Earned award clawed back because its booking was
    // cancelled - kept distinct from Redeemed so the history never implies
    // the user cashed points in for a discount they didn't actually take.
    public enum SkyPointsTransactionType
    {
        Earned,
        Redeemed,
        Reversed
    }
}
