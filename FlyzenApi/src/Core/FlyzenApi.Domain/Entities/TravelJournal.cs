using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    // Trip-level journal entry (title/body/photos/optional rating), distinct
    // from a single-place star rating - see TravelJournalService for the
    // "Verified traveler" eligibility check (BookingId must belong to the
    // author and its Flight must have actually completed).
    public class TravelJournal : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Proves the author actually traveled - only a Confirmed/Pending (i.e.
        // non-cancelled) booking on a Completed flight is eligible, checked at
        // creation time in TravelJournalService.CreateAsync. Every row in this
        // table is therefore inherently from a "verified traveler" - there's no
        // separate IsVerified flag, the FK's existence is the proof.
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        // Denormalized from Booking.Flight.ArrivalCityId at creation time (the
        // task calls for it as its own field, and it keeps city-based lookups
        // from having to join through Booking->Flight every time).
        public Guid DestinationCityId { get; set; }
        public City DestinationCity { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // Optional - lets a journal double as this app's only star-rating
        // mechanism for Dream Trip places/cities, since no separate Review
        // entity exists yet (see feature plan).
        public int? Rating { get; set; }

        // Defaults to auto-published (no moderation queue yet) - see the
        // feature plan's explicit recommendation. Kept as a real column
        // (not hardcoded true in queries) so an admin-approval flag can be
        // wired in later without a schema change.
        public bool IsPublished { get; set; } = true;

        public ICollection<TravelJournalImage> Images { get; set; } = new List<TravelJournalImage>();
    }
}
