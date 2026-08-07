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
        FlightArrived,
        // Appended (not inserted above), same reason as above. Unlike the
        // FlightDeparted/FlightArrived pair (admin-only alerts), these four are
        // sent to the affected travelers themselves - see
        // FlightNotificationBackgroundService and AdminService.UpdateFlightOperationalStatusAsync.
        CheckInOpen,
        GateChanged,
        FlightDelayed,
        BoardingReminder,
        // Appended (not inserted above), same reason as above. Admin-only, like
        // FlightDeparted/FlightArrived - see DynamicPricingBackgroundService.
        // Deliberately NOT the same as PriceChange (which notifies USERS after a
        // price actually changes) - this fires when a proposal is CREATED, before
        // any admin has approved or rejected it.
        PriceProposalCreated,
        // Appended (not inserted above), same reason as above. Sent to the
        // traveler themselves right after BookingService.CheckInAsync succeeds -
        // see NotificationService.RealTimePushTypes (pushed live, like CheckInOpen).
        CheckInCompleted,
        // Appended (not inserted above), same reason as above. Sent to every
        // traveler whose booking gets auto-cancelled when an admin sets a
        // flight's OperationalStatus to Cancelled - see
        // AdminService.UpdateFlightOperationalStatusAsync.
        FlightCancelled
    }

    public enum PriceProposalStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public enum FlightStatus
    {
        Scheduled,
        Departed,
        Completed
    }

    // Admin-settable operational display status, independent of the automatic
    // Scheduled/Departed/Completed lifecycle above (which FlightNotificationBackgroundService
    // owns based on departure/arrival time). Changing this - or GateNumber - via
    // AdminService.UpdateFlightOperationalStatusAsync notifies affected travelers.
    public enum FlightOperationalStatus
    {
        Normal,
        Delayed,
        Boarding,
        // Appended (not inserted above) so existing stored Flight rows' int
        // values for Normal/Delayed/Boarding keep their original meaning.
        // Unlike those three (informational only), setting this one actually
        // cascades: AdminService.UpdateFlightOperationalStatusAsync closes the
        // flight to search/booking, cancels every active booking on it, and
        // notifies each affected traveler - see NotificationType.FlightCancelled.
        Cancelled
    }

    // Distinguishes which admin-notification case already fired for a given
    // flight, so FlightNotificationBackgroundService never re-announces the
    // same event twice (see FlightNotificationLog).
    public enum FlightNotificationEvent
    {
        DepartureWithin24Hours,
        DepartureWithin1Hour,
        Departed,
        Arrived,
        // Appended (not inserted above), same reason as FlightNotificationEvent's
        // sibling enums. Dedupes the two new automatic traveler-facing checks
        // (see FlightNotificationBackgroundService.ProcessCheckInAsync/ProcessBoardingAsync).
        CheckInOpen,
        BoardingReminder
    }

    // Computed on every GET .../checkin-status call, never persisted - see
    // BookingService.GetCheckInStatusAsync for the window math.
    public enum CheckInAvailability
    {
        // Booking is Cancelled (or soft-deleted) - check-in was never possible.
        NotEligible,
        NotYetOpen,
        Open,
        AlreadyCheckedIn,
        // Past the cutoff (or the flight itself has departed/completed) without
        // ever checking in.
        Closed
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
